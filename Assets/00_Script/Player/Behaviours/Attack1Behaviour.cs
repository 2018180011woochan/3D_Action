using UnityEngine;

public class Attack1Behaviour : StateMachineBehaviour
{
    private PlayerCombat pc;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null)
            pc = animator.GetComponentInParent<PlayerCombat>();

        pc.StartAttack1Effect();
        pc.swordTrail.Begin();
        pc.Attack1Effect();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        pc.StopAttackEffect();
        pc.swordTrail.End();
    }
}