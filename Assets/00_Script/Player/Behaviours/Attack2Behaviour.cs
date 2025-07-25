using UnityEngine;


public class Attack2Behaviour : StateMachineBehaviour
{
    private SwordCombat sc;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (sc == null)
            sc = animator.GetComponentInParent<SwordCombat>();
        sc.swordTrail.Begin();
        sc.StartAttack2Effect();
        if (sc.fireSkill)
            sc.Attack2Effect();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        sc.swordTrail.End();
        sc.StopAttackEffect();
    }
}
