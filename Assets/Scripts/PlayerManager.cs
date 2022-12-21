using UnityEngine;

// calls and runs everything 
//runs all functionality for the player
public class PlayerManager : CharacterManager {

    private InputManager _inputManager;
    private CameraManager _cameraManager;
    private AnimatorManager _animatorManager;
    private PlayerLocomotion _playerLocomotion;


    [Header("Player Flags")]
    public bool isUsingRootMotion;
    public bool isSprinting;
    public bool isGrounded;
    public bool addRootMotionVelocity;

    public bool isInAir;
    // public bool canDoCombo;
    // public bool isUsingRightHand;
    // public bool isUsingLeftHand;
    // public bool isInvulnerable;

    private void Awake()
    {
        _inputManager = GetComponent<InputManager>();
        _playerLocomotion = GetComponent<PlayerLocomotion>();
        _animatorManager = GetComponent<AnimatorManager>();

        _cameraManager = FindObjectOfType<CameraManager>();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        _inputManager.HandleAllInputs(deltaTime);

        _playerLocomotion.HandleJumping();
        _playerLocomotion.HandleRollingAndSprinting();

        _animatorManager.animator.SetBool(_animatorManager.IsInAir, isInAir);
        isUsingRootMotion = _animatorManager.animator.GetBool(_animatorManager.IsUsingRootMotion);
        // addRootMotionVelocity = _animatorManager.animator.GetBool(_animatorManager.AddRootMotionVelocity);
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.deltaTime;

        _playerLocomotion.HandleMovement();
        _playerLocomotion.HandleFalling(deltaTime, _playerLocomotion.moveDirection);

        _playerLocomotion.HandleRotation(deltaTime);

    }

    private void LateUpdate()
    {
        float deltaTime = Time.deltaTime;

        if ( _cameraManager != null )
            _cameraManager.HandleAllCameraMovement(deltaTime, _inputManager.cameraInputX, _inputManager.cameraInputY);
        else Debug.LogWarning("[Error] No Camera found!");

        _inputManager.dodgeFlag = false;

    }
}