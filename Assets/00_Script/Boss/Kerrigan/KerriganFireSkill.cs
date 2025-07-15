using System.Collections;
using UnityEngine;

public class KerriganFireSkill : MonoBehaviour
{
    [Header("데미지")]
    public float Damage = 10f;

    [Header("콜라이더 설정")]
    public Vector3 colliderSize;
    public Vector3 colliderOffset;
    private BoxCollider damageCollider;

    [Header("지연 시간")]
    public float damageDelay = 0.5f;

    void Start()
    {
        damageCollider = gameObject.AddComponent<BoxCollider>();
        if (damageCollider == null)
            Debug.Log("콜라이더 못찾ㅂ음");
        damageCollider.isTrigger = true;
        damageCollider.size = colliderSize;
        damageCollider.center = colliderOffset;
        damageCollider.enabled = false;

        StartCoroutine(EnableDamageAfterDelay());
    }

    IEnumerator EnableDamageAfterDelay()
    {
        yield return new WaitForSeconds(damageDelay);

        if (damageCollider != null)
        {
            damageCollider.enabled = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerState playerState = other.GetComponent<PlayerState>();
            if (playerState != null)
            {
                Vector3 dir = dir = transform.root.forward;
                playerState.TakeCriticalDamage(Damage, dir);
                Debug.Log("플레이어가 불꽃 영역에 진입!");
            }
        }
    }

}
