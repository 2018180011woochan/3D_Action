using UnityEngine;

public class Phase2Projectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public float damage = 20f;
    public float explosionRadius = 2f;
    public float lifeTime = 5f;
    public GameObject explosionEffect; // 폭발 효과 프리팹

    private bool hasExploded = false;

    void Start()
    {
        // 일정 시간 후 자동 파괴
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        // 플레이어나 땅에 닿으면 폭발
        if (other.CompareTag("Player") || other.gameObject.name.Contains("Terrain"))
        {
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;

        // 폭발 효과 생성
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // 폭발 범위 내 플레이어 데미지 처리
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                // 플레이어 데미지 처리
                PlayerState playerState = hit.GetComponent<PlayerState>();
                if (playerState != null)
                {
                    playerState.TakeDamage(damage);
                    Debug.Log($"플레이어가 {damage} 데미지를 받았습니다!");
                }
            }
        }

        // 투사체 파괴
        Destroy(gameObject);
    }
}