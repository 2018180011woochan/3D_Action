using System.Collections;
using UnityEngine;

public class GhostProjectile : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 20f;

    [Header("Timing")]
    public float activationDelay = 1f;   // 소환 후 대기 시간
    public float activeDuration = 0.6f;  // (옵션) 데미지 창 길이
    public float lifeTime = 5f;          // 자동 파괴 시간(안전용)

    private BoxCollider col;
    private bool canDamage = false;
    private bool hasHit = false;

    void Awake()
    {
        col = GetComponent<BoxCollider>();
        if (col == null) Debug.LogWarning("BoxCollider가 필요합니다.");
    }

    void OnEnable()
    {
        // 안전 파괴 예약
        if (lifeTime > 0f) Destroy(gameObject, lifeTime);
        StartCoroutine(ActivateAfterDelay());
    }

    IEnumerator ActivateAfterDelay()
    {
        if (col) col.enabled = false;     // 1초 동안 무적
        yield return new WaitForSeconds(activationDelay);

        if (col) col.enabled = true;
        canDamage = true;
    }

    void TryHit(GameObject other)
    {
        if (!canDamage || hasHit) return;
        if (!other.CompareTag("Player")) return;

        var ps = other.GetComponent<PlayerState>();
        if (ps) ps.TakeDamage(damage);
        hasHit = true;
    }

    void OnTriggerEnter(Collider other) => TryHit(other.gameObject);

    // 활성화 시점에 이미 겹쳐있는 경우 대비
    void OnTriggerStay(Collider other) => TryHit(other.gameObject);
}
