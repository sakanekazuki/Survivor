using UnityEngine;
using MoreMountains.Tools;
using System.Collections.Generic;

using UnityEngine.InputSystem;
using UnityEngine.UIElements;
//using UnityEditor.VersionControl;
using static UnityEngine.EventSystems.StandaloneInputModule;
using System.Linq;

namespace MoreMountains.TopDownEngine
{	
	/// <summary>
	/// This persistent singleton handles the inputs and sends commands to the player.
	/// IMPORTANT : this script's Execution Order MUST be -100.
	/// You can define a script's execution order by clicking on the script's file and then clicking on the Execution Order button at the bottom right of the script's inspector.
	/// See https://docs.unity3d.com/Manual/class-ScriptExecution.html for more details
	/// </summary>
	[AddComponentMenu("TopDown Engine/Managers/Survive Input Manager")]
	public class SurviveInputManager : InputManager
	{
		
		//public SurviveControls surviveControls;
		//public SurviveControls.PlayerActions playerActions;
		public TopDownEngineInputActions actions;
		private InputActionAsset asset;
        public MMInput.IMButton SubmitButton { get; protected set; }
        public MMInput.IMButton CancelButton { get; protected set; }

        //protected List<MMInput.IMButton> ButtonList;

        /// <summary>
        /// On Start we look for what mode to use, and initialize our axis and buttons
        /// </summary>
        protected override void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Initializes buttons and axis
		/// </summary>
		protected override void PreInitialization()
		{
			//surviveControls = new SurviveControls();
			//playerActions = surviveControls.Player;
			//surviveControls.Enable();
			actions = new TopDownEngineInputActions();
			asset = actions.asset;
			asset.Enable();
			InitializeButtons();
			InitializeAxis();
		}
		
		/// <summary>
		/// On init we auto detect control schemes
		/// </summary>
		protected override void Initialization()
		{
			ControlsModeDetection();


		
		}

		/// <summary>
		/// Turns mobile controls on or off depending on what's been defined in the inspector, and what target device we're on
		/// </summary>
		public override void ControlsModeDetection()
		{
			if (GUIManager.HasInstance) { GUIManager.Instance.SetMobileControlsActive(false); }
			IsMobile=false;
			if (AutoMobileDetection)
			{
				#if UNITY_ANDROID || UNITY_IPHONE
					if (GUIManager.HasInstance) { GUIManager.Instance.SetMobileControlsActive(true,MovementControl); }
					IsMobile = true;
				#endif
			}
			if (InputForcedMode==InputForcedModes.Mobile)
			{
				if (GUIManager.HasInstance) { GUIManager.Instance.SetMobileControlsActive(true, MovementControl); }
				IsMobile = true;
			}
			if (InputForcedMode==InputForcedModes.Desktop)
			{
				if (GUIManager.HasInstance) { GUIManager.Instance.SetMobileControlsActive(false); }
				IsMobile = false;					
			}
			if (HideMobileControlsInEditor)
			{
				#if UNITY_EDITOR
				if (GUIManager.HasInstance) { GUIManager.Instance.SetMobileControlsActive(false); }
				IsMobile = false;	
				#endif
			}
		}

