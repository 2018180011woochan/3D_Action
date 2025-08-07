using UnityEngine;

public class DrawStatement : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SamuraiMovement movementScript = animator.GetComponent<SamuraiMovement>();

        if (movementScript != null)
        {
            movementScript.OnDrawEnd();
        }
        else
        {
            Debug.LogError("SamuraiMovement 스크립트를 찾을 수 없습니다!");
        }
    }
}
