using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitBox : MonoBehaviour
{
    private float damage = 5f;
    Animator animator;
    public GameObject AttackEffect;

    bool hasHitThisSwing = false;
    int lastAttackHash = -1;
    void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }

        
     void Update()
    {
        var s = animator.GetCurrentAnimatorStateInfo(0);

        if (s.IsTag("Attack"))
        {
            // Attack1→Attack2 같은 새 스윙 시작 시 리셋
            if (s.shortNameHash != lastAttackHash)
            {
                hasHitThisSwing = false;
                lastAttackHash = s.shortNameHash;
            }
        }
        else
        {
            // 공격 상태 벗어나면 리셋
            hasHitThisSwing = false;
            lastAttackHash = -1;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHitThisSwing) return; // 스윙당 1회

        if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            return;

        var ms = other.GetComponentInParent<MonsterState>();
        if (ms == null || ms.isDead) return;

        hasHitThisSwing = true;      // 이번 스윙에서 더 이상 타격 X
        UIManager.Instance.AddTargetMonster(ms);
        ms.TakeDamage(damage);

        if (AttackEffect)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Instantiate(AttackEffect, hitPoint, Quaternion.identity);
        }
    }
}
