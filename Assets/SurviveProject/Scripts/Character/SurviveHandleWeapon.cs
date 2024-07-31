using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// Add this class to a character so it can use weapons
	/// Note that this component will trigger animations (if their parameter is present in the Animator), based on 
	/// the current weapon's Animations
	/// Animator parameters : defined from the Weapon's inspector
	/// </summary>
	/// 


	//ComboWeaponがあるけど一旦こいつで完結する感じに作ってみる


	[AddComponentMenu("TopDown Engine/Character/Abilities/SurviveHandleWeapon")]
	public class SurviveHandleWeapon : CharacterHandleWeapon
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "This component will allow your character to pickup and use weapons. What the weapon will do is defined in the Weapon classes. This just describes the behaviour of the 'hand' holding the weapon, not the weapon itself. Here you can set an initial weapon for your character to start with, allow weapon pickup, and specify a weapon attachment (a transform inside of your character, could be just an empty child gameobject, or a subpart of your model."; }

		[Header("Weapon")]

		/// the initial weapon owned by the character
		//[Tooltip("the initial weapon owned by the character")]
		//public Weapon InitialWeapon;

		//とりあえず
		public List<Weapon> Weapons = new List<Weapon>();

		[MMReadOnly]
		public List<Weapon> CurrentWeapons = new List<Weapon>();



		private bool initialized = false;
		private bool preInitialized = false;

		//このクラス内でしか使っていなかったのと、多分いらないやつだと思うのでいったんnew
		//protected new WeaponAim _weaponAim;


		/// if this is set to true, the character can pick up PickableWeapons
		//[Tooltip("if this is set to true, the character can pick up PickableWeapons")]
		//public bool CanPickupWeapons = true;

		//[Header("Feedbacks")]
		///// a feedback that gets triggered at the character level everytime the weapon is used
		//[Tooltip("a feedback that gets triggered at the character level everytime the weapon is used")]
		//public MMFeedbacks WeaponUseFeedback;

		//[Header("Binding")]
		///// the position the weapon will be attached to. If left blank, will be this.transform.
		//[Tooltip("the position the weapon will be attached to. If left blank, will be this.transform.")]
		//public Transform WeaponAttachment;
		///// the position from which projectiles will be spawned (can be safely left empty)
		//[Tooltip("the position from which projectiles will be spawned (can be safely left empty)")]
		//public Transform ProjectileSpawn;
		///// if this is true this animator will be automatically bound to the weapon
		//[Tooltip("if this is true this animator will be automatically bound to the weapon")]
		//public bool AutomaticallyBindAnimator = true;
		///// the ID of the AmmoDisplay this ability should update
		//[Tooltip("the ID of the AmmoDisplay this ability should update")]
		//public int AmmoDisplayID = 0;
		///// if this is true, IK will be automatically setup if possible
		//[Tooltip("if this is true, IK will be automatically setup if possible")]
		//public bool AutoIK = true;

		//[Header("Input")]
		///// if this is true you won't have to release your fire button to auto reload
		//[Tooltip("if this is true you won't have to release your fire button to auto reload")]
		//public bool ContinuousPress = false;
		///// whether or not this character getting hit should interrupt its attack (will only work if the weapon is marked as interruptable)
		//[Tooltip("whether or not this character getting hit should interrupt its attack (will only work if the weapon is marked as interruptable)")]
		//public bool GettingHitInterruptsAttack = false;
		///// whether or not pushing the secondary axis above its threshold should cause the weapon to shoot
		//[Tooltip("whether or not pushing the secondary axis above its threshold should cause the weapon to shoot")]
		//public bool UseSecondaryAxisThresholdToShoot = false;
		///// if this is true, the ForcedWeaponAimControl mode will be applied to all weapons equipped by this character
		//[Tooltip("if this is true, the ForcedWeaponAimControl mode will be applied to all weapons equipped by this character")]
		//public bool ForceWeaponAimControl = false;
		///// if ForceWeaponAimControl is true, the AimControls mode to apply to all weapons equipped by this character
		//[Tooltip("if ForceWeaponAimControl is true, the AimControls mode to apply to all weapons equipped by this character")]
		//[MMCondition("ForceWeaponAimControl", true)]
		//public WeaponAim.AimControls ForcedWeaponAimControl = WeaponAim.AimControls.PrimaryMovement;

		//[Header("Buffering")]
		///// whether or not attack input should be buffered, letting you prepare an attack while another is being performed, making it easier to chain them
		//[Tooltip("whether or not attack input should be buffered, letting you prepare an attack while another is being performed, making it easier to chain them")]
		//public bool BufferInput;
		///// if this is true, every new input will prolong the buffer
		//[MMCondition("BufferInput", true)]
		//[Tooltip("if this is true, every new input will prolong the buffer")]
		//public bool NewInputExtendsBuffer;
		///// the maximum duration for the buffer, in seconds
		//[MMCondition("BufferInput", true)]
		//[Tooltip("the maximum duration for the buffer, in seconds")]
		//public float MaximumBufferDuration = 0.25f;
		///// if this is true, and if this character is using GridMovement, then input will only be triggered when on a perfect tile
		//[MMCondition("BufferInput", true)]
		//[Tooltip("if this is true, and if this character is using GridMovement, then input will only be triggered when on a perfect tile")]
		//public bool RequiresPerfectTile = false;

		//[Header("Debug")]

		///// the weapon currently equipped by the Character
		//[MMReadOnly]
		//[Tooltip("the weapon currently equipped by the Character")]
		//public Weapon CurrentWeapon;

		/// the ID / index of this CharacterHandleWeapon. This will be used to determine what handle weapon ability should equip a weapon.
		/// If you create more Handle Weapon abilities, make sure to override and increment this  
		public override int HandleWeaponID { get { return 1; } }

		/// an animator to update when the weapon is used
		//public Animator CharacterAnimator { get; set; }
		/// the weapon's weapon aim component, if it has one
		//public WeaponAim WeaponAimComponent { get { return _weaponAim; } }

		public delegate void OnWeaponChangeDelegate();
		/// a delegate you can hook to, to be notified of weapon changes
		//public OnWeaponChangeDelegate OnWeaponChange;

		//protected float _fireTimer = 0f;
		//protected float _secondaryHorizontalMovement;
		//protected float _secondaryVerticalMovement;
		//protected WeaponAim _weaponAim;
		//protected ProjectileWeapon _projectileWeapon;
		//protected WeaponIK _weaponIK;
		//protected Transform _leftHandTarget = null;
		//protected Transform _rightHandTarget = null;
		//protected float _bufferEndsAt = 0f;
		//protected bool _buffering = false;
		//protected const string _weaponEquippedAnimationParameterName = "WeaponEquipped";
		//protected const string _weaponEquippedIDAnimationParameterName = "WeaponEquippedID";
		//protected int _weaponEquippedAnimationParameter;
		//protected int _weaponEquippedIDAnimationParameter;
		//protected CharacterGridMovement _characterGridMovement;
		//protected List<WeaponModel> _weaponModels;

		/// <summary>
		/// Sets the weapon attachment
		/// </summary>
		protected override void PreInitialization()
		{	if (preInitialized)
				return;
			base.PreInitialization();
			// filler if the WeaponAttachment has not been set
			if (WeaponAttachment == null)
			{
				WeaponAttachment = transform;
			}
			preInitialized = true;
		}

		// Initialization
		protected override void Initialization()
		{
			if (!preInitialized)
            {
				PreInitialization();
			}
			if (initialized)
				return;
			base.Initialization();
			initialized = true;
			//Setup();
		}

		/// <summary>
		/// Grabs various components and inits stuff
		/// </summary>
		public override void Setup()
		{
			_character = this.gameObject.GetComponentInParent<Character>();
			_characterGridMovement = _character?.FindAbility<CharacterGridMovement>();
			_weaponModels = new List<WeaponModel>();
			foreach (WeaponModel model in _character.gameObject.GetComponentsInChildren<WeaponModel>())
			{
				_weaponModels.Add(model);
			}
			CharacterAnimator = _animator;
			// filler if the WeaponAttachment has not been set
			if (WeaponAttachment == null)
			{
				WeaponAttachment = transform;
			}
			if ((_animator != null) && (AutoIK))
			{
				_weaponIK = _animator.GetComponent<WeaponIK>();
			}
		
			_weaponAim = WeaponAttachment.gameObject.MMGetComponentNoAlloc<SurviveCharacterAim>();

			// we set the initial weapon

			//ここは初期化のタイミングで各データと合わせていく感じ
			//if (InitialWeapon != null)
			//{
			//	if (CurrentWeapon != null)
			//	{
			//		if (CurrentWeapon.name != InitialWeapon.name)
			//		{
			//			ChangeWeapon(InitialWeapon, InitialWeapon.WeaponName, false);    
			//		}
			//	}
			//	else
			//	{
			//		ChangeWeapon(InitialWeapon, InitialWeapon.WeaponName, false);    
			//	}
			//}
			//一旦複数装備テスト
			foreach (var w in Weapons)
            {
				AddWeapon(w, w.WeaponName, true);

			}

		}

		/// <summary>
		/// Every frame we check if it's needed to update the ammo display
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			HandleCharacterState();
			HandleFeedbacks();
			UpdateAmmoDisplay();
			HandleBuffer();
		}

		/// <summary>
		/// Checks character state and stops shooting if not in normal state
		/// </summary>
		protected override void HandleCharacterState()
		{
			if (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			{
				ShootStop();
			}
		}

		/// <summary>
		/// Triggers the weapon used feedback if needed
		/// </summary>
		protected override void HandleFeedbacks()
		{
			if (CurrentWeapon != null)
			{
				if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse)
				{
					WeaponUseFeedback?.PlayFeedbacks();
				}
			}
		}

		/// <summary>
		/// Gets input and triggers methods based on what's been pressed
		/// </summary>
		protected override void HandleInput()
        {
            //	if (!AbilityAuthorized
            //	    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
            //	    || (CurrentWeapon == null))
            //	{
            //		return;
            //	}

            //	bool inputAuthorized = true;
            //	if (CurrentWeapon != null)
            //	{
            //		inputAuthorized = CurrentWeapon.InputAuthorized;
            //	}

            //	if (inputAuthorized && ((_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown) || (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonDown)))
            //	{
            ShootStart();
			//}

			//bool buttonPressed =
			//	(_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed) ||
			//	(_inputManager.ShootAxis == MMInput.ButtonStates.ButtonPressed); 
                
			//if (inputAuthorized && ContinuousPress && (CurrentWeapon.TriggerMode == Weapon.TriggerModes.Auto) && buttonPressed)
			//{
			//	ShootStart();
			//}
            
			//if (_inputManager.ReloadButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			//{
			//	Reload();
			//}

			//if (inputAuthorized && ((_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonUp) || (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonUp)))
			//{
			//	ShootStop();
			//}

			//if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses)
			//    && ((_inputManager.ShootAxis == MMInput.ButtonStates.Off) && (_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.Off))
			//    && !(UseSecondaryAxisThresholdToShoot && (_inputManager.SecondaryMovement.magnitude > _inputManager.Threshold.magnitude)))
			//{
			//	CurrentWeapon.WeaponInputStop();
			//}

			//if (inputAuthorized && UseSecondaryAxisThresholdToShoot && (_inputManager.SecondaryMovement.magnitude > _inputManager.Threshold.magnitude))
			//{
			//	ShootStart();
			//}
		}

		/// <summary>
		/// Triggers an attack if the weapon is idle and an input has been buffered
		/// </summary>
		protected override void HandleBuffer()
		{
			if (CurrentWeapon == null)
			{
				return;
			}
            
			// if we are currently buffering an input and if the weapon is now idle
			if (_buffering && (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle))
			{
				// and if our buffer is still valid, we trigger an attack
				if (Time.time < _bufferEndsAt)
				{
					ShootStart();
				}
				else
				{
					_buffering = false;
				}                
			}
		}

		/// <summary>
		/// Causes the character to start shooting
		/// </summary>
		public override void ShootStart()
		{
			// if the Shoot action is enabled in the permissions, we continue, if not we do nothing.  If the player is dead we do nothing.
			if (!AbilityAuthorized
				//|| (CurrentWeapon == null)
				|| (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}

			//  if we've decided to buffer input, and if the weapon is in use right now
			//武器のステートの判定ではなく、プレイヤー自身のステートで判定していく（全体的に）
			if (BufferInput/* && (CurrentWeapon.WeaponState.CurrentState != Weapon.WeaponStates.WeaponIdle)*/)
			{
				// if we're not already buffering, or if each new input extends the buffer, we turn our buffering state to true
				ExtendBuffer();
			}

			if (BufferInput && RequiresPerfectTile && (_characterGridMovement != null))
			{
				if (!_characterGridMovement.PerfectTile)
				{
					ExtendBuffer();
					return;
				}
				else
				{
					_buffering = false;
				}
			}
			PlayAbilityStartFeedbacks();
			foreach (var w in CurrentWeapons)
			{
				w.WeaponInputStart();
			}
		}

		/// <summary>
		/// Extends the duration of the buffer if needed
		/// </summary>
		protected override void ExtendBuffer()
		{
			if (!_buffering || NewInputExtendsBuffer)
			{
				_buffering = true;
				_bufferEndsAt = Time.time + MaximumBufferDuration;
			}
		}

		/// <summary>
		/// Causes the character to stop shooting
		/// </summary>
		public override void ShootStop()
		{
			// if the Shoot action is enabled in the permissions, we continue, if not we do nothing
			if (!AbilityAuthorized
			    || (CurrentWeapon == null))
			{
				return;
			}

			if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle)
			{
				return;
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload)
			    || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart)
			    || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop))
			{
				return;
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse) && (!CurrentWeapon.DelayBeforeUseReleaseInterruption))
			{
				return;
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses) && (!CurrentWeapon.TimeBetweenUsesReleaseInterruption))
			{
				return;
			}

			if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse) 
			{
				return;
			}

			ForceStop();
		}

		/// <summary>
		/// Forces the weapon to stop 
		/// </summary>
		public override void ForceStop()
		{
			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();
			if (CurrentWeapon != null)
			{
				CurrentWeapon.TurnWeaponOff();    
			}
		}

		/// <summary>
		/// Reloads the weapon
		/// </summary>
		public override void Reload()
		{
			if (CurrentWeapon != null)
			{
				CurrentWeapon.InitiateReloadWeapon();
			}
		}

		/// <summary>
		/// Changes the character's current weapon to the one passed as a parameter
		/// </summary>
		/// <param name="newWeapon">The new weapon.</param>
		public override void ChangeWeapon(Weapon newWeapon, string weaponID, bool combo = false)
		{
			// if the character already has a weapon, we make it stop shooting
			if (CurrentWeapon != null)
			{
				CurrentWeapon.TurnWeaponOff();
				if (!combo)
				{
					ShootStop();
					if (_weaponAim != null) { _weaponAim.RemoveReticle(); }
					Destroy(CurrentWeapon.gameObject);
				}
			}

			if (newWeapon != null)
			{
				InstantiateWeapon(newWeapon, weaponID, combo);
			}
			else
			{
				CurrentWeapon = null;
			}

			if (OnWeaponChange != null)
			{
				OnWeaponChange();
			}
		}

		public SurviveWeaponBase AddWeapon(Weapon newWeapon, string weaponID, bool combo = false)
        {
            // if the character already has a weapon, we make it stop shooting
            //if (CurrentWeapon != null)
            //{
            //	CurrentWeapon.TurnWeaponOff();
            //	if (!combo)
            //	{
            //		ShootStop();
            //		if (_weaponAim != null) { _weaponAim.RemoveReticle(); }
            //		Destroy(CurrentWeapon.gameObject);
            //	}
            //}
            SurviveWeaponBase surviveWeaponBase = null;
            
			if (newWeapon != null)
			{
				surviveWeaponBase = InstantiateAddWeapon(newWeapon, weaponID, combo);
			}
			//else
			//{
			//	CurrentWeapon = null;
			//}

			//
			if (OnWeaponChange != null)
			{
				OnWeaponChange();
			}


			return surviveWeaponBase;
		}

		/// <summary>
		/// Instantiates the specified weapon
		/// </summary>
		/// <param name="newWeapon"></param>
		/// <param name="weaponID"></param>
		/// <param name="combo"></param>
		protected override void InstantiateWeapon(Weapon newWeapon, string weaponID, bool combo = false)
		{
			if (!combo)
			{
				CurrentWeapon = (Weapon)Instantiate(newWeapon, WeaponAttachment.transform.position + newWeapon.WeaponAttachmentOffset, WeaponAttachment.transform.rotation);
			}

			CurrentWeapon.name = newWeapon.name;
			CurrentWeapon.transform.parent = WeaponAttachment.transform;
			CurrentWeapon.transform.localPosition = newWeapon.WeaponAttachmentOffset;


			//既存の武器システムを継承して使うか悩みどころ
			CurrentWeapon.SetOwner(_character, this);
			CurrentWeapon.WeaponID = weaponID;
			CurrentWeapon.FlipWeapon();
			//_weaponAim = CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();

			HandleWeaponAim();

			// we handle (optional) inverse kinematics (IK) 
			HandleWeaponIK();

			// we handle the weapon model
			HandleWeaponModel(newWeapon, weaponID, combo, CurrentWeapon);

			// we turn off the gun's emitters.
			CurrentWeapon.Initialization();
			CurrentWeapon.InitializeComboWeapons();
			CurrentWeapon.InitializeAnimatorParameters();
			InitializeAnimatorParameters();
		}

		protected  SurviveWeaponBase InstantiateAddWeapon(Weapon newWeapon, string weaponID, bool combo = false)
		{

			if (!initialized)
				Initialization();

			var CurrentWeapon = (Weapon)Instantiate(newWeapon, WeaponAttachment.transform.position + newWeapon.WeaponAttachmentOffset, WeaponAttachment.transform.rotation);

			CurrentWeapon.name = newWeapon.name;
			CurrentWeapon.transform.parent = WeaponAttachment.transform;
			CurrentWeapon.transform.localPosition = newWeapon.WeaponAttachmentOffset;


			//既存の武器システムを継承して使うか悩みどころ
			CurrentWeapon.SetOwner(_character, this);
			CurrentWeapon.WeaponID = weaponID;
			CurrentWeapon.FlipWeapon();
			//_weaponAim = CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();

			HandleWeaponAim();

			// we handle (optional) inverse kinematics (IK) 
			HandleWeaponIK(CurrentWeapon);

			// we handle the weapon model
			HandleWeaponModel(newWeapon, weaponID, combo, CurrentWeapon);

			// we turn off the gun's emitters.
			CurrentWeapon.Initialization();
			CurrentWeapon.InitializeComboWeapons();
			CurrentWeapon.InitializeAnimatorParameters();
			CurrentWeapons.Add(CurrentWeapon);
			InitializeAnimatorParameters();

			return CurrentWeapon as SurviveWeaponBase;
		}

		/// <summary>
		/// Applies aim if possible
		/// </summary>
		protected override void HandleWeaponAim()
		{
			if ((_weaponAim != null) && (_weaponAim.enabled))
			{
				if (ForceWeaponAimControl)
				{
					_weaponAim.AimControl = ForcedWeaponAimControl;
				}
				_weaponAim.ApplyAim();
			}
		}

		/// <summary>
		/// Sets IK handles if needed
		/// </summary>
		protected void HandleWeaponIK(Weapon CurrentWeapon)
		{
			if (_weaponIK != null)
			{
				_weaponIK.SetHandles(CurrentWeapon.LeftHandHandle, CurrentWeapon.RightHandHandle);
			}
			_projectileWeapon = CurrentWeapon.gameObject.MMFGetComponentNoAlloc<ProjectileWeapon>();
			if (_projectileWeapon != null)
			{
				_projectileWeapon.SetProjectileSpawnTransform(ProjectileSpawn);
			}
		}

		protected override void HandleWeaponModel(Weapon newWeapon, string weaponID, bool combo = false, Weapon weapon = null)
		{
			if (_weaponModels == null)
			{
				return;
			}
			foreach (WeaponModel model in _weaponModels)
			{
				model.Hide();
				if (model.WeaponID == weaponID)
				{
					model.Show();
					if (model.UseIK)
					{
						_weaponIK.SetHandles(model.LeftHandHandle, model.RightHandHandle);
					}
					if (weapon != null)
					{
						if (model.BindFeedbacks)
						{
							weapon.WeaponStartMMFeedback = model.WeaponStartMMFeedback;
							weapon.WeaponUsedMMFeedback = model.WeaponUsedMMFeedback;
							weapon.WeaponStopMMFeedback = model.WeaponStopMMFeedback;
							weapon.WeaponReloadMMFeedback = model.WeaponReloadMMFeedback;
							weapon.WeaponReloadNeededMMFeedback = model.WeaponReloadNeededMMFeedback;
						}
						if (model.AddAnimator)
						{
							weapon.Animators.Add(model.TargetAnimator);
						}
						if (model.OverrideWeaponUseTransform)
						{
							weapon.WeaponUseTransform = model.WeaponUseTransform;
						}
					}
				}
			}
		}

		/// <summary>
		/// Flips the current weapon if needed
		/// </summary>
		public override void Flip()
		{
		}

		/// <summary>
		/// Updates the ammo display bar and text.
		/// </summary>
		public override void UpdateAmmoDisplay()
		{
			if ((GUIManager.HasInstance) && (_character.CharacterType == Character.CharacterTypes.Player))
			{
				if (CurrentWeapon == null)
				{
					GUIManager.Instance.SetAmmoDisplays(false, _character.PlayerID, AmmoDisplayID);
					return;
				}

				if (!CurrentWeapon.MagazineBased && (CurrentWeapon.WeaponAmmo == null))
				{
					GUIManager.Instance.SetAmmoDisplays(false, _character.PlayerID, AmmoDisplayID);
					return;
				}

				if (CurrentWeapon.WeaponAmmo == null)
				{
					GUIManager.Instance.SetAmmoDisplays(true, _character.PlayerID, AmmoDisplayID);
					GUIManager.Instance.UpdateAmmoDisplays(CurrentWeapon.MagazineBased, 0, 0, CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.MagazineSize, _character.PlayerID, AmmoDisplayID, false);
					return;
				}
				else
				{
					GUIManager.Instance.SetAmmoDisplays(true, _character.PlayerID, AmmoDisplayID); 
					GUIManager.Instance.UpdateAmmoDisplays(CurrentWeapon.MagazineBased, CurrentWeapon.WeaponAmmo.CurrentAmmoAvailable + CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.WeaponAmmo.MaxAmmo, CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.MagazineSize, _character.PlayerID, AmmoDisplayID, true);
					return;
				}
			}
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			if (CurrentWeapon == null)
			{ return; }

			RegisterAnimatorParameter(_weaponEquippedAnimationParameterName, AnimatorControllerParameterType.Bool, out _weaponEquippedAnimationParameter);
			RegisterAnimatorParameter(_weaponEquippedIDAnimationParameterName, AnimatorControllerParameterType.Int, out _weaponEquippedIDAnimationParameter);
		}

		/// <summary>
		/// Override this to send parameters to the character's animator. This is called once per cycle, by the Character
		/// class, after Early, normal and Late process().
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _weaponEquippedAnimationParameter, (CurrentWeapon != null), _character._animatorParameters, _character.RunAnimatorSanityChecks);
			if (CurrentWeapon == null)
			{
				MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _weaponEquippedIDAnimationParameter, -1, _character._animatorParameters, _character.RunAnimatorSanityChecks);
				return;
			}
			else
			{
				MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _weaponEquippedIDAnimationParameter, CurrentWeapon.WeaponAnimationID, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			}
		}

		protected override void OnHit()
		{
			base.OnHit();
			if (GettingHitInterruptsAttack && (CurrentWeapon != null))
			{
				CurrentWeapon.Interrupt();
			}
		}

		protected override void OnDeath()
		{
			base.OnDeath();
			ShootStop();
			if (CurrentWeapon != null)
			{
				ChangeWeapon(null, "");
			}
		}

		protected override void OnRespawn()
		{
			//base.OnRespawn();
			//Setup();
		}
	}
}