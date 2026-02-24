using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Vector3 velocity;
    private float gravity = 9.81f;
    private bool isFlying = true;
    private float damage = 2f;
    public GameObject AttackEffect;
    // 화살 초기화
    public void Initialize(Vector3 direction, float force)
    {
        velocity = direction * force;
        isFlying = true;

        transform.rotation = Quaternion.LookRotation(velocity) * Quaternion.Euler(90, 0, 0);
    }

    void Update()
    {
        if (!isFlying) return;

        velocity.y -= gravity * Time.deltaTime;

        transform.position += velocity * Time.deltaTime;

        // 화살이 항상 날아가는 방향을 바라보도록
        if (velocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(velocity) * Quaternion.Euler(90, 0, 0);
        }

        // 땅에 닿았는지 체크 (임시)
        if (transform.position.y < 0)
        {
            isFlying = false;
        }
    }

    // 일정 시간 후 풀로 반환
    void OnEnable()
    {
        Invoke("ReturnToPool", 5f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            Debug.Log("화살히트");
            var ms = other.GetComponent<MonsterState>();
            if (ms.isDead) return;
            if (ms != null)
            {
                UIManager.Instance.AddTargetMonster(ms);
                //ms.TakeDamage(damage);

                if (AttackEffect != null)
                {
                    Vector3 hitPoint = other.ClosestPoint(transform.position);
                    GameObject effect = Instantiate(AttackEffect, hitPoint, Quaternion.identity);

                }
            }
        }
    }

    void ReturnToPool()
    {
        PoolManager.Instance.ReturnArrow(gameObject);
    }
}