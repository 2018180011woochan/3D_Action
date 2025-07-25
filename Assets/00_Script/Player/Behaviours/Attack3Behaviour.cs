using UnityEngine;

public class Attack3Behaviour : StateMachineBehaviour
{
    public float triggerTime = 0.7f;

    private SwordCombat sc;
    private bool hasTriggered;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasTriggered = false;

        if (sc == null)
            sc = animator.GetComponentInParent<SwordCombat>();

        sc.swordTrail.Begin();
        sc.StartAttack3Effect();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hasTriggered) return;

        float clipLength = stateInfo.length;
        float playedTime = (stateInfo.normalizedTime % 1f) * clipLength;

        if (playedTime >= triggerTime)
        {
            if (sc.fireSkill)
                sc.Attack3Effect();
            hasTriggered = true;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        sc.swordTrail.End();
        sc.StopAttackEffect();
    }
}
