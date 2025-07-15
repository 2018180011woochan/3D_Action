using UnityEngine;

public class KickAttackHitBox : MonoBehaviour
{
    public float damage = 10f;
    Animator animator;
    public GameObject AttackEffect;

    void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            return;

        if (other.CompareTag("Player"))
        {
            var ps = other.GetComponent<PlayerState>();
            if (ps != null)
            {
                Vector3 dir = dir = transform.root.forward;
                ps.TakeCriticalDamage(damage, dir);

                if (AttackEffect != null)
                {
                    // 충돌 지점에 이펙트 생성
                    Vector3 hitPoint = other.ClosestPoint(transform.position);
                    GameObject effect = Instantiate(AttackEffect, hitPoint, Quaternion.identity);

                }
            }
        }
    }
}
