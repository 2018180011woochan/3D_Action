using UnityEngine;

public class Attack3Behaviour : StateMachineBehaviour
{
    private PlayerCombat pc;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null)
            pc = animator.GetComponentInParent<PlayerCombat>();
        pc.StartAttack3Effect();
    }
}