		/// <summary>
		/// Initializes the buttons. If you want to add more buttons, make sure to register them here.
		/// </summary>
		protected override void InitializeButtons()
		{
			ButtonList = new List<MMInput.IMButton> ();
            ButtonList.Add(JumpButton = new MMInput.IMButton(PlayerID, "Jump", JumpButtonDown, JumpButtonPressed, JumpButtonUp));

            ButtonList.Add(RunButton  = new MMInput.IMButton (PlayerID, "Run", RunButtonDown, RunButtonPressed, RunButtonUp));
			ButtonList.Add(InteractButton = new MMInput.IMButton(PlayerID, "Interact", InteractButtonDown, InteractButtonPressed, InteractButtonUp));
			ButtonList.Add(DashButton  = new MMInput.IMButton (PlayerID, "Dash", DashButtonDown, DashButtonPressed, DashButtonUp));
			ButtonList.Add(CrouchButton  = new MMInput.IMButton (PlayerID, "Crouch", CrouchButtonDown, CrouchButtonPressed, CrouchButtonUp));
			ButtonList.Add(SecondaryShootButton = new MMInput.IMButton(PlayerID, "SecondaryShoot", SecondaryShootButtonDown, SecondaryShootButtonPressed, SecondaryShootButtonUp));
            ButtonList.Add(ShootButton = new MMInput.IMButton(PlayerID, "Shoot", ShootButtonDown, ShootButtonPressed, ShootButtonUp));
   //         playerActions.Player1_Shoot.started += (_)=> { ShootButtonDown(); };
			//playerActions.Player1_Shoot.performed += (_)=> { ShootButtonPressed(); };
			//playerActions.Player1_Shoot.canceled += (_)=> { ShootButtonUp(); };

			ButtonList.Add(ReloadButton = new MMInput.IMButton (PlayerID, "Reload", ReloadButtonDown, ReloadButtonPressed, ReloadButtonUp));
			ButtonList.Add(SwitchWeaponButton = new MMInput.IMButton (PlayerID, "SwitchWeapon", SwitchWeaponButtonDown, SwitchWeaponButtonPressed, SwitchWeaponButtonUp));
			ButtonList.Add(PauseButton = new MMInput.IMButton(PlayerID, "Pause", PauseButtonDown, PauseButtonPressed, PauseButtonUp));
			ButtonList.Add(TimeControlButton = new MMInput.IMButton(PlayerID, "TimeControl", TimeControlButtonDown, TimeControlButtonPressed, TimeControlButtonUp));
			ButtonList.Add(SwitchCharacterButton = new MMInput.IMButton(PlayerID, "SwitchCharacter", SwitchCharacterButtonDown, SwitchCharacterButtonPressed, SwitchCharacterButtonUp));
            ButtonList.Add(SubmitButton = new MMInput.IMButton(PlayerID, "Submit", SubmitButtonDown, SubmitButtonPressed, SubmitButtonUp));
            ButtonList.Add(CancelButton = new MMInput.IMButton(PlayerID, "Cancel", CancelButtonDown, CancelButtonPressed, CancelButtonUp));
        }

        /// <summary>
        /// Initializes the axis strings.
        /// </summary>
        protected override void InitializeAxis()
		{
			_axisHorizontal = PlayerID+"_Horizontal";
			_axisVertical = PlayerID+"_Vertical";
			_axisSecondaryHorizontal = PlayerID+"_SecondaryHorizontal";
			_axisSecondaryVertical = PlayerID+"_SecondaryVertical";
			_axisShoot = PlayerID+"_ShootAxis";
			_axisShootSecondary = PlayerID + "_SecondaryShootAxis";
			_axisCamera = PlayerID + "_CameraRotationAxis";
		}

		/// <summary>
		/// On LateUpdate, we process our button states
		/// </summary>
		protected override void LateUpdate()
		{
			ProcessButtonStates();
		}

		/// <summary>
		/// At update, we check the various commands and update our values and states accordingly.
		/// </summary>
		protected override void Update()
		{		
			if (!IsMobile && InputDetectionActive)
			{	
				SetMovement();	
				SetSecondaryMovement ();
				SetShootAxis ();
				SetCameraRotationAxis();
				GetInputButtons ();
				GetLastNonNullValues();

				CheckInputMode();

            }									
		}

		/// <summary>
		/// Gets the last non null values for both primary and secondary axis
		/// </summary>
		protected override void GetLastNonNullValues()
		{
			if (_primaryMovement.magnitude > Threshold.x)
			{
				LastNonNullPrimaryMovement = _primaryMovement;
			}
			if (_secondaryMovement.magnitude > Threshold.x)
			{
				LastNonNullSecondaryMovement = _secondaryMovement;
			}
		}

		/// <summary>
		/// If we're not on mobile, watches for input changes, and updates our buttons states accordingly
		/// </summary>
		protected override void GetInputButtons()
		{
			foreach(MMInput.IMButton button in ButtonList)
			{
				//ˆê’Uinputsystem‘¤‚É‚ ‚é‚©‚È‚¢‚©‚Ì”»’è‚ð‚µ‚Ä‘S•”“®‚­‚æ‚¤‚É‚µ‚Ä‚Ý‚é ‚»‚à‚»‚à“ü‚ê‚È‚¢‚æ‚¤‚É‚µ‚Ä‚Ý‚é

				//if(playerActions.Player1_Jump.IsPressed)
				string name = button.ButtonID.Replace(PlayerID + "_", "");
				var act = actions.FindAction(name, throwIfNotFound: true);

				if (act != null)
                {
					if (act.WasPerformedThisFrame())
					{ 
						button.TriggerButtonPressed();
					}
					if (act.WasPressedThisFrame())
					{
						button.TriggerButtonDown();
					}
					if (act.WasReleasedThisFrame())
					{
						button.TriggerButtonUp();
					}
				}
				else
                {
					if (Input.GetButton(button.ButtonID))
					{
						button.TriggerButtonPressed();
					}
					if (Input.GetButtonDown(button.ButtonID))
					{
						button.TriggerButtonDown();
					}
					if (Input.GetButtonUp(button.ButtonID))
					{
						button.TriggerButtonUp();
					}
				}

				
			}
		}

