using UnityEngine;

public class PlayerAttackHitBox : MonoBehaviour
{
    [SerializeField] float damage = 10f;
    float secondSwingAt = 0.5f;   

    Animator animator;
    public GameObject AttackEffect;

    bool hasHitThisSwing = false;
    int lastAttackHash = -1;
    int lastSwingPhase = -1; 

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

        hasHitThisSwing = true; 

        UIManager.Instance.AddTargetMonster(ms);

        ms.TakeDamage(damage);

        if (AttackEffect)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Instantiate(AttackEffect, hitPoint, Quaternion.identity);
        }
    }
}
