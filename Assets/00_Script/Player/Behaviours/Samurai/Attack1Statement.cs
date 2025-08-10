using UnityEngine;

public class Attack1Statement : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SamuraiCombat combatScript = animator.GetComponent<SamuraiCombat>();

        if (combatScript != null)
        {
            combatScript.OnAttackEnd();
        }
    }
}
