using UnityEngine;


public class Attack2Behaviour : StateMachineBehaviour
{
    private PlayerCombat pc;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null)
            pc = animator.GetComponentInParent<PlayerCombat>();
        pc.swordTrail.Begin();
        pc.StartAttack2Effect();
        pc.SpawnSlashEffectCardinal();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        pc.swordTrail.End();
        pc.StopAttackEffect();
    }
}
