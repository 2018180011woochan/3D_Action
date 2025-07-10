using UnityEngine;

public class PlayerAttackHitBox : MonoBehaviour
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

        if (other.CompareTag("Monster"))
        {
            var ms = other.GetComponent<MonsterState>();
            if (ms != null)
            {
                Debug.Log("�浹 ����");
                ms.TakeDamage(damage);

                if (AttackEffect != null)
                {
                    // �浹 ������ ����Ʈ ����
                    Vector3 hitPoint = other.ClosestPoint(transform.position);
                    GameObject effect = Instantiate(AttackEffect, hitPoint, Quaternion.identity);

                }
            }
        }
    }
}