		/// <summary>
		/// Called at LateUpdate(), this method processes the button states of all registered buttons
		/// </summary>
		public override void ProcessButtonStates()
		{
			// for each button, if we were at ButtonDown this frame, we go to ButtonPressed. If we were at ButtonUp, we're now Off
			foreach (MMInput.IMButton button in ButtonList)
			{
				if (button.State.CurrentState == MMInput.ButtonStates.ButtonDown)
				{
					button.State.ChangeState(MMInput.ButtonStates.ButtonPressed);				
				}	
				if (button.State.CurrentState == MMInput.ButtonStates.ButtonUp)
				{
					button.State.ChangeState(MMInput.ButtonStates.Off);				
				}	
			}
		}

		/// <summary>
		/// Called every frame, if not on mobile, gets primary movement values from input
		/// </summary>
		public override void SetMovement()
		{
			if (!IsMobile && InputDetectionActive)
			{
				if (SmoothMovement)
				{
					//_primaryMovement.x = Input.GetAxis(_axisHorizontal);
					//_primaryMovement.y = Input.GetAxis(_axisVertical);		

					_primaryMovement = actions.PlayerControls.PrimaryMovement.ReadValue<Vector2>();

				}
				else
				{
					_primaryMovement.x = Input.GetAxisRaw(_axisHorizontal);
					_primaryMovement.y = Input.GetAxisRaw(_axisVertical);
				}
				_primaryMovement = ApplyCameraRotation(_primaryMovement);
			}
		}

		/// <summary>
		/// Called every frame, if not on mobile, gets secondary movement values from input
		/// </summary>
		public override void SetSecondaryMovement()
		{
			if (!IsMobile && InputDetectionActive)
			{
				if (SmoothMovement)
				{
					//_secondaryMovement.x = Input.GetAxis(_axisSecondaryHorizontal);
					//_secondaryMovement.y = Input.GetAxis(_axisSecondaryVertical);		
					_secondaryMovement = actions.PlayerControls.SecondaryMovement.ReadValue<Vector2>();

				}
				else
				{
					_secondaryMovement.x = Input.GetAxisRaw(_axisSecondaryHorizontal);
					_secondaryMovement.y = Input.GetAxisRaw(_axisSecondaryVertical);
				}
				_secondaryMovement = ApplyCameraRotation(_secondaryMovement);
			}
		}

		/// <summary>
		/// Called every frame, if not on mobile, gets shoot axis values from input
		/// </summary>
		protected override void SetShootAxis()
		{
			if (!IsMobile && InputDetectionActive)
			{
				ShootAxis = MMInput.ProcessAxisAsButton (_axisShoot, Threshold.y, ShootAxis);
				SecondaryShootAxis = MMInput.ProcessAxisAsButton(_axisShootSecondary, Threshold.y, SecondaryShootAxis, MMInput.AxisTypes.Positive);
			}
		}

		/// <summary>
		/// Grabs camera rotation input and stores it
		/// </summary>
		protected override void SetCameraRotationAxis()
		{
			if (!IsMobile)
			{
				_cameraRotationInput = Input.GetAxis(_axisCamera);	
			}
		}

		/// <summary>
		/// If you're using a touch joystick, bind your main joystick to this method
		/// </summary>
		/// <param name="movement">Movement.</param>
		public override void SetMovement(Vector2 movement)
		{
			if (IsMobile && InputDetectionActive)
			{
				_primaryMovement.x = movement.x;
				_primaryMovement.y = movement.y;
			}
			_primaryMovement = ApplyCameraRotation(_primaryMovement);
		}

