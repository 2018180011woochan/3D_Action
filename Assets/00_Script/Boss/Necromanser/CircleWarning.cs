using System.Collections;
using UnityEngine;

public class CircleWarning : MonoBehaviour
{
    [Header("Shape")]
    public float radius = 10f;                  // 원 반경
    [Range(16, 256)] public int segments = 96;  // 가장자리 분할

    [Header("Visuals")]
    public Material baseMaterial;
    public Color lightRed = new Color(1f, 0f, 0f, 0.25f);
    public Color darkRed = new Color(1f, 0f, 0f, 0.80f);
    public float fillDuration = 2f;             // 채우는 시간(초)
    public float yOffset = 0.02f;               // z-fight 방지

    [Header("Attack")]
    public GameObject attackPrefab;             // 공격 이펙트 프리팹
    public float attackYawOffsetDeg = 0f;       // 프리팹 로컬 Y 회전 보정(필요 시)
    public Vector3 attackOffset;                // 프리팹 로컬 오프셋

    [Header("Ground")]
    public bool alignToGround = true;
    public LayerMask groundMask = ~0;

    Transform fill; // 진한 채움계층

    void Start()
    {
        if (alignToGround) AlignToGround();
        Build();
        StartCoroutine(FillThenAttack());
    }

    void Build()
    {
        var mesh = BuildDiscMesh(radius, segments);

        // 1) 밝은 바닥
        var baseGO = new GameObject("WarnBase");
        baseGO.transform.SetParent(transform, false);
        baseGO.transform.localPosition = Vector3.up * yOffset;
        var mf1 = baseGO.AddComponent<MeshFilter>(); mf1.sharedMesh = mesh;
        var mr1 = baseGO.AddComponent<MeshRenderer>();
        mr1.sharedMaterial = new Material(baseMaterial);
        SetColor(mr1.sharedMaterial, lightRed);

        // 2) 진한 채움(스케일 0→1로 퍼지게)
        var fillGO = new GameObject("WarnFill");
        fillGO.transform.SetParent(transform, false);
        fillGO.transform.localPosition = Vector3.up * (yOffset + 0.001f);
        var mf2 = fillGO.AddComponent<MeshFilter>(); mf2.sharedMesh = mesh;
        var mr2 = fillGO.AddComponent<MeshRenderer>();
        mr2.sharedMaterial = new Material(baseMaterial);
        SetColor(mr2.sharedMaterial, darkRed);

        fill = fillGO.transform;
        fill.localScale = new Vector3(0f, 1f, 0f);   // 반지름 0부터 시작
    }

    IEnumerator FillThenAttack()
    {
        float dur = Mathf.Max(0f, fillDuration);
        if (dur <= 0f) fill.localScale = Vector3.one;
        else
        {
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                fill.localScale = new Vector3(k, 1f, k); // 0→1
                yield return null;
            }
        }

        // 공격 이펙트 생성 (원형이라 방향 자유: 필요시 보정 각도만 적용)
        if (attackPrefab)
        {
            var finalRot = transform.rotation * Quaternion.Euler(0f, attackYawOffsetDeg, 0f);
            var pos = transform.position + finalRot * attackOffset;
            Instantiate(attackPrefab, pos, finalRot);
        }

        Destroy(gameObject); // 경고 제거
    }

    // 원형(디스크) 메쉬: 중심 + 둘레(seg+1개, 마지막은 시작점과 동일)
    Mesh BuildDiscMesh(float R, int seg)
    {
        seg = Mathf.Max(3, seg);

        Mesh m = new Mesh();
        int vCount = seg + 2; // center + (0..seg) 둘레 + 클로저 1
        var v = new Vector3[vCount];
        var n = new Vector3[vCount];
        var uv = new Vector2[vCount];

        v[0] = Vector3.zero; n[0] = Vector3.up; uv[0] = new Vector2(0.5f, 0.5f);

        // 0..seg(포함)까지 돌면서 2π를 커버(마지막 점은 시작점과 동일 각도)
        for (int i = 0; i <= seg; i++)
        {
            float th = Mathf.PI * 2f * i / seg;    // 0 ~ 2π
            float x = Mathf.Sin(th) * R;
            float z = Mathf.Cos(th) * R;
            int idx = i + 1;

            v[idx] = new Vector3(x, 0f, z);
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

    void AlignToGround()
    {
        var start = transform.position + Vector3.up * 10f;
        var hits = Physics.RaycastAll(start, Vector3.down, 100f, groundMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.CompareTag("Player")) continue; 
            transform.position = new Vector3(transform.position.x, hits[i].point.y + 0.01f, transform.position.z);
            transform.rotation = Quaternion.FromToRotation(Vector3.up, hits[i].normal) * transform.rotation;
            return;
        }
    }

    static void SetColor(Material m, Color c)
    {
        if (!m) return;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        else if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        else m.color = c;
    }
}

