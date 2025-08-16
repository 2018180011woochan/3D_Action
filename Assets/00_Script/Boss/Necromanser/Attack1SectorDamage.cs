using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Attack1SectorDamage : MonoBehaviour
{
    [Header("섹터 모양")]
    public float radius = 10f;                    // 부채꼴 반경
    [Range(1f, 179f)] public float angleDeg = 60f; // 부채꼴 각도(전체 각)
    public float height = 3f;                     // 수직 허용 높이

    [Header("방향 보정")]
    public Transform directionRef;                // 기준 방향(없으면 origin)
    [Range(-360f, 360f)] public float yawOffsetDeg = 0f;  // +반시계, -시계

    [Header("데미지")]
    public int criticalDamage = 20;

    [Header("기타")]
    public Transform origin;                      // 기준 위치(없으면 자신)
    public float startDelay = 0f;                 // 연출 후 딜레이

    // (선택) 레이어로 1차 필터; 설정 안해도 동작(Everything)
    public LayerMask layerMask = Physics.AllLayers;

    readonly Collider[] _buffer = new Collider[64];
    readonly HashSet<Transform> _hitRoots = new HashSet<Transform>();

    void OnEnable()
    {
        if (!origin) origin = transform;
        _hitRoots.Clear();

        if (startDelay <= 0f) DoHit();
        else StartCoroutine(HitAfterDelay());
    }

    IEnumerator HitAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        DoHit();
    }

    void DoHit()
    {
        Transform refT = directionRef ? directionRef : origin;

        // 섹터 중심 방향(야우 오프셋 반영)
        Vector3 f = Quaternion.Euler(0f, yawOffsetDeg, 0f) * refT.forward;
        f.y = 0f; f.Normalize();

        Vector3 o = origin.position;

        int count = Physics.OverlapSphereNonAlloc(
            o, radius, _buffer, layerMask, QueryTriggerInteraction.Ignore);

        float cosHalf = Mathf.Cos(angleDeg * 0.5f * Mathf.Deg2Rad);

        for (int i = 0; i < count; i++)
        {
            var col = _buffer[i];
            if (!col || !col.CompareTag("Player")) continue; 

            // 수직/수평 판정
            Vector3 c = col.bounds.center;
            if (Mathf.Abs(c.y - o.y) > height * 0.5f) continue;

            Vector3 to = c - o; to.y = 0f;
            float dist = to.magnitude;
            if (dist <= 0.0001f || dist > radius) continue;

            to.Normalize();
            if (Vector3.Dot(f, to) < cosHalf) continue;     

            // 같은 대상의 여러 콜라이더 중복 방지
            var root = col.attachedRigidbody ? col.attachedRigidbody.transform : col.transform;
            if (_hitRoots.Contains(root)) continue;
            _hitRoots.Add(root);

            var ps = col.GetComponentInParent<PlayerState>();
            if (ps) ps.TakeCriticalDamage(criticalDamage);  
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Transform o = origin ? origin : transform;
        Transform refT = directionRef ? directionRef : o;

        Vector3 pos = o.position;
        Vector3 fwd = Quaternion.Euler(0f, yawOffsetDeg, 0f) * refT.forward;
        fwd.y = 0f; fwd.Normalize();

        UnityEditor.Handles.color = new Color(0f, 1f, 1f, 0.25f);
        UnityEditor.Handles.DrawSolidArc(pos, Vector3.up,
            Quaternion.Euler(0, -angleDeg * 0.5f, 0) * fwd, angleDeg, radius);

        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawLine(pos + Vector3.up * (height * 0.5f),
                        pos - Vector3.up * (height * 0.5f));
    }
#endif
}
