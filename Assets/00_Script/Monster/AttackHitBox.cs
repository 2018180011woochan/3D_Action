using UnityEngine;

public class AttackHitBox : MonoBehaviour
{
    public float damage = 20f;
    Animator animator;
    bool hasAttacked;
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
                ps.TakeDamage(damage);
        }
    }
}
