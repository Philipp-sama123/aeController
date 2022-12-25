using System.Collections;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour {

    private AnimatorManager _animatorManager;
    private PlayerManager _playerManager;
    private InputManager _inputManager;

    public new Rigidbody rigidbody;

    private Vector3 _moveDirection;
    private Camera _mainCamera;

    [Header("Falling")]
    [SerializeField] private float groundDetectionRayStartPoint = .5f;
    [SerializeField] private float minimumDistanceNeededToBeginFall = 1f;
    [SerializeField] private float groundDirectionRayDistance = 0.25f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float fallingSpeed = 25f;
    [SerializeField] private float leapingVelocity = 2.5f;
    public int inAirTimer;

    private Vector3 _normalVector;
    private Vector3 _targetPosition;

    [Header("Movement Speeds")]
    [SerializeField] private float walkingSpeed = 2;
    [SerializeField] private float runningSpeed = 4f;
    [SerializeField] private float sprintingSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] public Vector3 moveDirection;

    [Header("Jump Speeds")]
    [SerializeField] private float jumpSpeed = 100f;

    private void Awake()
    {
        if ( Camera.main != null ) _mainCamera = Camera.main;
        else Debug.LogWarning("[Not Assigned]: Camera");

        _animatorManager = GetComponent<AnimatorManager>();
        _playerManager = GetComponent<PlayerManager>();
        _inputManager = GetComponent<InputManager>();
        rigidbody = GetComponent<Rigidbody>();
    }



    public void HandleMovement()
    {
        //normalVector = transform.up; // ToDo !

        // if ( _playerManager.isGrounded == false ) return;
        Transform mainCameraTransform = _mainCamera.transform;
        float speed = runningSpeed;

        moveDirection = mainCameraTransform.forward * _inputManager.verticalInput;
        moveDirection += mainCameraTransform.right * _inputManager.horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

        if ( _inputManager.dodgeAndSprintInput && _inputManager.moveAmount > 0.5f )
        {
            speed = sprintingSpeed;
            _playerManager.isSprinting = true;
            moveDirection *= speed;
        }
        else
        {
            if ( _inputManager.moveAmount < 0.5f )
            {
                moveDirection *= walkingSpeed;
                _playerManager.isSprinting = false;
            }
            else
            {
                moveDirection *= speed;
                _playerManager.isSprinting = false;
            }
        }

        Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, _normalVector);
        rigidbody.velocity = projectedVelocity;
        _animatorManager.UpdateAnimatorValues(0, _inputManager.moveAmount, _playerManager.isSprinting);

    }

    public void HandleRotation(float deltaTime)
    {
        if ( _inputManager.moveAmount == 0 ) return;

        Vector3 targetDirection;
        Transform mainCameraTransform = _mainCamera.transform;

        targetDirection = mainCameraTransform.forward * _inputManager.verticalInput;
        targetDirection += mainCameraTransform.right * _inputManager.horizontalInput;

        targetDirection.Normalize();
        targetDirection.y = 0; // no movement on y-Axis (!)

        if ( targetDirection == Vector3.zero )
            targetDirection = transform.forward;

        float rs = rotationSpeed;

        Quaternion tr = Quaternion.LookRotation(targetDirection);
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rs * deltaTime);

        transform.rotation = targetRotation;
    }



    public void HandleJumping()
    {
        if ( _inputManager.jumpInput )
        {
            _animatorManager.PlayTargetAnimation("JumpingFull", true, true);
            StartCoroutine(AddJumpAcceleration());
        }
    }


    public void HandleRollingAndSprinting()
    {
        if ( _inputManager.dodgeFlag )
        {
            Transform mainCameraTransform = _mainCamera.transform;
            moveDirection = mainCameraTransform.forward * _inputManager.verticalInput;
            moveDirection += mainCameraTransform.right * _inputManager.horizontalInput;

            if ( _inputManager.moveAmount > 0 )
            {
                _animatorManager.PlayTargetAnimation("Dodge Forward", true); // Todo: rename Anim
                moveDirection.y = 0;

                Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = rollRotation;
            }
            else
            {
                _animatorManager.PlayTargetAnimation("Dodge Backward", true); // Todo: rename Anim
                moveDirection.y = 0;
            }
        }
    }

    public void HandleFalling(float deltaTime)
    {
        _playerManager.isGrounded = false;
        rigidbody.useGravity = true;

        RaycastHit hit;
        Vector3 origin = transform.position;
        origin.y += groundDetectionRayStartPoint;

        // if ( Physics.Raycast(origin, transform.forward, out hit, 0.4f) )
        // {
        //     movementDirection = Vector3.zero;
        // }

        if ( _playerManager.isInAir )
        {
            inAirTimer++;
            rigidbody.AddForce(Vector3.forward * leapingVelocity, ForceMode.Impulse);
            rigidbody.AddForce(Vector3.down * fallingSpeed * 9.8f * inAirTimer * deltaTime, ForceMode.Acceleration);
        }

        Vector3 dir = moveDirection;
        dir.Normalize();
        origin = origin + dir * groundDirectionRayDistance;

        _targetPosition = transform.position;

        Debug.DrawRay(origin, -Vector3.up * minimumDistanceNeededToBeginFall, Color.red);
        if ( Physics.Raycast(origin, -Vector3.up, out hit, minimumDistanceNeededToBeginFall, groundLayer) )
        {
            _normalVector = hit.normal;
            Vector3 tp = hit.point;
            _playerManager.isGrounded = true;
            _targetPosition.y = tp.y;

            if ( _playerManager.isInAir )
            {
                if ( inAirTimer > 0.5f )
                {
                    Debug.Log("[Info] Landing You were in the air for " + inAirTimer);
                    _animatorManager.PlayTargetAnimation("Landing", true);
                    inAirTimer = 0;
                }
                else
                {
                    Debug.Log("[Info] EMPTY You were in the air for " + inAirTimer);
                    _animatorManager.PlayTargetAnimation("Empty", false);
                    inAirTimer = 0;

                }

                _playerManager.isInAir = false;
            }
        }
        else
        {
            if ( _playerManager.isGrounded )
            {
                _playerManager.isGrounded = false;
            }

            if ( _playerManager.isInAir == false )
            {
                if ( _playerManager.isUsingRootMotion == false )
                {
                    _animatorManager.PlayTargetAnimation("Falling", false);
                }
                _playerManager.isInAir = true;

                Vector3 vel = rigidbody.velocity;
                vel.Normalize();
                rigidbody.velocity = vel * (runningSpeed / 2);
                _playerManager.isInAir = true;
            }
        }

        if ( _playerManager.isGrounded && !_playerManager.isUsingRootMotion )
            transform.position = Vector3.Lerp(transform.position, _targetPosition, deltaTime / .2f);
    }

}