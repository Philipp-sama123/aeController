using UnityEngine;

namespace _AnimatorScripts {
    public class ResetBool : StateMachineBehaviour {
        public string isUsingRootMotionBool = "IsUsingRootMotion";
        public bool isUsingRootMotionStatus = false;
        public string addRootMotionVelocityBool = "AddRootMotionVelocity";
        public bool addRootMotionVelocityStatus = false;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public override void OnStateEnter(UnityEngine.Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(isUsingRootMotionBool, isUsingRootMotionStatus);
            animator.SetBool(addRootMotionVelocityBool, addRootMotionVelocityStatus);
        }

    }
}