		/// <summary>
		/// If you're using a touch joystick, bind your secondary joystick to this method
		/// </summary>
		/// <param name="movement">Movement.</param>
		public override void SetSecondaryMovement(Vector2 movement)
		{
			if (IsMobile && InputDetectionActive)
			{
				_secondaryMovement.x = movement.x;
				_secondaryMovement.y = movement.y;
			}
			_secondaryMovement = ApplyCameraRotation(_secondaryMovement);
		}

		/// <summary>
		/// If you're using touch arrows, bind your left/right arrows to this method
		/// </summary>
		/// <param name="">.</param>
		public override void SetHorizontalMovement(float horizontalInput)
		{
			if (IsMobile && InputDetectionActive)
			{
				_primaryMovement.x = horizontalInput;
			}
		}

		/// <summary>
		/// If you're using touch arrows, bind your secondary down/up arrows to this method
		/// </summary>
		/// <param name="">.</param>
		public override void SetVerticalMovement(float verticalInput)
		{
			if (IsMobile && InputDetectionActive)
			{
				_primaryMovement.y = verticalInput;
			}
		}

		/// <summary>
		/// If you're using touch arrows, bind your secondary left/right arrows to this method
		/// </summary>
		/// <param name="">.</param>
		public override void SetSecondaryHorizontalMovement(float horizontalInput)
		{
			if (IsMobile && InputDetectionActive)
			{
				_secondaryMovement.x = horizontalInput;
			}
		}

		/// <summary>
		/// If you're using touch arrows, bind your down/up arrows to this method
		/// </summary>
		/// <param name="">.</param>
		public override void SetSecondaryVerticalMovement(float verticalInput)
		{
			if (IsMobile && InputDetectionActive)
			{
				_secondaryMovement.y = verticalInput;
			}
		}

		/// <summary>
		/// Sets an associated camera, used to rotate input based on camera position
		/// </summary>
		/// <param name="targetCamera"></param>
		/// <param name="rotationAxis"></param>
		public override void SetCamera(Camera targetCamera, bool camera3D)
		{
			_targetCamera = targetCamera;
			_camera3D = camera3D;
		}

		/// <summary>
		/// Sets the current camera rotation input, which you'll want to keep between -1 (left) and 1 (right), 0 being no rotation
		/// </summary>
		/// <param name="newValue"></param>
		public override void SetCameraRotationInput(float newValue)
		{
			_cameraRotationInput = newValue;
		}

		/// <summary>
		/// Rotates input based on camera orientation
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override Vector2 ApplyCameraRotation(Vector2 input)
		{
			if (RotateInputBasedOnCameraDirection)
			{
				if (_camera3D)
				{
					_cameraAngle = _targetCamera.transform.localEulerAngles.y;
					return MMMaths.RotateVector2(input, -_cameraAngle);
				}
				else
				{
					_cameraAngle = _targetCamera.transform.localEulerAngles.z;
					return MMMaths.RotateVector2(input, _cameraAngle);
				}
			}
			else
			{
				return input;
			}
		}

		/// <summary>
		/// If we lose focus, we reset the states of all buttons
		/// </summary>
		/// <param name="hasFocus"></param>
		//protected void OnApplicationFocus(bool hasFocus)
		//{
		//	if (!hasFocus && ResetButtonStatesOnFocusLoss && (ButtonList != null))
		//	{
		//		foreach (MMInput.IMButton button in ButtonList)
		//		{
		//			button.State.ChangeState(MMInput.ButtonStates.ButtonUp);
		//		}
		//	}
		//}

		public override void JumpButtonDown()		{ JumpButton.State.ChangeState (MMInput.ButtonStates.ButtonDown); }
		public override void JumpButtonPressed()		{ JumpButton.State.ChangeState (MMInput.ButtonStates.ButtonPressed); }
		public override void JumpButtonUp()			{ JumpButton.State.ChangeState (MMInput.ButtonStates.ButtonUp); }

		public override void DashButtonDown()		{ DashButton.State.ChangeState (MMInput.ButtonStates.ButtonDown); }
		public override void DashButtonPressed()		{ DashButton.State.ChangeState (MMInput.ButtonStates.ButtonPressed); }
		public override void DashButtonUp()			{ DashButton.State.ChangeState (MMInput.ButtonStates.ButtonUp); }

