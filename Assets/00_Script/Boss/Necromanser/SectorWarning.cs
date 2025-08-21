using System.Collections;
using UnityEngine;

public class SectorWarning : MonoBehaviour
{
    [Header("Shape")]
    public float radius = 10f;
    public float angleDeg = 60f;
    [Range(8, 128)] public int segments = 48;

    [Header("Visuals")]
    public Material baseMaterial;
    public Color lightRed = new Color(1f, 0f, 0f, 0.25f);
    public Color darkRed = new Color(1f, 0f, 0f, 0.80f);
    public float fillDuration = 2f;
    public float yOffset = 0.02f;

    [Header("Align & Attack")]
    public float warningYawOffsetDeg = 0f;
    public GameObject attackPrefab;
    public float attackYawOffsetDeg = 270f;
    public float attackForwardOffset = 0f;
    public Vector3 attackOffset;

    [Header("Ground")]
    public bool alignToGround = true;
    public LayerMask groundMask = ~0;

    [Header("Sound")]
    public AudioClip attackSfx;

    Transform warnRoot;
    Transform fill;
    public GameObject Boss;

    AudioSource sfx;

    void Start()
    {
        if (alignToGround) AlignToGround();
        Build();
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.loop = false;
        sfx.spatialBlend = 0f;
        StartCoroutine(FillThenAttack());
        Boss = GameObject.FindWithTag("Boss");
    }

    void Update()
    {
        if (Boss.GetComponent<NecromanserAI>().isDead) Destroy(gameObject);
    }

    void Build()
    {
        var mesh = BuildSectorMesh(radius, angleDeg, segments);

        var root = new GameObject("WarnRoot");
        root.transform.SetParent(transform, false);
        root.transform.localRotation = Quaternion.Euler(0f, warningYawOffsetDeg, 0f);
        warnRoot = root.transform;

        var baseGO = new GameObject("WarnBase");
        baseGO.transform.SetParent(warnRoot, false);
        baseGO.transform.localPosition = Vector3.up * yOffset;
        var mf = baseGO.AddComponent<MeshFilter>(); mf.sharedMesh = mesh;
        var mr = baseGO.AddComponent<MeshRenderer>();
        mr.sharedMaterial = new Material(baseMaterial);
        SetColor(mr.sharedMaterial, lightRed);

        var fillGO = new GameObject("WarnFill");
        fillGO.transform.SetParent(warnRoot, false);
        fillGO.transform.localPosition = Vector3.up * (yOffset + 0.001f);
        var mf2 = fillGO.AddComponent<MeshFilter>(); mf2.sharedMesh = mesh;
        var mr2 = fillGO.AddComponent<MeshRenderer>();
        mr2.sharedMaterial = new Material(baseMaterial);
        SetColor(mr2.sharedMaterial, darkRed);

        fill = fillGO.transform;
        fill.localScale = new Vector3(0f, 1f, 0f);
    }

    IEnumerator FillThenAttack()
    {
        float t = 0f;
        float dur = Mathf.Max(0f, fillDuration);

        if (dur <= 0f) fill.localScale = new Vector3(1f, 1f, 1f);
        else
        {
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                fill.localScale = new Vector3(k, 1f, k);
                yield return null;
            }
        }

        if (attackPrefab)
        {
            var warnRot = transform.rotation * Quaternion.Euler(0f, warningYawOffsetDeg, 0f);
            var finalRot = warnRot * Quaternion.Euler(0f, attackYawOffsetDeg, 0f);
            Vector3 pos = transform.position + warnRot * (Vector3.forward * attackForwardOffset) + finalRot * attackOffset;
            Instantiate(attackPrefab, pos, finalRot);
        }

        if (attackSfx) AudioSource.PlayClipAtPoint(attackSfx, transform.position, 1f);
        Destroy(gameObject);
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
            float th = -half + total * i / seg;
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
        if (hits == null || hits.Length == 0) return;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i].collider;
            if (col.isTrigger) continue;
            if (col.transform.root.CompareTag("Player")) continue;
            if (col.transform.root.CompareTag("Monster")) continue;

            transform.position = new Vector3(transform.position.x, hits[i].point.y + 0.01f, transform.position.z);
            transform.rotation = Quaternion.FromToRotation(Vector3.up, hits[i].normal) * transform.rotation;
            return;
        }
    }

    static void SetColor(Material m, Color c)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        else if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        else m.color = c;
    }
}
