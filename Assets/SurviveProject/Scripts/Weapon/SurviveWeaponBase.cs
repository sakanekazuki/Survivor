using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Random = UnityEngine.Random;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// A weapon class aimed specifically at allowing the creation of various projectile weapons, from shotgun to machine gun, via plasma gun or rocket launcher
	/// </summary>
	[AddComponentMenu("TopDown Engine/Weapons/SurviveWeponBase")]
	public class SurviveWeaponBase : Weapon
	{
		//武器のパラメータを保持する　予定
		protected bool IsBurstShot = true;
		public class WeaponParameter
        {
			//武器自体の攻撃力　（倍率?）
			
			//
        }
		protected WeaponParameter weaponParameter;
		protected AbilityData.WeaponLevelData currentData;
		protected GameDefine.StatusAttribute currentAttribute;
		protected int shootCount = 0;
		protected SurvivePlayer exOwner;

		public bool main = false;


		public List<SubData> subDatas = new List<SubData>();

        public Guid ID { get; private set; }

		public int AbilityID = 0;
        public class SubData
        {
			public bool active = false;
			public string weaponID;

			public delegate void HitCallBackDelegate(GameObject target);
			public HitCallBackDelegate hitCallback;
			public delegate void ShotCallBackDeletgate(GameObject targe);
			public HitCallBackDelegate shotCallback;
		}



		public virtual void SetParameter(AbilityData.WeaponLevelData data, GameDefine.StatusAttribute attribute, int id)
        {
			currentData = data;
			currentAttribute = attribute;
			AbilityID = id;
			if (IsBurstShot)
			{
				BurstLength = currentData.Num + (Owner!= null?(Owner as SurvivePlayer).battleParameter.GetThrowNum(AbilityID): 0);//対応していないやつとかチェック必須
																				 //バーストの間隔の設定は未定
			}
			else
			{
				//バーストではないパターンはこれ系
				(this as SurviveWeaponProjectile).ProjectilesPerShot = currentData.Num + (Owner != null ? (Owner as SurvivePlayer).battleParameter.GetThrowNum(AbilityID) : 0);//対応していないやつとかチェック必須
			}
			//var PermanentEffects = (Owner as SurvivePlayer).CalcPermanentDataStatus();

			////最低限のスピードは持たせる　要デバッグ
			//TimeBetweenUses = Mathf.Max(currentData.Interval - (currentData.Interval * PermanentEffects[(int)SurviveProgressManager.PermanentEffect.AttackInterval] / 100f), currentData.Interval*0.1f);

		}
		public virtual void UpdateParameter()
		{
			//キャラステータス変更に合わせて更新入れたり

			if (IsBurstShot)
			{
				BurstLength = currentData.Num + (Owner as SurvivePlayer).battleParameter.GetThrowNum(AbilityID);//対応していないやつとかチェック必須
																				 //バーストの間隔の設定は未定
			}
			else
			{
				//バーストではないパターンはこれ系
				(this as SurviveWeaponProjectile).ProjectilesPerShot = currentData.Num + (Owner as SurvivePlayer).battleParameter.GetThrowNum(AbilityID);//対応していないやつとかチェック必須
			}
			var PermanentEffects = (Owner as SurvivePlayer).CalcPermanentDataStatus();

			//最低限のスピードは持たせる　要デバッグ
			TimeBetweenUses = Mathf.Max(currentData.Interval * (Owner as SurvivePlayer).battleParameter.GetSkillInterval(main), currentData.Interval * 0.1f);

		}
		/// <summary>
		/// On start we initialize our weapon
		/// </summary>
		protected override void Start()
		{
			if (InitializeOnStart)
			{
				Initialization();
			}
		}

		/// <summary>
		/// Initialize this weapon.
		/// </summary>
		public override void Initialization()
		{
			Flipped = false;
			_spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
			_comboWeapon = this.gameObject.GetComponent<ComboWeapon>();
			_weaponPreventShooting = this.gameObject.GetComponent<WeaponPreventShooting>();

			WeaponState = new MMStateMachine<WeaponStates>(gameObject, true);
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
			WeaponAmmo = GetComponent<WeaponAmmo>();
			_animatorParameters = new List<HashSet<int>>();

			ID = Guid.NewGuid();
			//_weaponAim = GetComponent<WeaponAim>();

			//キャラが持つように
			//初期化順が怪しいかも
			if (Owner != null)
			{
				_weaponAim = (Owner as SurvivePlayer).characterAim;
			}
			InitializeAnimatorParameters();
			if (WeaponAmmo == null)
			{
				CurrentAmmoLoaded = MagazineSize;
			}
			InitializeFeedbacks();

			shootCount = 0;
		}

		protected override void InitializeFeedbacks()
		{
			WeaponStartMMFeedback?.Initialization(this.gameObject);
			WeaponUsedMMFeedback?.Initialization(this.gameObject);
			WeaponUsedMMFeedbackAlt?.Initialization(this.gameObject);
			WeaponStopMMFeedback?.Initialization(this.gameObject);
			WeaponReloadNeededMMFeedback?.Initialization(this.gameObject);
			WeaponReloadMMFeedback?.Initialization(this.gameObject);
		}

		/// <summary>
		/// Initializes the combo weapon, if it's one
		/// </summary>
		public override void InitializeComboWeapons()
		{
			if (_comboWeapon != null)
			{
				_comboWeapon.Initialization();
			}
		}

		/// <summary>
		/// Sets the weapon's owner
		/// </summary>
		/// <param name="newOwner">New owner.</param>
		public override void SetOwner(Character newOwner, CharacterHandleWeapon handleWeapon)
		{
			Owner = newOwner;
			if (Owner != null && handleWeapon != null)
			{
				CharacterHandleWeapon = handleWeapon;
				_characterMovement = Owner.GetComponent<Character>()?.FindAbility<CharacterMovement>();
				_controller = Owner.GetComponent<TopDownController>();

				_controllerIs3D = Owner.GetComponent<TopDownController3D>() != null;

				if (CharacterHandleWeapon.AutomaticallyBindAnimator)
				{
					if (CharacterHandleWeapon.CharacterAnimator != null)
					{
						_ownerAnimator = CharacterHandleWeapon.CharacterAnimator;
					}
					if (_ownerAnimator == null)
					{
						_ownerAnimator = CharacterHandleWeapon.gameObject.GetComponentInParent<Character>().CharacterAnimator;
					}
					if (_ownerAnimator == null)
					{
						_ownerAnimator = CharacterHandleWeapon.gameObject.GetComponentInParent<Animator>();
					}
				}
			}
		}

		/// <summary>
		/// Called by input, turns the weapon on
		/// </summary>
		public override void WeaponInputStart()
		{
			if (_reloading)
			{
				return;
			}

			if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
			{
				_triggerReleased = false;
				TurnWeaponOn();
			}
		}

		/// <summary>
		/// Describes what happens when the weapon starts
		/// </summary>
		protected override void TurnWeaponOn()
		{
			TriggerWeaponStartFeedback();
			WeaponState.ChangeState(WeaponStates.WeaponStart);
			if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
			{
				//_movementMultiplierStorage = _characterMovement.MovementSpeedMultiplier;
				//_characterMovement.MovementSpeedMultiplier = MovementMultiplier;
			}
			if (_comboWeapon != null)
			{
				_comboWeapon.WeaponStarted(this);
			}
			if (PreventAllMovementWhileInUse && (_characterMovement != null) && (_controller != null))
			{
				_characterMovement.SetMovement(Vector2.zero);
				_characterMovement.MovementForbidden = true;
			}
			if (PreventAllAimWhileInUse && (_weaponAim != null))
			{
				_weaponAim.enabled = false;
			}
		}

		/// <summary>
		/// On Update, we check if the weapon is or should be used
		/// </summary>
		protected override void Update()
		{
			FlipWeapon();
			ApplyOffset();
		}

		/// <summary>
		/// On LateUpdate, processes the weapon state
		/// </summary>
		protected override void LateUpdate()
		{
			ProcessWeaponState();
		}

		/// <summary>
		/// Called every lastUpdate, processes the weapon's state machine
		/// </summary>
		protected override void ProcessWeaponState()
		{
			if (WeaponState == null) { return; }

			UpdateAnimator();

			switch (WeaponState.CurrentState)
			{
				case WeaponStates.WeaponIdle:
					CaseWeaponIdle();
					break;

				case WeaponStates.WeaponStart:
					CaseWeaponStart();
					break;

				case WeaponStates.WeaponDelayBeforeUse:
					CaseWeaponDelayBeforeUse();
					break;

				case WeaponStates.WeaponUse:
					CaseWeaponUse();
					break;

				case WeaponStates.WeaponDelayBetweenUses:
					CaseWeaponDelayBetweenUses();
					break;

				case WeaponStates.WeaponStop:
					CaseWeaponStop();
					break;

				case WeaponStates.WeaponReloadNeeded:
					CaseWeaponReloadNeeded();
					break;

				case WeaponStates.WeaponReloadStart:
					CaseWeaponReloadStart();
					break;

				case WeaponStates.WeaponReload:
					CaseWeaponReload();
					break;

				case WeaponStates.WeaponReloadStop:
					CaseWeaponReloadStop();
					break;

				case WeaponStates.WeaponInterrupted:
					CaseWeaponInterrupted();
					break;
			}
		}

		/// <summary>
		/// If the weapon is idle, we reset the movement multiplier
		/// </summary>
		public override void CaseWeaponIdle()
		{
			ResetMovementMultiplier();
		}

		/// <summary>
		/// When the weapon starts we switch to a delay or shoot based on our weapon's settings
		/// </summary>
		public override void CaseWeaponStart()
		{
			if (DelayBeforeUse > 0)
			{
				_delayBeforeUseCounter = DelayBeforeUse;
				WeaponState.ChangeState(WeaponStates.WeaponDelayBeforeUse);
			}
			else
			{
				StartCoroutine(ShootRequestCo());
			}
		}

		/// <summary>
		/// If we're in delay before use, we wait until our delay is passed and then request a shoot
		/// </summary>
		public override void CaseWeaponDelayBeforeUse()
		{
			_delayBeforeUseCounter -= Time.deltaTime;
			if (_delayBeforeUseCounter <= 0)
			{
				StartCoroutine(ShootRequestCo());
			}
		}

		/// <summary>
		/// On weapon use we use our weapon then switch to delay between uses
		/// </summary>
		public override void CaseWeaponUse()
		{
			WeaponUse();
			_delayBetweenUsesCounter = TimeBetweenUses;
			WeaponState.ChangeState(WeaponStates.WeaponDelayBetweenUses);
		}

		/// <summary>
		/// When in delay between uses, we either turn our weapon off or make a shoot request
		/// </summary>
		public override void CaseWeaponDelayBetweenUses()
		{
			if (_triggerReleased && TimeBetweenUsesReleaseInterruption)
			{
				TurnWeaponOff();
				return;
			}

			_delayBetweenUsesCounter -= Time.deltaTime;
			if (_delayBetweenUsesCounter <= 0)
			{
				if ((TriggerMode == TriggerModes.Auto) && !_triggerReleased)
				{
					StartCoroutine(ShootRequestCo());
				}
				else
				{
					TurnWeaponOff();
				}
			}
		}

		/// <summary>
		/// On weapon stop, we switch to idle
		/// </summary>
		public override void CaseWeaponStop()
		{
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
		}

		/// <summary>
		/// If a reload is needed, we mention it and switch to idle
		/// </summary>
		public override void CaseWeaponReloadNeeded()
		{
			ReloadNeeded();
			ResetMovementMultiplier();
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
		}

		/// <summary>
		/// on reload start, we reload the weapon and switch to reload
		/// </summary>
		public override void CaseWeaponReloadStart()
		{
			ReloadWeapon();
			_reloadingCounter = ReloadTime;
			WeaponState.ChangeState(WeaponStates.WeaponReload);
		}

		/// <summary>
		/// on reload, we reset our movement multiplier, and switch to reload stop once our reload delay has passed
		/// </summary>
		public override void CaseWeaponReload()
		{
			ResetMovementMultiplier();
			_reloadingCounter -= Time.deltaTime;
			if (_reloadingCounter <= 0)
			{
				WeaponState.ChangeState(WeaponStates.WeaponReloadStop);
			}
		}

		/// <summary>
		/// on reload stop, we swtich to idle and load our ammo
		/// </summary>
		public override void CaseWeaponReloadStop()
		{
			_reloading = false;
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
			if (WeaponAmmo == null)
			{
				CurrentAmmoLoaded = MagazineSize;
			}
		}

		/// <summary>
		/// on weapon interrupted, we turn our weapon off and switch back to idle
		/// </summary>
		public override void CaseWeaponInterrupted()
		{
			TurnWeaponOff();
			ResetMovementMultiplier();
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
		}

		/// <summary>
		/// Call this method to interrupt the weapon
		/// </summary>
		public override void Interrupt()
		{
			if (Interruptable)
			{
				WeaponState.ChangeState(WeaponStates.WeaponInterrupted);
			}
		}

		public void ResetdelayBetweenUsesCounter()
        {
			_delayBetweenUsesCounter = -1;
			_lastShootRequestAt = -1;
		}


		/// <summary>
		/// Determines whether or not the weapon can fire
		/// </summary>
		public override IEnumerator ShootRequestCo()
		{
			if (Time.time - _lastShootRequestAt < TimeBetweenUses)
			{
				yield break;
			}

			int remainingShots = UseBurstMode ? BurstLength : 1;
			float interval = UseBurstMode ? BurstTimeBetweenShots : 1;

			while (remainingShots > 0)
			{
				ShootRequest();
				_lastShootRequestAt = Time.time;
				remainingShots--;
				yield return MMCoroutine.WaitFor(interval);
			}
		}

		public override void ShootRequest()
		{
			// if we have a weapon ammo component, we determine if we have enough ammunition to shoot
			if (_reloading)
			{
				return;
			}

			if (_weaponPreventShooting != null)
			{
				if (!_weaponPreventShooting.ShootingAllowed())
				{
					return;
				}
			}

			if (MagazineBased)
			{
				if (WeaponAmmo != null)
				{
					if (WeaponAmmo.EnoughAmmoToFire())
					{
						WeaponState.ChangeState(WeaponStates.WeaponUse);
					}
					else
					{
						if (AutoReload && MagazineBased)
						{
							InitiateReloadWeapon();
						}
						else
						{
							WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
						}
					}
				}
				else
				{
					if (CurrentAmmoLoaded > 0)
					{
						WeaponState.ChangeState(WeaponStates.WeaponUse);
						CurrentAmmoLoaded -= AmmoConsumedPerShot;
					}
					else
					{
						if (AutoReload)
						{
							InitiateReloadWeapon();
						}
						else
						{
							WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
						}
					}
				}
			}
			else
			{
				if (WeaponAmmo != null)
				{
					if (WeaponAmmo.EnoughAmmoToFire())
					{
						WeaponState.ChangeState(WeaponStates.WeaponUse);
					}
					else
					{
						WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
					}
				}
				else
				{
					WeaponState.ChangeState(WeaponStates.WeaponUse);
				}
			}
		}

		/// <summary>
		/// When the weapon is used, plays the corresponding sound
		/// </summary>
		public override void WeaponUse()
		{
			// apply recoil
			if ((RecoilForce != 0f) && (_controller != null))
			{
				if (Owner != null)
				{
					if (!_controllerIs3D)
					{
						if (Flipped)
						{
							_controller.Impact(this.transform.right, RecoilForce);
						}
						else
						{
							_controller.Impact(-this.transform.right, RecoilForce);
						}
					}
					else
					{
						_controller.Impact(-this.transform.forward, RecoilForce);
					}
				}
			}
			TriggerWeaponUsedFeedback();

			if(main)
            {
				MainWeaponShotEvent.Trigger(this);
			}
		}

		/// <summary>
		/// Called by input, turns the weapon off if in auto mode
		/// </summary>
		public override void WeaponInputStop()
		{
			if (_reloading)
			{
				return;
			}
			_triggerReleased = true;
			if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
			{
				//_characterMovement.MovementSpeedMultiplier = _movementMultiplierStorage;
				//_movementMultiplierStorage = 1f;
			}
		}

		/// <summary>
		/// Turns the weapon off.
		/// </summary>
		public override void TurnWeaponOff()
		{
			if ((WeaponState.CurrentState == WeaponStates.WeaponIdle || WeaponState.CurrentState == WeaponStates.WeaponStop))
			{
				return;
			}
			_triggerReleased = true;

			TriggerWeaponStopFeedback();
			WeaponState.ChangeState(WeaponStates.WeaponStop);
			ResetMovementMultiplier();
			if (_comboWeapon != null)
			{
				_comboWeapon.WeaponStopped(this);
			}
			if (PreventAllMovementWhileInUse && (_characterMovement != null))
			{
				_characterMovement.MovementForbidden = false;
			}
			if (PreventAllAimWhileInUse && (_weaponAim != null))
			{
				_weaponAim.enabled = true;
			}

			if (NoInputReload)
			{
				bool needToReload = false;
				if (WeaponAmmo != null)
				{
					needToReload = !WeaponAmmo.EnoughAmmoToFire();
				}
				else
				{
					needToReload = (CurrentAmmoLoaded <= 0);
				}

				if (needToReload)
				{
					InitiateReloadWeapon();
				}
			}
		}

		protected override void ResetMovementMultiplier()
		{
			if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
			{
				//_characterMovement.MovementSpeedMultiplier = _movementMultiplierStorage;
				//_movementMultiplierStorage = 1f;
			}
		}

		/// <summary>
		/// Describes what happens when the weapon needs a reload
		/// </summary>
		public override void ReloadNeeded()
		{
			TriggerWeaponReloadNeededFeedback();
		}

		/// <summary>
		/// Initiates a reload
		/// </summary>
		public override void InitiateReloadWeapon()
		{
			// if we're already reloading, we do nothing and exit
			if (_reloading || !MagazineBased)
			{
				return;
			}
			if (PreventAllMovementWhileInUse && (_characterMovement != null))
			{
				_characterMovement.MovementForbidden = false;
			}
			if (PreventAllAimWhileInUse && (_weaponAim != null))
			{
				_weaponAim.enabled = true;
			}
			WeaponState.ChangeState(WeaponStates.WeaponReloadStart);
			_reloading = true;
		}

		/// <summary>
		/// Reloads the weapon
		/// </summary>
		/// <param name="ammo">Ammo.</param>
		protected override void ReloadWeapon()
		{
			if (MagazineBased)
			{
				TriggerWeaponReloadFeedback();
			}
		}

		/// <summary>
		/// Flips the weapon.
		/// </summary>
		public override void FlipWeapon()
		{
			if (Owner == null)
			{
				return;
			}

			if (Owner.Orientation2D == null)
			{
				return;
			}

			if (FlipWeaponOnCharacterFlip)
			{
				Flipped = !Owner.Orientation2D.IsFacingRight;
				if (_spriteRenderer != null)
				{
					_spriteRenderer.flipX = Flipped;
				}
				else
				{
					transform.localScale = Flipped ? LeftFacingFlipValue : RightFacingFlipValue;
				}
			}

			if (_comboWeapon != null)
			{
				_comboWeapon.FlipUnusedWeapons();
			}
		}

		/// <summary>
		/// Destroys the weapon
		/// </summary>
		/// <returns>The destruction.</returns>
		public override IEnumerator WeaponDestruction()
		{
			yield return new WaitForSeconds(AutoDestroyWhenEmptyDelay);
			// if we don't have ammo anymore, and need to destroy our weapon, we do it
			TurnWeaponOff();
			Destroy(this.gameObject);

			if (WeaponID != null)
			{
				// we remove it from the inventory
				List<int> weaponList = Owner.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterInventory>().WeaponInventory.InventoryContains(WeaponID);
				if (weaponList.Count > 0)
				{
					Owner.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterInventory>().WeaponInventory.DestroyItem(weaponList[0]);
				}
			}
		}

		/// <summary>
		/// Applies the offset specified in the inspector
		/// </summary>
		public override void ApplyOffset()
		{

			if (!WeaponCurrentlyActive)
			{
				return;
			}

			_weaponAttachmentOffset = WeaponAttachmentOffset;

			if (Owner == null)
			{
				return;
			}

			if (Owner.Orientation2D != null)
			{
				if (Flipped)
				{
					_weaponAttachmentOffset.x = -WeaponAttachmentOffset.x;
				}

				// we apply the offset
				if (transform.parent != null)
				{
					_weaponOffset = transform.parent.position + _weaponAttachmentOffset;
					transform.position = _weaponOffset;
				}
			}
			else
			{
				if (transform.parent != null)
				{
					_weaponOffset = _weaponAttachmentOffset;
					transform.localPosition = _weaponOffset;
				}
			}
		}

		/// <summary>
		/// Plays the weapon's start sound
		/// </summary>
		protected override void TriggerWeaponStartFeedback()
		{
			WeaponStartMMFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Plays the weapon's used sound
		/// </summary>
		protected override void TriggerWeaponUsedFeedback()
		{
			if (WeaponUsedMMFeedbackAlt != null)
			{
				int random = MMMaths.RollADice(2);
				if (random > 1)
				{
					WeaponUsedMMFeedbackAlt?.PlayFeedbacks(this.transform.position);
				}
				else
				{
					WeaponUsedMMFeedback?.PlayFeedbacks(this.transform.position);
				}
			}
			else
			{
				WeaponUsedMMFeedback?.PlayFeedbacks(this.transform.position);
			}

		}

		/// <summary>
		/// Plays the weapon's stop sound
		/// </summary>
		protected override void TriggerWeaponStopFeedback()
		{
			WeaponStopMMFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Plays the weapon's reload needed sound
		/// </summary>
		protected override void TriggerWeaponReloadNeededFeedback()
		{
			WeaponReloadNeededMMFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Plays the weapon's reload sound
		/// </summary>
		protected override void TriggerWeaponReloadFeedback()
		{
			WeaponReloadMMFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		public override void InitializeAnimatorParameters()
		{
			if (Animators.Count > 0)
			{
				for (int i = 0; i < Animators.Count; i++)
				{
					_animatorParameters.Add(new HashSet<int>());
					AddParametersToAnimator(Animators[i], _animatorParameters[i]);
					if (!PerformAnimatorSanityChecks)
					{
						Animators[i].logWarnings = false;
					}
				}
			}

			if (_ownerAnimator != null)
			{
				_ownerAnimatorParameters = new HashSet<int>();
				AddParametersToAnimator(_ownerAnimator, _ownerAnimatorParameters);
				if (!PerformAnimatorSanityChecks)
				{
					_ownerAnimator.logWarnings = false;
				}
			}
		}

		protected override void AddParametersToAnimator(Animator animator, HashSet<int> list)
		{
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, WeaponAngleAnimationParameter, out _weaponAngleAnimationParameter, AnimatorControllerParameterType.Float, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, WeaponAngleRelativeAnimationParameter, out _weaponAngleRelativeAnimationParameter, AnimatorControllerParameterType.Float, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, IdleAnimationParameter, out _idleAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, StartAnimationParameter, out _startAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, DelayBeforeUseAnimationParameter, out _delayBeforeUseAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, DelayBetweenUsesAnimationParameter, out _delayBetweenUsesAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, StopAnimationParameter, out _stopAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadStartAnimationParameter, out _reloadStartAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadStopAnimationParameter, out _reloadStopAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadAnimationParameter, out _reloadAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, SingleUseAnimationParameter, out _singleUseAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, UseAnimationParameter, out _useAnimationParameter, AnimatorControllerParameterType.Bool, list);

			if (_comboWeapon != null)
			{
				MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, _comboWeapon.ComboInProgressAnimationParameter, out _comboInProgressAnimationParameter, AnimatorControllerParameterType.Bool, list);
			}
		}

		/// <summary>
		/// Override this to send parameters to the character's animator. This is called once per cycle, by the Character
		/// class, after Early, normal and Late process().
		/// </summary>
		public override void UpdateAnimator()
		{
			for (int i = 0; i < Animators.Count; i++)
			{
				UpdateAnimator(Animators[i], _animatorParameters[i]);
			}

			if ((_ownerAnimator != null) && (WeaponState != null) && (_ownerAnimatorParameters != null))
			{
				UpdateAnimator(_ownerAnimator, _ownerAnimatorParameters);
			}
		}

		protected override void UpdateAnimator(Animator animator, HashSet<int> list)
		{
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _idleAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _startAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponStart), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _delayBeforeUseAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _useAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse || WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse || WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _singleUseAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _delayBetweenUsesAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _stopAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponStop), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadStartAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadStopAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop), list, PerformAnimatorSanityChecks);

			if (Owner != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(animator, _aliveAnimationParameter, (Owner.ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead), list, PerformAnimatorSanityChecks);
			}

			if (_weaponAim != null)
			{
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleAnimationParameter, _weaponAim.CurrentAngle, list, PerformAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleRelativeAnimationParameter, _weaponAim.CurrentAngleRelative, list, PerformAnimatorSanityChecks);
			}
			else
			{
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleAnimationParameter, 0f, list, PerformAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleRelativeAnimationParameter, 0f, list, PerformAnimatorSanityChecks);
			}

			if (_comboWeapon != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(animator, _comboInProgressAnimationParameter, _comboWeapon.ComboInProgress, list, PerformAnimatorSanityChecks);
			}
		}




		protected virtual void SetBulletData(SurviveProjectile projectile)
        {
			if(projectile == null)
            {
				return;
            }
			//パラメータを設定する

			//ショップレベルデータ
			var PermanentEffects =(Owner as SurvivePlayer).CalcPermanentDataStatus();

			//ダメージのブレに関してどうするか　　検討（多分特に扱わない）
			//武器パラメータ、キャラ、キャラ強化、等参照
			//一旦ダメージだけ反映してみる(キャラステそのまま反映)
			//parameter
			float damage = (Owner as SurvivePlayer).battleParameter.ATTACK * (currentData.DamageRate / 100f);
			damage = Mathf.Floor(damage);//繰り上げ
			projectile.SetDamage(damage, damage);
			projectile.Speed = currentData.Speed  + (currentData.Speed * PermanentEffects[(int)SurviveProgressManager.PermanentEffect.ItemFlySpeed] / 100f);

			var size = currentData.Size * (Owner as SurvivePlayer).battleParameter.GetSkillArea(main);
			projectile.transform.localScale = new Vector3(size, size, 1);


			//間隔が三か所で設定してしまっているので注意
			//最低限のスピードは持たせる　要デバッグ
			TimeBetweenUses = Mathf.Max(currentData.Interval * (Owner as SurvivePlayer).battleParameter.GetSkillInterval(main), currentData.Interval * 0.1f);

			/*currentData.Num;*/
		
			projectile.LifeTime = currentData.Time + (currentData.Time * PermanentEffects[(int)SurviveProgressManager.PermanentEffect.LastTime] / 100f);
			
			
			//こっちで生存時間後のDestroyを設定する　（projectile側のenable時invokeを行わない）
			//各武器のonenable確認必要
			projectile.Invoke("Destroy", projectile.LifeTime);
			
			//これもアクセスが
			if (currentData.Penetration > 0)
            {
				if (projectile.GetHealth() != null)
				{
					projectile.GetHealth().MaximumHealth = currentData.Penetration;
					projectile.GetHealth().InitialHealth = currentData.Penetration;
					projectile.GetHealth().InitializeCurrentHealth();
				}
				//壁当たり　（貫通するとき別途対応）
				projectile.GetDamageOnTouch().DamageTakenNonDamageable = currentData.Penetration;
				//敵当たり　（貫通するとき別途対応）
				projectile.GetDamageOnTouch().DamageTakenDamageable = 1;
				//これは0
				projectile.GetDamageOnTouch().DamageTakenEveryTime = 0;
			}
            else
			{
				if (projectile.GetHealth() != null)
				{
					//当たっても消えない
					projectile.GetHealth().MaximumHealth = 999999;
					projectile.GetHealth().InitialHealth = 999999;
					projectile.GetHealth().InitializeCurrentHealth();
				}
				//壁当たり　（貫通するとき別途対応）
				projectile.GetDamageOnTouch().DamageTakenNonDamageable = 0;
				//敵当たり　（貫通するとき別途対応）
				projectile.GetDamageOnTouch().DamageTakenDamageable = 0;
				//これは0
				projectile.GetDamageOnTouch().DamageTakenEveryTime = 0;
			}
			

			if(currentData.Knockback > 0f)
            {
				projectile.GetDamageOnTouch().DamageCausedKnockbackType = DamageOnTouch.KnockbackStyles.AddForce;
				projectile.GetDamageOnTouch().DamageCausedKnockbackDirection = DamageOnTouch.KnockbackDirections.BasedOnScriptDirection;

				projectile.GetDamageOnTouch().DamageCausedKnockbackForce = new Vector3(currentData.Knockback*(Owner as SurvivePlayer).battleParameter.KnockBack, 0 , 0 );

			}
            else
            {
				projectile.GetDamageOnTouch().DamageCausedKnockbackType = DamageOnTouch.KnockbackStyles.NoKnockback;
			}

			if (currentData != null && currentData.exWeaponStatuses != null && currentData.exWeaponStatuses.Count > 0)
			{
				(projectile.GetDamageOnTouch() as SurviveDamageOnTouch).SetDamageAttribute(SurviveDamageOnTouch.DamageAttribute.NONE, 0f);
				foreach (var ex in currentData.exWeaponStatuses)
				{
					if (ex.exWeaponStatus == AbilityData.ExWeaponStatusData.ExWeaponStatus.STUN)
					{
						if(ex.value2 > 0)
                        {
							if(Random.Range(0, 100) < ex.value2)
                            {
								(projectile.GetDamageOnTouch() as SurviveDamageOnTouch).SetDamageAttribute(SurviveDamageOnTouch.DamageAttribute.STUN, ex.value1 / 100f);
							}
						}
                        else
                        {
							(projectile.GetDamageOnTouch() as SurviveDamageOnTouch).SetDamageAttribute(SurviveDamageOnTouch.DamageAttribute.STUN, ex.value1 / 100f);
						}
					}
					else if (ex.exWeaponStatus == AbilityData.ExWeaponStatusData.ExWeaponStatus.SLOW)
					{
						(projectile.GetDamageOnTouch() as SurviveDamageOnTouch).SetDamageAttribute(SurviveDamageOnTouch.DamageAttribute.SLOW, projectile.GetDamageOnTouch().InvincibilityDuration);
					}
				}
            }
            else
            {
				(projectile.GetDamageOnTouch() as SurviveDamageOnTouch).SetDamageAttribute(SurviveDamageOnTouch.DamageAttribute.NONE, 0f);
			}
			//対象の武器の種類がバーストタイプか同時か
			//バーストで対応してみる
			if (IsBurstShot)
            {
				BurstLength = currentData.Num + (Owner as SurvivePlayer).battleParameter.GetThrowNum(AbilityID);//対応していないやつとかチェック必須
				//バーストの間隔の設定は未定
			}
			else
            {
				//バーストではないパターンはこれ系 継承先で何とかする
				if (this is SurviveWeaponProjectile)
				{
					(this as SurviveWeaponProjectile).ProjectilesPerShot = currentData.Num + (Owner as SurvivePlayer).battleParameter.GetThrowNum(AbilityID);//対応していないやつとかチェック必須
				}
			}

		}

		protected virtual void SetExBulletData(SurviveProjectile projectile, int id)
		{
			if (projectile == null)
			{
				return;
			}
			if(currentData.exWeaponStatuses == null || currentData.exWeaponStatuses.Count <= id)
            {
				return;
            }
			var data = currentData.exWeaponStatuses[id].exStatus;
			//パラメータを設定する



			//ダメージのブレに関してどうするか　　検討（多分特に扱わない）
			//武器パラメータ、キャラ、キャラ強化、等参照
			//一旦ダメージだけ反映してみる(キャラステそのまま反映)
			//parameter

			var owner = (Owner != null) ? (Owner as SurvivePlayer) : exOwner;
			//無理やり
			if(Owner == null)
            {
				projectile.gameObject.MMFGetComponentNoAlloc<SurviveDamageOnTouch>().Owner = owner.gameObject;

			}
			float damage = owner.battleParameter.ATTACK * (data.DamageRate / 100f);
			damage = Mathf.Floor(damage);//繰り上げ
			projectile.SetDamage(damage, damage);
			projectile.Speed = data.Speed;
			projectile.transform.localScale = new Vector3(data.Size, data.Size, 1);
			var PermanentEffects = owner.CalcPermanentDataStatus();
			TimeBetweenUses = Mathf.Max(currentData.Interval * (owner as SurvivePlayer).battleParameter.GetSkillInterval(main), data.Interval * 0.1f);

			/*currentData.Num;*/
			//ノックバックはあとで　（弾継承しないとアクセス作れないや）
			//projectile. currentData.Knockback
			projectile.LifeTime = data.Time;

			//projectile.LifeTime = currentData.Time + (currentData.Time * PermanentEffects[(int)SurviveProgressManager.PermanentEffect.LastTime] / 100f);


			//こっちで生存時間後のDestroyを設定する　（projectile側のenable時invokeを行わない）
			//各武器のonenable確認必要
			projectile.Invoke("Destroy", projectile.LifeTime);


			//これもアクセスが
			if (data.Penetration > 0)
			{
				if (projectile.GetHealth() != null)
				{
					projectile.GetHealth().MaximumHealth = data.Penetration;
					projectile.GetHealth().ResetHealthToMaxHealth();
				}
				//壁当たり　（貫通するとき別途対応）
				projectile.GetDamageOnTouch().DamageTakenNonDamageable = data.Penetration;
				//敵当たり　（貫通するとき別途対応）
				projectile.GetDamageOnTouch().DamageTakenDamageable = 1;
				//これは0
				projectile.GetDamageOnTouch().DamageTakenEveryTime = 0;
			}
			else
			{
				if (projectile.GetHealth() != null)
				{
					//当たっても消えない
					projectile.GetHealth().MaximumHealth = 999999;
					projectile.GetHealth().ResetHealthToMaxHealth();
				}
				//壁当たり　（貫通するとき別途対応）
				projectile.GetDamageOnTouch().DamageTakenNonDamageable = 0;
				//敵当たり　（貫通するとき別途対応）
				projectile.GetDamageOnTouch().DamageTakenDamageable = 0;
				//これは0
				projectile.GetDamageOnTouch().DamageTakenEveryTime = 0;
			}


			if (data.Knockback > 0f)
			{
				projectile.GetDamageOnTouch().DamageCausedKnockbackType = DamageOnTouch.KnockbackStyles.AddForce;
				projectile.GetDamageOnTouch().DamageCausedKnockbackDirection = DamageOnTouch.KnockbackDirections.BasedOnScriptDirection;

				projectile.GetDamageOnTouch().DamageCausedKnockbackForce = new Vector3(data.Knockback, 0, 0);

			}
			else
			{
				projectile.GetDamageOnTouch().DamageCausedKnockbackType = DamageOnTouch.KnockbackStyles.NoKnockback;
			}

			//対象の武器の種類がバーストタイプか同時か
			//バーストで対応してみる
			if (IsBurstShot)
			{
				BurstLength = data.Num;
				//バーストの間隔の設定は未定
			}
			else
			{
				//バーストではないパターンはこれ系
				if (this is SurviveWeaponProjectile)
				{
					(this as SurviveWeaponProjectile).ProjectilesPerShot = data.Num;
				}
			}




			//ダメージタイプ追加
			if (data != null)
			{
				(projectile.GetDamageOnTouch() as SurviveDamageOnTouch).SetDamageAttribute(SurviveDamageOnTouch.DamageAttribute.NONE, 0f);
				if (data.exWeaponStatus == AbilityData.ExWeaponStatusData.ExWeaponStatus.BombAddDamage)
				{
					(projectile.GetDamageOnTouch() as SurviveDamageOnTouch).SetDamageAttribute(SurviveDamageOnTouch.DamageAttribute.PLUS_DAMAGE, data.exWeaponStatusValue / 100f);
				}
			}
		}

		public AbilityData.WeaponLevelData GetCurrentData()
        {
			return currentData;
        }


		public void SetExOwner(SurvivePlayer survivePlayer)
		{
			exOwner = survivePlayer;
		}
	}


	public struct MainWeaponShotEvent
	{

        /// <summary>
        /// Initializes a new instance of the <see cref="MoreMountains.TopDownEngine.TopDownEngineEvent"/> struct.
        /// </summary>
        /// <param name="eventType">Event type.</param>
     
        public SurviveWeaponBase WeaponBase;
		static MainWeaponShotEvent e;
		public static void Trigger(SurviveWeaponBase weaponBase)
		{
			e.WeaponBase = weaponBase;
			MMEventManager.TriggerEvent(e);
		}
	}

	public struct DashEvent
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.TopDownEngine.TopDownEngineEvent"/> struct.
		/// </summary>
		/// <param name="eventType">Event type.</param>


		public Vector3 DashDirection;
		//public SurviveWeaponBase WeaponBase;
		static DashEvent e;
		public static void Trigger(Vector3 dashDirection)
		{
			//e.WeaponBase = weaponBase;
			e.DashDirection = dashDirection;
			MMEventManager.TriggerEvent(e);
		}
	}
	public struct KillEvent
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.TopDownEngine.TopDownEngineEvent"/> struct.
		/// </summary>
		/// <param name="eventType">Event type.</param>


		//public SurviveWeaponBase WeaponBase;
		static KillEvent e;
		public static void Trigger()
		{
			//e.WeaponBase = weaponBase;
			MMEventManager.TriggerEvent(e);
		}
	}
}