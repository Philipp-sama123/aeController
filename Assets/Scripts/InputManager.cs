using UnityEngine;

public class InputManager : MonoBehaviour {
    private ThirdPersonControls _playerControls;
    private PlayerManager _playerManager;
    private CameraManager _cameraManager;
    
    [Header("Movement")]
    public Vector2 movementInput;
    public float horizontalInput;
    public float verticalInput;
    public float moveAmount;

    [Header("Camera")]
    public Vector2 cameraInput;
    public float cameraInputX;
    public float cameraInputY;

    [Header("Jump and Dodge")]
    public bool dodgeAndSprintInput;
    public bool jumpInput;
    public bool sprintFlag;
    public bool dodgeFlag;
    public float rollInputTimer;

    /** Lock On **/
    public bool lockOnInput;
    public bool lockOnFlag;
    public bool rightStickLeftInput;
    public bool rightStickRightInput;

    private void Awake()
    {
        _playerManager = GetComponent<PlayerManager>();
        _cameraManager = FindObjectOfType<CameraManager>();
    }

    private void OnEnable()
    {
        if ( _playerControls == null )
        {
            _playerControls = new ThirdPersonControls();

            _playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
            _playerControls.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();

            // while you hold it --> true!
            _playerControls.PlayerActions.Sprint.performed += i => dodgeAndSprintInput = true;
            _playerControls.PlayerActions.Sprint.canceled += i => dodgeAndSprintInput = false;
            // when you press the button --> True
            _playerControls.PlayerActions.Jump.performed += i => jumpInput = true;
            _playerControls.PlayerActions.Jump.canceled += i => jumpInput = false;
        }
        _playerControls.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }

    public void HandleAllInputs(float deltaTime)
    {
        HandleMovementInput();
        HandleRollAndSprintInput(deltaTime);
    }

    private void HandleMovementInput()
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;

        cameraInputY = cameraInput.y;
        cameraInputX = cameraInput.x;

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));

    }

    private void HandleRollAndSprintInput(float deltaTime)
    {
        if ( dodgeAndSprintInput )
        {
            rollInputTimer += deltaTime;
            // if ( playerStats.currentStamina <= 0 )
            // {
            // dodgeAndSprintInput = false;
            // sprintFlag = false;
            // }

            if ( moveAmount > 0.5f /*&& playerStats.currentStamina > 0*/ )
            {
                sprintFlag = true;
            }
        }
        else
        {
            sprintFlag = false;
            if ( rollInputTimer > 0 && rollInputTimer < 0.5f )
            {
                dodgeFlag = true;
            }
            rollInputTimer = 0;
        }

        // if ( dodgeAndSprintInput && verticalInput > 0.5f )
        // {
        //     _playerManager.isSprinting = true;
        // }
        // else
        // {
        //     _playerManager.isSprinting = false;
        // }
    }
}