using UnityEngine;

public class Attack2State : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SamuraiCombat combatScript = animator.GetComponent<SamuraiCombat>();

        if (combatScript != null)
        {
            combatScript.OnAttackEnd();
        }
        else
        {
            Debug.LogError("SamuraiMovement 스크립트를 찾을 수 없습니다!");
        }
    }
}
