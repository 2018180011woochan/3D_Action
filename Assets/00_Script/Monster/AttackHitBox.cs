using UnityEngine;

public class AttackHitBox : MonoBehaviour
{
    public float damage = 10f;
    Animator animator;
    public GameObject AttackEffect;

    bool hasSwing = false;

    void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }

    void OnEnable()
    {
        hasSwing = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasSwing) return;

        if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            return;

        var ps = other.GetComponentInParent<PlayerState>();
        if (ps == null) return;

        hasSwing = true; 

        bool isAttackSucces = ps.TakeDamage(damage);

        if (!isAttackSucces)
        {
            animator.SetTrigger("GetHit"); // ÆÐ¸µ
        }

        if (AttackEffect != null)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Instantiate(AttackEffect, hitPoint, Quaternion.identity);
        }
    }
}