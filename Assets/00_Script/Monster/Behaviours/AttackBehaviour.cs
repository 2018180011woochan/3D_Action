using UnityEngine;

public class AttackBehaviour : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var hitBox = animator.transform.Find("AttackHitBox");
        if (hitBox) hitBox.gameObject.SetActive(true);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var hitBox = animator.transform.Find("AttackHitBox");
        if (hitBox) hitBox.gameObject.SetActive(false);
    }
}
