using UnityEngine;

namespace Invector.vCharacterController
{
    public class vThirdPersonInput : MonoBehaviour
    {
        #region Variables       
        // Related Components
        Character mCharacter;

        [HideInInspector] public vThirdPersonController cc;
        [HideInInspector] public vThirdPersonCamera tpCamera;
        [HideInInspector] public Camera cameraMain;

        // Inputs
        PlayerInputActions2 inputActions;
        bool mStrafe = false;
        bool mSprint = false;
        Vector2 mMovementInput;
        Vector2 mMouseInput;

        #endregion

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            mCharacter = GetComponent<Character>();

            inputActions = new PlayerInputActions2();
            inputActions.PlayerControls.Jump.performed += ctx => JumpInput();
            inputActions.PlayerControls.Strafe.started += ctx => { mStrafe = true; };
            inputActions.PlayerControls.Strafe.canceled += ctx => { mStrafe = false; };
            inputActions.PlayerControls.Sprint.started += ctx => { mSprint = true; };
            inputActions.PlayerControls.Sprint.canceled += ctx => { mSprint = false; };
            inputActions.PlayerControls.Move.performed += ctx => { mMovementInput = ctx.ReadValue<Vector2>(); };
            inputActions.PlayerControls.Rotate.performed += ctx => { mMouseInput = ctx.ReadValue<Vector2>(); };
        }

        protected virtual void Start()
        {
            InitilizeController();
            if (mCharacter.Local)
                InitializeTpCamera();
        }

        protected virtual void FixedUpdate()
        {
            cc.UpdateMotor();               // updates the ThirdPersonMotor methods
            cc.ControlLocomotionType();     // handle the controller locomotion type and movespeed
            cc.ControlRotationType();       // handle the controller rotation type
        }

        protected virtual void Update()
        {
            InputHandle();                  // update the input methods
            cc.UpdateAnimator();            // updates the Animator Parameters
        }

        public virtual void OnAnimatorMove()
        {
            cc.ControlAnimatorRootMotion(); // handle root motion animations 
        }

        #region Basic Locomotion Inputs

        protected virtual void InitilizeController()
        {
            cc = GetComponent<vThirdPersonController>();

            if (cc != null)
                cc.Init();
        }

        protected virtual void InitializeTpCamera()
        {
            if (tpCamera == null)
            {
                tpCamera = FindObjectOfType<vThirdPersonCamera>();
                if (tpCamera == null)
                    return;
                if (tpCamera)
                {
                    tpCamera.SetMainTarget(this.transform);
                    tpCamera.Init();
                }
            }
        }

        protected virtual void InputHandle()
        {
            if (!mCharacter.Local)
                return; //TODO: Add non main character movements (from server updates)
            MoveInput();
            CameraInput();
            SprintInput();
            StrafeInput();
        }

        public virtual void MoveInput()
        {
            cc.input.x = mMovementInput.x;
            cc.input.z = mMovementInput.y;
            Debug.Log(mMovementInput);
        }

        protected virtual void CameraInput()
        {
            if (!cameraMain)
            {
                if (!Camera.main) Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
                else
                {
                    cameraMain = Camera.main;
                    cc.rotateTarget = cameraMain.transform;
                }
            }

            if (cameraMain)
            {
                cc.UpdateMoveDirection(cameraMain.transform);
            }

            if (tpCamera == null)
                return;

            tpCamera.RotateCamera(mMouseInput.x, mMouseInput.y);
        }

        protected virtual void StrafeInput()
        {
            if (mStrafe)
                cc.Strafe();
        }

        protected virtual void SprintInput()
        {
            cc.Sprint(mSprint);
        }

        /// <summary>
        /// Conditions to trigger the Jump animation & behavior
        /// </summary>
        /// <returns></returns>
        protected virtual bool JumpConditions()
        {
            return cc.isGrounded && cc.GroundAngle() < cc.slopeLimit && !cc.isJumping && !cc.stopMove;
        }

        /// <summary>
        /// Input to trigger the Jump 
        /// </summary>
        protected virtual void JumpInput()
        {
            if (JumpConditions())
                cc.Jump();
        }

        #endregion

        // Also needed for inputs
        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }
    }
}