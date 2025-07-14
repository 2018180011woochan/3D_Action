using UnityEngine;

public class SwingAttackBehaviour : StateMachineBehaviour
{
    [Header("히트박스 타이밍 설정")]
    public float hitTimeStart = 0.12f;
    public float hitTimeEnd = 0.16f;

    Transform hitBox;
    Collider hitBoxCollider;
    bool isEnabled;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 히트박스 스크립트 찾기 (비활성 상태 오브젝트 포함)
        var hitBoxScript = animator.transform.root
                                 .GetComponentInChildren<SwingAttackHitBox>(includeInactive: true);

        if (hitBoxScript != null)
        {
            hitBox = hitBoxScript.transform;
            hitBoxCollider = hitBox.GetComponent<Collider>();

            if (hitBoxCollider == null)
                Debug.LogWarning("히트박스 오브젝트에 Collider가 없습니다!");
            else
                hitBoxCollider.enabled = false;    

            Debug.Log("스윙어택박스 스크립트 찾음ㅋ");
        }
        else
        {
            Debug.LogError("스윙어택박스 스크립트 찾을수없음");
        }

        isEnabled = false;
    }

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
        }
        // 히트박스 끄기
        else if (isEnabled && currentTime >= hitTimeEnd)
        {
            hitBoxCollider.enabled = false;
            isEnabled = false;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 상태 종료 시에도 콜라이더 꺼두기
        if (hitBoxCollider != null)
            hitBoxCollider.enabled = false;
    }
}
