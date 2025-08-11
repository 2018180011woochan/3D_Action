using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DeathDissolveSwap : MonoBehaviour
{
    [Header("Hook")]
    public SkinnedMeshRenderer targetRenderer;   // RockGolemMesh
    public MonoBehaviour monsterState;           // MonsterState (isDead 읽기)

    [Header("Materials")]
    public Material dissolveTemplate;            // M_Golem_Dissolve
    public string dissolveProp = "_Dissolve";    // ShaderGraph Reference

    [Header("Timing")]
    public float waitBefore = 3f;
    public float dissolveDuration = 2f;
    public bool destroyAtEnd = true;

    Material[] originalShared;
    Material[] runtimeDissolve;
    bool started;

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        originalShared = targetRenderer ? targetRenderer.sharedMaterials : null;
    }

    void Update()
    {
        if (started || monsterState == null) return;

        var t = monsterState.GetType();
        var f = t.GetField("isDead");
        var p = t.GetProperty("isDead");
        bool dead =
            (f != null && f.FieldType == typeof(bool) && (bool)f.GetValue(monsterState)) ||
            (p != null && p.PropertyType == typeof(bool) && (bool)p.GetValue(monsterState));

        if (dead)
        {
            started = true;
            StartCoroutine(SwapAndDissolve());
        }
    }

    IEnumerator SwapAndDissolve()
    {
        yield return new WaitForSeconds(waitBefore);

        int count = targetRenderer.sharedMaterials.Length;
        runtimeDissolve = new Material[count];

        for (int i = 0; i < count; i++)
        {
            var src = originalShared[i];
            var dst = new Material(dissolveTemplate);

            // 필요한 속성만 안전하게 복사
            CopyProp(src, dst, "_BaseMap");        // Albedo
            CopyProp(src, dst, "_BaseColor");
            CopyProp(src, dst, "_BumpMap");        // Normal
            CopyProp(src, dst, "_OcclusionMap");   // AO
            CopyProp(src, dst, "_Metallic");       // Float
            CopyProp(src, dst, "_Smoothness");     // Float
            CopyProp(src, dst, "_EmissionMap");
            CopyProp(src, dst, "_EmissionColor");

            if (dst.HasProperty(dissolveProp)) dst.SetFloat(dissolveProp, 0f);

            runtimeDissolve[i] = dst;
        }

        targetRenderer.materials = runtimeDissolve;

        // 디졸브 진행
        float t = 0f;
        int id = Shader.PropertyToID(dissolveProp);
        while (t < dissolveDuration)
        {
            t += Time.deltaTime;
            float v = Mathf.Clamp01(t / dissolveDuration);
            for (int i = 0; i < runtimeDissolve.Length; i++)
                if (runtimeDissolve[i].HasProperty(id))
                    runtimeDissolve[i].SetFloat(id, v);
            yield return null;
        }

        if (destroyAtEnd) Destroy(gameObject);
        else targetRenderer.enabled = false;
    }

    // ▶ 핵심: 속성 타입에 맞춰 복사
    void CopyProp(Material src, Material dst, string prop)
    {
        if (src == null || dst == null) return;

        int iSrc = src.shader.FindPropertyIndex(prop);
        int iDst = dst.shader.FindPropertyIndex(prop);
        if (iSrc < 0 || iDst < 0) return;

        var tSrc = src.shader.GetPropertyType(iSrc);
        var tDst = dst.shader.GetPropertyType(iDst);
        if (tSrc != tDst) return; // 타입 다르면 패스

        switch (tSrc)
        {
            case ShaderPropertyType.Texture:
                var tex = src.GetTexture(prop);
                if (tex)
                {
                    dst.SetTexture(prop, tex);
                    // 스케일/오프셋까지 복사
                    dst.SetTextureScale(prop, src.GetTextureScale(prop));
                    dst.SetTextureOffset(prop, src.GetTextureOffset(prop));
                }
                break;
            case ShaderPropertyType.Color:
                dst.SetColor(prop, src.GetColor(prop));
                break;
            case ShaderPropertyType.Float:
            case ShaderPropertyType.Range:
                dst.SetFloat(prop, src.GetFloat(prop));
                break;
            case ShaderPropertyType.Vector:
                dst.SetVector(prop, src.GetVector(prop));
                break;
        }
    }
}
