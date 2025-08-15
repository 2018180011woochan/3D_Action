using UnityEngine;

public class WarningEffect : MonoBehaviour
{
    [Header("Shape")]
    public float radius = 6f;
    public float angleDeg = 90f;
    [Range(8, 128)] public int segments = 48;
    public float yOffset = 0.02f; // 바닥과 겹침 방지

    [Header("Visuals")]
    public Material baseMaterial;
    public Color lightRed = new Color(1, 0, 0, 0.25f); // 바닥
    public Color darkRed = new Color(1, 0, 0, 0.7f);  // 채움

    [Header("Test Fill")]
    [Range(0, 1)] public float fill01 = 0f; // 인스펙터에서 직접 올려보기

    Transform fill; // 진한 채움 Transform

    void Awake()
    {
        var mesh = BuildSectorMesh(radius, angleDeg, segments);

        // 밝은 바닥
        var baseGO = new GameObject("WarnBase");
        baseGO.transform.SetParent(transform, false);
        baseGO.transform.localPosition = Vector3.up * yOffset;
        var mf = baseGO.AddComponent<MeshFilter>(); mf.sharedMesh = mesh;
        var mr = baseGO.AddComponent<MeshRenderer>();
        mr.sharedMaterial = new Material(baseMaterial);
        SetColor(mr.sharedMaterial, lightRed);

        // 진한 채움
        var fillGO = new GameObject("WarnFill");
        fillGO.transform.SetParent(transform, false);
        fillGO.transform.localPosition = Vector3.up * (yOffset + 0.001f);
        var mf2 = fillGO.AddComponent<MeshFilter>(); mf2.sharedMesh = mesh;
        var mr2 = fillGO.AddComponent<MeshRenderer>();
        mr2.sharedMaterial = new Material(baseMaterial);
        SetColor(mr2.sharedMaterial, darkRed);

        fill = fillGO.transform;
        ApplyFillScale(); // 초기 스케일 반영
    }

    void OnValidate() { if (fill) ApplyFillScale(); }

    void ApplyFillScale()
    {
        // XZ만 키워서 중심에서 바깥으로 퍼지는 느낌
        float k = Mathf.Clamp01(fill01);
        // 완전 0이면 일부 플랫폼에서 0면적 메쉬 문제가 있으니 작은 값 보정
        k = Mathf.Max(0.0001f, k);
        fill.localScale = new Vector3(k, 1f, k);
    }

    Mesh BuildSectorMesh(float R, float angDeg, int seg)
    {
        Mesh m = new Mesh();
        int vCount = seg + 2;

        var v = new Vector3[vCount];
        var n = new Vector3[vCount];
        var uv = new Vector2[vCount];

        v[0] = Vector3.zero; n[0] = Vector3.up; uv[0] = new Vector2(0.5f, 0.5f);

        float half = angDeg * 0.5f * Mathf.Deg2Rad;
        float total = angDeg * Mathf.Deg2Rad;

        for (int i = 0; i <= seg; i++)
        {
            float th = -half + total * (i / (float)seg);
            float x = Mathf.Sin(th) * R;
            float z = Mathf.Cos(th) * R;
            int idx = i + 1;
            v[idx] = new Vector3(x, 0, z);
            n[idx] = Vector3.up;
            uv[idx] = new Vector2((x / R + 1f) * 0.5f, (z / R + 1f) * 0.5f);
        }

        var tri = new int[seg * 3];
        for (int i = 0; i < seg; i++)
        {
            tri[i * 3 + 0] = 0;
            tri[i * 3 + 1] = i + 1;
            tri[i * 3 + 2] = i + 2;
        }

        m.vertices = v; m.normals = n; m.uv = uv; m.triangles = tri;
        m.RecalculateBounds();
        return m;
    }

    static void SetColor(Material m, Color c)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        else if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        else m.color = c;
    }
}
