using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine.TextCore.Text;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// Add this ability to a character and it'll be able to dash in 2D, covering a certain distance in a certain duration
	///
	/// Animation parameters :
	/// Dashing : true if the character is currently dashing
	/// DashingDirectionX : the x component of the dash direction, normalized
	/// DashingDirectionY : the y component of the dash direction, normalized
	/// </summary>
	[AddComponentMenu("TopDown Engine/Character/Abilities/SurviveDash")]
	public class SurviveDash : CharacterDash2D
	{
		//ダッシュ連続でできる回数 インターバルで回復
		public int DashNum = 1;
		public int CurrentDashNum = 0;

		public bool coolDownChack = false;

		public MMProgressBar dashBar;
		public TextMeshProUGUI dashNumText;
		/// <summary>
		/// On init, we stop our particles, and initialize our dash bar
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			CurrentDashNum = DashNum;
			//Cooldown.Initialization();

			//_mainCamera = Camera.main;

			//if (GUIManager.HasInstance && _character.CharacterType == Character.CharacterTypes.Player)
			//{
			//	GUIManager.Instance.SetDashBar(true, _character.PlayerID);
			//	UpdateDashBar();
			//}
		}

		public void SetDashParameter(int num, float power, float interval)
        {
			DashNum = num;
			CurrentDashNum = DashNum;
			//ダッシュ時間どうするか
			DashDistance = power;
			Cooldown.ConsumptionDuration = 0;
			Cooldown.RefillDuration = interval;
		}
		/// <summary>
		/// Watches for dash inputs
		/// </summary>
		protected override void HandleInput()
		{
            //base.HandleInput();
            if (!AbilityAuthorized
                || (CurrentDashNum <= 0)
                || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
            {
                return;
            }
            if (_inputManager.DashButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
            {
                DashStart();
            }
        }

		/// <summary>
		/// Initiates the dash
		/// </summary>
		public override void DashStart()
		{
			if (_dashing || CurrentDashNum <= 0 /*|| !Cooldown.Ready()*/)
			{
				return;
			}

			//Cooldown.Start();
			_movement.ChangeState(CharacterStates.MovementStates.Dashing);
			_dashing = true;
			_dashTimer = 0f;
			_dashOrigin = this.transform.position;
			_controller.FreeMovement = false;
			DashFeedback?.PlayFeedbacks(this.transform.position);
			PlayAbilityStartFeedbacks();

			if (InvincibleWhileDashing)
			{
				_health.DamageDisabled();
			}

			HandleDashMode();

			CurrentDashNum--;

			//ダッシュしたら無敵に
			this.gameObject.MMGetComponentNoAlloc<SurviveHealth>().ImmuneToDamage = true;



			//Dash関連Skill発動
			DashEvent.Trigger(_controller.CurrentDirection.normalized * DashDistance);
		}

		protected override void HandleDashMode()
		{
			switch (DashMode)
			{
				case DashModes.MainMovement:
					_dashDestination = this.transform.position + _controller.CurrentDirection.normalized * DashDistance;
					break;

				case DashModes.Fixed:
					_dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
					break;

				case DashModes.SecondaryMovement:
					_dashDestination = this.transform.position + (Vector3)_character.LinkedInputManager.SecondaryMovement.normalized * DashDistance;
					break;

				case DashModes.MousePosition:
					_inputPosition = _mainCamera.ScreenToWorldPoint(InputManager.Instance.MousePosition);
					_inputPosition.z = this.transform.position.z;
					_dashDestination = this.transform.position + (_inputPosition - this.transform.position).normalized * DashDistance;
					break;
			}
		}

		/// <summary>
		/// Stops the dash
		/// </summary>
		public override void DashStop()
		{
			DashFeedback?.StopFeedbacks(this.transform.position);

			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();

			if (InvincibleWhileDashing)
			{
				_health.DamageEnabled();
			}

			_movement.ChangeState(CharacterStates.MovementStates.Idle);
			_dashing = false;
			_controller.FreeMovement = true;
			this.gameObject.MMGetComponentNoAlloc<SurviveHealth>().ImmuneToDamage = false;


			//Dash関連Skill発動
			DashEndEvent.Trigger();
		}

		/// <summary>
		/// On update, moves the character if needed
		/// </summary>
		public override void ProcessAbility()
		{
			//base.ProcessAbility();

			if (_dashing)
			{
				if (_dashTimer < DashDuration)
				{
					_dashAnimParameterDirection = (_dashDestination - _dashOrigin).normalized;
					if (DashSpace == DashSpaces.World)
					{
						_newPosition = Vector3.Lerp(_dashOrigin, _dashDestination, DashCurve.Evaluate(_dashTimer / DashDuration));
						_dashTimer += Time.deltaTime;
						_controller.MovePosition(_newPosition);
					}
					else
					{
						_oldPosition = _dashTimer == 0 ? _dashOrigin : _newPosition;
						_newPosition = Vector3.Lerp(_dashOrigin, _dashDestination, DashCurve.Evaluate(_dashTimer / DashDuration));
						_dashTimer += Time.deltaTime;
						_controller.MovePosition(this.transform.position + _newPosition - _oldPosition);
					}
				}
				else
				{
					DashStop();
				}
            }
            else
            {
				Cooldown.Update();
				if(coolDownChack && Cooldown.Ready())
                {
					coolDownChack = false;
					if(CurrentDashNum < DashNum)
                    {
						CurrentDashNum++;
                        _character.CharacterModel.GetComponent<Animator>().SetTrigger("DashCharge");


                    }
                }
				if(Cooldown.CooldownState == MMCooldown.CooldownStates.Idle)
                {
					if(CurrentDashNum < DashNum)
                    {
						Cooldown.Start();
						coolDownChack = true;
					}
                }
			}
			UpdateDashBar();

			//メインUI上のものも更新
			SurviveGUIManager.GetInstance().UpdateDashCount(CurrentDashNum, DashNum);
		}

		/// <summary>
		/// Updates the GUI jetpack bar.
		/// </summary>
		protected override void UpdateDashBar()
		{
			if(dashBar!=null)
            {
				if(Cooldown.CooldownState == MMCooldown.CooldownStates.Idle)
                {
					dashBar.UpdateBar(Cooldown.RefillDuration, 0f, Cooldown.RefillDuration);

                }
                else {
				dashBar.UpdateBar(Cooldown.CurrentDurationLeft, 0f, Cooldown.RefillDuration);
				}
				//if (/*CurrentDashNum == DashNum && */Cooldown.CurrentDurationLeft == Cooldown.ConsumptionDuration)
				//            {
				//	dashBar.HideBar(0f);
				//            }
				//            else
				//            {
				//	dashBar.ShowBar();
				//            }


			}
			if (dashNumText != null)
			{
				dashNumText.text = CurrentDashNum.ToString();
			}
			if ((GUIManager.HasInstance) && (_character.CharacterType == Character.CharacterTypes.Player))
			{
				GUIManager.Instance.UpdateDashBars(Cooldown.CurrentDurationLeft, 0f, Cooldown.ConsumptionDuration, _character.PlayerID);
			}
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter(_dashingAnimationParameterName, AnimatorControllerParameterType.Bool, out _dashingAnimationParameter);
			RegisterAnimatorParameter(_dashingDirectionXAnimationParameterName, AnimatorControllerParameterType.Float, out _dashingDirectionXAnimationParameter);
			RegisterAnimatorParameter(_dashingDirectionYAnimationParameterName, AnimatorControllerParameterType.Float, out _dashingDirectionYAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, we send our Running status to the character's animator
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _dashingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Dashing), _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _dashingDirectionXAnimationParameter, _dashAnimParameterDirection.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _dashingDirectionYAnimationParameter, _dashAnimParameterDirection.y, _character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}