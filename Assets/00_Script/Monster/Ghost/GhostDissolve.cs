using System.Collections;
using UnityEngine;

public class GhostDissolve : MonoBehaviour
{
    [Header("Shader Property")]
    [SerializeField] string dissolveProp = "_Dissolve"; // Shader Graph의 Reference 이름
    [SerializeField] float startValue = 1f;             // 평소 값
    [SerializeField] float endValue = 0f;               // 사망 시 값(=0)
    [SerializeField] float duration = 2f;               // 서서히 사라질 시간(즉시면 0)

    Renderer[] rends;
    MaterialPropertyBlock block;
    int propId;

    void Awake()
    {
        // 스키닝 메쉬까지 전부
        rends = GetComponentsInChildren<Renderer>(true);
        block = new MaterialPropertyBlock();
        propId = Shader.PropertyToID(dissolveProp);
    }

    public void Play()
    {
        if (duration <= 0f) { SetAll(endValue); return; }
        StopAllCoroutines();
        StartCoroutine(CoDissolve());
    }

    IEnumerator CoDissolve()
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float v = Mathf.Lerp(startValue, endValue, t / duration);
            SetAll(v);
            yield return null;
        }
        SetAll(endValue);
    }

    void SetAll(float v)
    {
        foreach (var r in rends)
        {
            // 머테리얼이 해당 프로퍼티를 가질 때만 적용(안전)
            var mat = r.sharedMaterial;
            if (mat != null && mat.HasProperty(propId))
            {
                r.GetPropertyBlock(block);
                block.SetFloat(propId, v);
                r.SetPropertyBlock(block);
            }
        }
    }
}
