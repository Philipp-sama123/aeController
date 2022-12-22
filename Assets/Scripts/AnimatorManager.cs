using UnityEngine;

public class AnimatorManager : MonoBehaviour {
    public Animator animator;
    private PlayerManager _playerManager;
    private PlayerLocomotion _playerLocomotion;

    public int IsUsingRootMotion { get; } = Animator.StringToHash("IsUsingRootMotion");

    // public int AddRootMotionVelocity { get; } = Animator.StringToHash("AddRootMotionVelocity");
    public int IsInAir { get; } = Animator.StringToHash("IsInAir");
    private int Vertical { get; } = Animator.StringToHash("Vertical");
    private int Horizontal { get; } = Animator.StringToHash("Horizontal");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        _playerManager = GetComponent<PlayerManager>();
        _playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    public void UpdateAnimatorValues(float horizontalMovement, float verticalMovement, bool isSprinting)
    {
        animator.SetFloat(Horizontal, isSprinting ? horizontalMovement * 2 : horizontalMovement, 0.1f, Time.deltaTime);
        animator.SetFloat(Vertical, isSprinting ? verticalMovement * 2 : verticalMovement, 0.1f, Time.deltaTime);
    }

    public void PlayTargetAnimation(string targetAnimation, bool useRootMotion = false)
    {
        animator.applyRootMotion = useRootMotion;
        animator.SetBool(IsUsingRootMotion, useRootMotion);
        // animator.SetBool(AddRootMotionVelocity, addRootMotionVelocity);
        animator.CrossFade(targetAnimation, 0.2f);
    }

    private void OnAnimatorMove()
    {
        if ( _playerManager.isUsingRootMotion == false )
            return;

        float delta = Time.deltaTime;
        Vector3 deltaPosition = animator.deltaPosition;
        Vector3 velocity = deltaPosition / delta;

        _playerLocomotion.rigidbody.drag = 0;
        _playerLocomotion.rigidbody.velocity = velocity;
    }

}