using UnityEngine;

public class CameraManager : MonoBehaviour {
    private PlayerManager _playerManager;
    
    public Transform cameraPivotTransform; //The object the camera uses to pivot (Look up and down)
    public Transform cameraTransform; //The transform of the actual camera object in the scene

    public LayerMask collisionLayers; //The layers we want our camera to collide with
    public LayerMask environmentLayer;
    
    private float _defaultPosition;
    private Vector3 _cameraFollowVelocity = Vector3.zero;
    private Vector3 _cameraVectorPosition;

    public float cameraCollisionOffSet = 0.2f; //How much the camera will jump off of objects its colliding with
    public float minimumCollisionOffSet = 0.2f;
    public float cameraCollisionRadius = 2;
    public float cameraFollowSpeed = 0.1f;
    public float cameraLookSpeed = 2;
    public float cameraPivotSpeed = 2;

    public float lookAngle; //Camera looking up and down
    public float pivotAngle; //Camera looking left and right
    public float minimumPivotAngle = -45;
    public float maximumPivotAngle = 45;
    private Vector3 cameraFollowVelocity;

    private void Awake()
    {
        _playerManager = FindObjectOfType<PlayerManager>();
        
        if ( Camera.main != null ) cameraTransform = Camera.main.transform;
        _defaultPosition = cameraTransform.localPosition.z;
        // collisionLayers = ~(1 << 8 | 1 << 9 | 1 << 10);
    }

    public void HandleAllCameraMovement(float deltaTime, float cameraInputX, float cameraInputY)
    {
        FollowTarget(deltaTime);
        RotateCamera(deltaTime, cameraInputX, cameraInputY);
    }

    private void FollowTarget(float deltaTime)
    {
        Vector3 targetPosition = Vector3.SmoothDamp
            (transform.position, _playerManager.transform.position, ref cameraFollowVelocity, deltaTime / cameraFollowSpeed);
    
        transform.position = targetPosition;
        // HandleCameraCollisions(deltaTime);
    }

    private void RotateCamera(float deltaTime, float cameraInputX, float cameraInputY)
    {
            lookAngle += (cameraInputX * cameraLookSpeed);
            pivotAngle -= (cameraInputY * cameraPivotSpeed);
            pivotAngle = Mathf.Clamp(pivotAngle, minimumPivotAngle, maximumPivotAngle);

            Vector3 rotation = Vector3.zero;
            rotation.y = lookAngle;
            Quaternion targetRotation = Quaternion.Euler(rotation);
            transform.rotation = targetRotation;

            rotation = Vector3.zero;
            rotation.x = pivotAngle;

            targetRotation = Quaternion.Euler(rotation);
            cameraPivotTransform.localRotation = targetRotation;
    }
}