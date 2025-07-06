using UnityEngine;

public class AttackHitBox : MonoBehaviour
{
    public float damage = 20f;
    Animator animator;

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
            other.GetComponent<PlayerState>()?.TakeDamage(damage);
            Debug.Log("플레이어 공격 성공");
        }
    }
}
