using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantSpawnDissolve : MonoBehaviour
{
    public Material dissolveMaterial;

    public Material finalMaterial;

    public string dissolveProperty = "_Dissolve";
    public float startValue = 1f;        // 소환 시 시작값
    public float endValue = 0f;          // 0으로 내려가며 등장
    public float duration = 1f;          // 1초

    public float outDelay = 5f;      // 죽은 뒤 대기 시간
    public float outDuration = 1f;   // 사라질 때 디졸브 시간
    public bool destroyAfterOut = true;

    // 내부
    Renderer[] rends;
    int propId;
    struct Slot { public Renderer r; public int index; }
    List<Slot> targetSlots = new List<Slot>();

    void Awake()
    {
        rends = GetComponentsInChildren<Renderer>(true);
        propId = Shader.PropertyToID(dissolveProperty);

        foreach (var r in rends)
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (m == null) continue;

                bool match = (dissolveMaterial != null && m == dissolveMaterial) ||
                             (m.HasProperty(propId)); // 안전망

                if (match)
                {
                    targetSlots.Add(new Slot { r = r, index = i });
                }
            }
        }
    }

    void OnEnable()
    {
        // 초기값 설정
        SetValueForAll(startValue);
        // 애니 시작
        StopAllCoroutines();
        StartCoroutine(CoDissolve());
    }

    IEnumerator CoDissolve()
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float v = Mathf.Lerp(startValue, endValue, Mathf.Clamp01(t / duration));
            SetValueForAll(v);
            yield return null;
        }
        SetValueForAll(endValue);

        // 끝나면 지정한 슬롯의 머티리얼을 원래 머티리얼로 교체
        if (finalMaterial)
            SwapToFinalMaterial();
    }

    void SetValueForAll(float v)
    {
        foreach (var slot in targetSlots)
        {
            if (slot.r == null) continue;

            // submesh별 PropertyBlock 사용
            var block = new MaterialPropertyBlock();
            slot.r.GetPropertyBlock(block, slot.index);
            block.SetFloat(propId, v);
            slot.r.SetPropertyBlock(block, slot.index);
        }
    }

    void SwapToFinalMaterial()
    {
        foreach (var slot in targetSlots)
        {
            if (slot.r == null) continue;

            var mats = slot.r.materials;      // 인스턴스용 배열(이 오브젝트만 변경)
            if (slot.index >= 0 && slot.index < mats.Length)
                mats[slot.index] = finalMaterial;

            slot.r.materials = mats;

            var block = new MaterialPropertyBlock();
            slot.r.SetPropertyBlock(block, slot.index);
        }
    }

    public void PlayOut(float delay = -1f, float durationOverride = -1f)
    {
        // 최종 머티리얼 → 디졸브 머티리얼로 되돌리고, 값 0에서 시작
        SetToDissolveMaterial();
        SetValueForAll(0f);

        if (delay < 0f) delay = outDelay;
        if (durationOverride > 0f) outDuration = durationOverride;

        StopAllCoroutines();
        StartCoroutine(CoDissolveOut(delay));
    }

    IEnumerator CoDissolveOut(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        float t = 0f;
        while (t < outDuration)
        {
            t += Time.deltaTime;
            float v = Mathf.Lerp(0f, 1f, Mathf.Clamp01(t / outDuration)); // 0 → 1
            SetValueForAll(v);
            yield return null;
        }
        SetValueForAll(1f);

        if (destroyAfterOut)
            Destroy(gameObject);
    }

    // 최종 머티리얼로 바꿔놨던 슬롯들을 다시 디졸브 머티리얼로 교체
    void SetToDissolveMaterial()
    {
        foreach (var slot in targetSlots)
        {
            if (slot.r == null) continue;

            var mats = slot.r.materials;   // 인스턴스 배열(이 오브젝트만 변경)
            if (slot.index >= 0 && slot.index < mats.Length)
                mats[slot.index] = dissolveMaterial;

            slot.r.materials = mats;

            // 기존 PropertyBlock 잔여치 제거
            var block = new MaterialPropertyBlock();
            slot.r.SetPropertyBlock(block, slot.index);
        }
    }
}
