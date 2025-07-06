using UnityEngine;


public class Attack2Behaviour : StateMachineBehaviour
{
    private PlayerCombat pc;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null)
            pc = animator.GetComponentInParent<PlayerCombat>();
        pc.StartAttack2Effect();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        pc.StopAttackEffect();
    }
}
