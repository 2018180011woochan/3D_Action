using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Vector3 velocity;
    private float gravity = 9.81f;
    private bool isFlying = true;

    // 화살 초기화
    public void Initialize(Vector3 direction, float force)
    {
        velocity = direction * force;
        isFlying = true;

        // 화살이 속도 방향을 바라보도록
        transform.rotation = Quaternion.LookRotation(velocity) * Quaternion.Euler(90, 0, 0);
    }

    void Update()
    {
        if (!isFlying) return;

        // 중력 적용
        velocity.y -= gravity * Time.deltaTime;

        // 위치 업데이트
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

    void ReturnToPool()
    {
        PoolManager.Instance.ReturnArrow(gameObject);
    }
}