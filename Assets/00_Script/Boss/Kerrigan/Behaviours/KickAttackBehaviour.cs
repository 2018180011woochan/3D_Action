using UnityEngine;

public class KickAttackBehaviour : StateMachineBehaviour
{
    [Header("히트박스 타이밍 (초)")]
    public float hitTimeStart = 0.3f;  
    public float hitTimeEnd = 0.35f; 

    Collider hitBoxCollider;
    bool hasActivated;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var hitBoxScript = animator.transform.root
                                 .GetComponentInChildren<KickAttackHitBox>(true);
        if (hitBoxScript != null)
        {
            hitBoxCollider = hitBoxScript.GetComponent<Collider>();
            if (hitBoxCollider == null)
                Debug.LogWarning("KickAttackHitBox에 Collider가 없습니다!");
            else
                hitBoxCollider.enabled = false;
        }
        else
        {
            Debug.LogError("KickAttackHitBox 스크립트를 찾을 수 없음");
        }

        hasActivated = false;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hitBoxCollider == null) return;

        float clipTime = (stateInfo.normalizedTime % 1f) * stateInfo.length;

        if (!hasActivated && clipTime >= hitTimeStart && clipTime < hitTimeEnd)
        {
            hitBoxCollider.enabled = true;
            hasActivated = true;
        }
        else if (hasActivated && clipTime >= hitTimeEnd)
        {
            hitBoxCollider.enabled = false;
        }
    }


    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hitBoxCollider != null)
            hitBoxCollider.enabled = false;
    }
}
