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
    [SerializeField] private float fallingForce = 100f;
    [SerializeField] private float groundDetectionRayStartPoint = .5f;
    [SerializeField] private float minimumDistanceNeededToBeginFall = 1f;
    [SerializeField] private float groundDirectionRayDistance = 0.25f;
    [SerializeField] private LayerMask groundLayer;
    private Vector3 normalVector;
    private Vector3 targetPosition;

    [Header("Movement Speeds")]
    [SerializeField] private float walkingSpeed = 2.5f;
    [SerializeField] private float runningSpeed = 5f;
    [SerializeField] private float sprintingSpeed = 7.5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Vector3 moveDirection;

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

        Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
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

    public void HandleFalling()
    {
        if ( _playerManager.isGrounded )
        {
            _animatorManager.animator.SetBool(_animatorManager.IsGrounded, true);
            return;
        }

        _animatorManager.animator.SetBool(_animatorManager.IsGrounded, false);
        rigidbody.AddForce(Vector3.down * fallingForce, ForceMode.Acceleration);

    }

    public void HandleJumping()
    {
        if ( _inputManager.jumpInput )
        {
            _animatorManager.PlayTargetAnimation("JumpingFull", true,true);
            // rigidbody.velocity += (Vector3.up * jumpSpeed);
            Debug.LogWarning("[ToDO]: HandleJumping properly.");
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

    public void CheckIfGrounded(float deltaTime)
    {
        RaycastHit hit;
        Vector3 direction = moveDirection;
        Vector3 currentPosition = transform.position;
        Vector3 origin = currentPosition;
        targetPosition = currentPosition;

        direction.Normalize();
        origin.y += groundDetectionRayStartPoint;
        origin = origin + direction * groundDirectionRayDistance;

        Debug.DrawRay(origin, -Vector3.up * minimumDistanceNeededToBeginFall, Color.red);
        if ( Physics.Raycast(origin, -Vector3.up, out hit, minimumDistanceNeededToBeginFall, groundLayer) )
        {
            Vector3 hitPoint = hit.point;
            normalVector = hit.normal;

            targetPosition.y = hitPoint.y;
            _playerManager.isGrounded = true;
        }
        else
        {
            if ( _playerManager.isGrounded )
            {
                _playerManager.isGrounded = false;
            }
        }
        AlignFeetToGround(deltaTime);
    }

    private void AlignFeetToGround(float deltaTime)
    {
        if ( !_playerManager.isUsingRootMotion && _playerManager.isGrounded )
        {
            Debug.LogWarning("Align Feet");
            transform.position = Vector3.Lerp(transform.position, targetPosition, deltaTime / 0.2f);
        }
    }
}