using UnityEngine;

public class HookAttackBehaviour : StateMachineBehaviour
{
    [Header("히트박스 타이밍 설정")]
    public float hitTimeStart = 0.12f;   
    public float hitTimeEnd = 0.20f;    

    Transform hitBox;
    Collider hitBoxCollider;
    bool isEnabled;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //var hitBoxScript = animator.GetComponentInChildren<AttackHitBox>();
        var hitBoxScript = animator.transform.root
                                 .GetComponentInChildren<AttackHitBox>(includeInactive: true);


        if (hitBoxScript != null)
        {
            hitBox = hitBoxScript.transform;
            hitBoxCollider = hitBox.GetComponent<Collider>();

            if (hitBoxCollider == null)
                Debug.LogWarning("히트박스 오브젝트에 Collider가 없습니다!");
            else
                hitBoxCollider.enabled = false;

            Debug.Log("어택박스 스크립트 찾음ㅋ");
        }
        else
            Debug.Log("어택히트박스 찾을수없음");
        isEnabled = false;
    }

    // 애니메이션 재생 중
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hitBoxCollider == null) return;

        float normalizedTime = stateInfo.normalizedTime % 1f;
        float currentTime = normalizedTime * stateInfo.length;

        // 히트박스 켜기
        if (!isEnabled && currentTime >= hitTimeStart && currentTime < hitTimeEnd)
        {
            hitBoxCollider.enabled = true;
            isEnabled = true;
            Debug.Log($"히트박스 ON at {currentTime:F2}초");
        }
        // 히트박스 끄기
        else if (isEnabled && currentTime >= hitTimeEnd)
        {
            hitBoxCollider.enabled = false;
            isEnabled = false;
            Debug.Log($"히트박스 OFF at {currentTime:F2}초");
        }
    }

    // 애니메이션 상태 종료 시
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hitBoxCollider != null)
            hitBoxCollider.enabled = false;

    }
}
