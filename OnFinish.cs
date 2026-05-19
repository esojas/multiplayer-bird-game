using UnityEngine;

public class OnFinish : StateMachineBehaviour
{

    [SerializeField] private string animation;

    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponentInParent<PlayerMovement>().ChangeCurrentAnimation(animation, 0.2f,stateInfo.length);
    }

}
