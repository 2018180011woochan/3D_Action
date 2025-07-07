using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class DamageScreenEffect : MonoBehaviour
{
    [Header("GlobalPost에 붙은 Volume 컴포넌트")]
    public Volume volume;

    Vignette vignette;
    DepthOfField dof;
    float originalVigIntensity;
    float originalFocusDist;

    void Awake()
    {
        // volume 필드에 에디터에서 할당하지 않았다면 씬에서 첫 번째 Volume 가져오기
        if (volume == null)
            volume = FindObjectOfType<Volume>();

        // Vignette 세팅 캐시
        if (volume.profile.TryGet<Vignette>(out vignette))
            originalVigIntensity = vignette.intensity.value;
        else
            Debug.LogError("▶ Volume Profile에 Vignette Override가 없습니다!");

        // Depth Of Field 세팅 캐시 (원한다면 사용)
        if (volume.profile.TryGet<DepthOfField>(out dof))
            originalFocusDist = dof.focusDistance.value;
        else
            Debug.LogWarning("▶ Volume Profile에 DepthOfField Override가 없습니다.");
    }

    /// <summary>
    /// 한 번 피격 화면 이펙트 트리거
    /// </summary>
    public void TriggerHitEffect()
    {
        StopAllCoroutines();
        StartCoroutine(HitRoutine());
    }

    IEnumerator HitRoutine()
    {
        // 1) 순간적으로 비네팅 짙게
        vignette.intensity.value = 0.6f;

        // (원한다면 DOF도 같이 변화)
        if (dof != null)
            dof.focusDistance.value = 2f;

        // 0.1초 유지
        yield return new WaitForSeconds(0.1f);

        // 2) 서서히 기본값으로 복구
        float t = 0f, dur = 0.3f;
        float startV = vignette.intensity.value;
        float startF = dof != null ? dof.focusDistance.value : 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(startV, originalVigIntensity, t / dur);
            if (dof != null)
                dof.focusDistance.value = Mathf.Lerp(startF, originalFocusDist, t / dur);
            yield return null;
        }
    }
}