		public override void CrouchButtonDown()		{ CrouchButton.State.ChangeState (MMInput.ButtonStates.ButtonDown); }
		public override void CrouchButtonPressed()	{ CrouchButton.State.ChangeState (MMInput.ButtonStates.ButtonPressed); }
		public override void CrouchButtonUp()		{ CrouchButton.State.ChangeState (MMInput.ButtonStates.ButtonUp); }

		public virtual void RunButtonDown()			{ RunButton.State.ChangeState (MMInput.ButtonStates.ButtonDown); }
		public override void RunButtonPressed()		{ RunButton.State.ChangeState (MMInput.ButtonStates.ButtonPressed); }
		public override void RunButtonUp()			{ RunButton.State.ChangeState (MMInput.ButtonStates.ButtonUp); }

		public override void ReloadButtonDown()		{ ReloadButton.State.ChangeState (MMInput.ButtonStates.ButtonDown); }
		public override void ReloadButtonPressed()	{ ReloadButton.State.ChangeState (MMInput.ButtonStates.ButtonPressed); }
		public override void ReloadButtonUp()		{ ReloadButton.State.ChangeState (MMInput.ButtonStates.ButtonUp); }

		public override void InteractButtonDown() { InteractButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
		public override void InteractButtonPressed() { InteractButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
		public override void InteractButtonUp() { InteractButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

		public override void ShootButtonDown()		{ ShootButton.State.ChangeState (MMInput.ButtonStates.ButtonDown); }
		public override void ShootButtonPressed()	{ ShootButton.State.ChangeState (MMInput.ButtonStates.ButtonPressed); }
		public override void ShootButtonUp()			{ ShootButton.State.ChangeState (MMInput.ButtonStates.ButtonUp); }

		public override void SecondaryShootButtonDown() { SecondaryShootButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
		public override void SecondaryShootButtonPressed() { SecondaryShootButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
		public override void SecondaryShootButtonUp() { SecondaryShootButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

		public override void PauseButtonDown() { PauseButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
		public override void PauseButtonPressed() { PauseButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
		public override void PauseButtonUp() { PauseButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

		public override void TimeControlButtonDown() { TimeControlButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
		public override void TimeControlButtonPressed() { TimeControlButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
		public override void TimeControlButtonUp() { TimeControlButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

		public override void SwitchWeaponButtonDown()		{ SwitchWeaponButton.State.ChangeState (MMInput.ButtonStates.ButtonDown); }
		public override void SwitchWeaponButtonPressed()		{ SwitchWeaponButton.State.ChangeState (MMInput.ButtonStates.ButtonPressed); }
		public override void SwitchWeaponButtonUp()			{ SwitchWeaponButton.State.ChangeState (MMInput.ButtonStates.ButtonUp); }

		public override void SwitchCharacterButtonDown() { SwitchCharacterButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
		public override void SwitchCharacterButtonPressed() { SwitchCharacterButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
		public override void SwitchCharacterButtonUp() { SwitchCharacterButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }


        public virtual void SubmitButtonDown() { SubmitButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void SubmitButtonPressed() { SubmitButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void SubmitButtonUp() { SubmitButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }
        public virtual void CancelButtonDown() { CancelButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void CancelButtonPressed() { CancelButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void CancelButtonUp() { CancelButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }



        public InputMode _inputMode = InputMode.KeyBoardMouse;
        public InputPad _inputPad = InputPad.Unknown;

        public InputMode _beforeInputMode = InputMode.KeyBoardMouse;

        public InputDevice _beforeInputDevice;
        public InputDevice _inputDevice;
        public enum InputMode
        {
            KeyBoardMouse,
            Gamepad,

            Unknown
        }
        public enum InputPad
        {
            PS,
            Other,
            Unknown
        }
        public void CheckInputMode()
        {

            _beforeInputMode = _inputMode;
            if (actions.PlayerControls.Any.activeControl != null)
            {
                var device = actions.PlayerControls.Any.activeControl.device;
                _beforeInputDevice = _inputDevice;
                _inputDevice = device;
                if (device is Pointer ||
                    device is Mouse ||
                    device is Keyboard)
                {
                    _inputMode = InputMode.KeyBoardMouse;
                }
                else if (device is Gamepad)
                {
                    _inputMode = InputMode.Gamepad;
                    

                }
                else
                {

                }
            }

        }

		public InputMode GetInputMode()
		{
			return _inputMode;
		}

    }
}