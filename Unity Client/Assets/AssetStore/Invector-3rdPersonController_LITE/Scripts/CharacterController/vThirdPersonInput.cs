using UnityEngine;

namespace Invector.vCharacterController
{
  public class vThirdPersonInput : MonoBehaviour
  {
    #region Variables       
    // Related Components
    Character m_Character;

    [HideInInspector] public vThirdPersonController cc;
    [HideInInspector] public vThirdPersonCamera tpCamera;
    [HideInInspector] public Camera cameraMain;

    // Inputs
    PlayerInputActions m_InputActions;
    bool m_Strafe = false;
    bool m_Sprint = false;
    Vector2 m_MovementInput;
    Vector2 m_MouseInput;

    #endregion

    private void Awake()
    {
      Cursor.lockState = CursorLockMode.Locked;
      m_Character = GetComponent<Character>();

      if (m_Character.m_Local)
      {
        m_InputActions = new PlayerInputActions();
        m_InputActions.PlayerControls.Jump.performed += ctx => JumpInput();
        m_InputActions.PlayerControls.Strafe.started += ctx => { m_Strafe = true; };
        m_InputActions.PlayerControls.Strafe.canceled += ctx => { m_Strafe = false; };
        m_InputActions.PlayerControls.Sprint.started += ctx => { m_Sprint = true; };
        m_InputActions.PlayerControls.Sprint.canceled += ctx => { m_Sprint = false; };
        m_InputActions.PlayerControls.Move.performed += ctx => { m_MovementInput = ctx.ReadValue<Vector2>(); };
        m_InputActions.PlayerControls.Rotate.performed += ctx => { m_MouseInput = ctx.ReadValue<Vector2>(); };
      }
    }

    protected virtual void Start()
    {
      InitilizeController();
      if (m_Character.m_Local)
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
      MoveInput();
      CameraInput();
      SprintInput();
      StrafeInput();
    }

    public virtual void MoveInput()
    {
      cc.input.x = m_MovementInput.x;
      cc.input.z = m_MovementInput.y;
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

      tpCamera.RotateCamera(m_MouseInput.x, m_MouseInput.y);
    }

    protected virtual void StrafeInput()
    {
      if (m_Strafe)
        cc.Strafe();
    }

    protected virtual void SprintInput()
    {
      cc.Sprint(m_Sprint);
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
      if (m_Character.m_Local && JumpConditions())
        cc.Jump();
    }

    #endregion

    // Also needed for inputs
    private void OnEnable()
    {
      m_InputActions.Enable();
    }

    private void OnDisable()
    {
      m_InputActions.Disable();
    }
  }
}