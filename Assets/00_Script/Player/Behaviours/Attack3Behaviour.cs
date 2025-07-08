using UnityEngine;

public class Attack3Behaviour : StateMachineBehaviour
{
    public float triggerTime = 0.7f;

    private PlayerCombat pc;
    private bool hasTriggered;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasTriggered = false;

        if (pc == null)
            pc = animator.GetComponentInParent<PlayerCombat>();

        pc.swordTrail.Begin();
        pc.StartAttack3Effect();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hasTriggered) return;

        float clipLength = stateInfo.length;
        float playedTime = (stateInfo.normalizedTime % 1f) * clipLength;

        if (playedTime >= triggerTime)
        {
            if (pc.fireSkill)
                pc.Attack3Effect();
            hasTriggered = true;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        pc.swordTrail.End();
        pc.StopAttackEffect();
    }
}
