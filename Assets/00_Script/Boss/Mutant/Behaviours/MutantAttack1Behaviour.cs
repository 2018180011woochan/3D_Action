using UnityEngine;

public class MutantAttack1Behaviour : StateMachineBehaviour
{
    public string hitboxObjectName = "Character1_RightHand";
    public int windowStartFrame = 11;
    public int windowEndFrame = 15;

    Transform hitBox;              // ★ GameObject를 켜고 끈다
    AttackHitBox hit;
    float startN, endN;
    bool active;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!hitBox)
        {
            // 경로로 찾기 → 실패 시 이름으로 전역 탐색
            var t = animator.transform.Find(hitboxObjectName);
            if (!t)
                foreach (var tr in animator.GetComponentsInChildren<Transform>(true))
                    if (tr.name == hitboxObjectName) { t = tr; break; }

            if (t) { hitBox = t; hit = t.GetComponent<AttackHitBox>(); }
        }

        if (hitBox) hitBox.gameObject.SetActive(false); // 기본 OFF
        active = false;

        // 프레임 → 정규화 구간 계산
        var infos = animator.GetCurrentAnimatorClipInfo(layerIndex);
        if (infos.Length > 0 && infos[0].clip)
        {
            var clip = infos[0].clip;
            int totalFrames = Mathf.Max(1, Mathf.RoundToInt(clip.length * clip.frameRate));
            startN = Mathf.Clamp01(windowStartFrame / (float)totalFrames);
            endN = Mathf.Clamp01(windowEndFrame / (float)totalFrames);
        }
        else
        {
            // 예비: 60fps 가정
            float total = stateInfo.length * 60f;
            startN = Mathf.Clamp01(windowStartFrame / total);
            endN = Mathf.Clamp01(windowEndFrame / total);
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!hitBox) return;

        float norm = stateInfo.normalizedTime - Mathf.Floor(stateInfo.normalizedTime);
        bool inWindow = norm >= startN && norm <= endN;

        if (inWindow && !active)
        {
            hitBox.gameObject.SetActive(true);   // ★ AttackHitBox.OnEnable() → hasSwing 리셋
            active = true;
        }
        else if (!inWindow && active)
        {
            hitBox.gameObject.SetActive(false);
            active = false;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hitBox) hitBox.gameObject.SetActive(false);
        active = false;
    }
}
