using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class DamageScreenEffect : MonoBehaviour
{
    [Header("GlobalPost�� ���� Volume ������Ʈ")]
    public Volume volume;

    Vignette vignette;
    DepthOfField dof;
    float originalVigIntensity;
    float originalFocusDist;

    void Awake()
    {
        // volume �ʵ忡 �����Ϳ��� �Ҵ����� �ʾҴٸ� ������ ù ��° Volume ��������
        if (volume == null)
            volume = FindObjectOfType<Volume>();

        // Vignette ���� ĳ��
        if (volume.profile.TryGet<Vignette>(out vignette))
            originalVigIntensity = vignette.intensity.value;
        else
            Debug.LogError("�� Volume Profile�� Vignette Override�� �����ϴ�!");

        // Depth Of Field ���� ĳ�� (���Ѵٸ� ���)
        if (volume.profile.TryGet<DepthOfField>(out dof))
            originalFocusDist = dof.focusDistance.value;
        else
            Debug.LogWarning("�� Volume Profile�� DepthOfField Override�� �����ϴ�.");
    }

    /// <summary>
    /// �� �� �ǰ� ȭ�� ����Ʈ Ʈ����
    /// </summary>
    public void TriggerHitEffect()
    {
        StopAllCoroutines();
        StartCoroutine(HitRoutine());
    }

    IEnumerator HitRoutine()
    {
        // 1) ���������� ����� £��
        vignette.intensity.value = 0.6f;

        // (���Ѵٸ� DOF�� ���� ��ȭ)
        if (dof != null)
            dof.focusDistance.value = 2f;

        // 0.1�� ����
        yield return new WaitForSeconds(0.1f);

        // 2) ������ �⺻������ ����
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
