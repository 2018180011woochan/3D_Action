using UnityEngine;

public class Attack1Behaviour : StateMachineBehaviour
{
    private SwordCombat sc;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (sc == null)
            sc = animator.GetComponentInParent<SwordCombat>();

        sc.StartAttack1Effect();
        sc.swordTrail.Begin();

        if (sc.fireSkill)
            sc.Attack1Effect();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        sc.StopAttackEffect();
        sc.swordTrail.End();
    }
}