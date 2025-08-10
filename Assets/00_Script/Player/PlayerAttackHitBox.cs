using UnityEngine;

public class PlayerAttackHitBox : MonoBehaviour
{
    [SerializeField] float damage = 5f;
    [SerializeField, Tooltip("클립 정규화 시간 기준 2타가 시작되는 지점(0~1)")]
    float secondSwingAt = 0.5f;   // 2타 시작 시점(대략 절반이면 0.5)

    Animator animator;
    public GameObject AttackEffect;

    bool hasHitThisSwing = false;
    int lastAttackHash = -1;
    int lastSwingPhase = -1; // 0 = 1타 구간, 1 = 2타 구간

    void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }

    void Update()
    {
        var s = animator.GetCurrentAnimatorStateInfo(0);

        if (s.IsTag("Attack"))
        {
            float t = s.normalizedTime % 1f;
            int phase = (t < secondSwingAt) ? 0 : 1;

            // 스테이트가 바뀌었거나(Attack1→Attack2) 또는 같은 스테이트 내에서 1타→2타로 넘어가면 리셋
            if (s.shortNameHash != lastAttackHash || phase != lastSwingPhase)
            {
                hasHitThisSwing = false;
                lastAttackHash = s.shortNameHash;
                lastSwingPhase = phase;
            }
        }
        else
        {
            hasHitThisSwing = false;
            lastAttackHash = -1;
            lastSwingPhase = -1;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHitThisSwing) return;
        if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) return;

        var ms = other.GetComponentInParent<MonsterState>();
        if (ms == null || ms.isDead) return;

        hasHitThisSwing = true; // 현재 스윙(1타 또는 2타)에서 1회만

        // 기존 기능 유지
        UIManager.Instance.AddTargetMonster(ms);

        ms.TakeDamage(damage);

        if (AttackEffect)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Instantiate(AttackEffect, hitPoint, Quaternion.identity);
        }
    }
}
