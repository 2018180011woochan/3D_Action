using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSkillProjectile : MonoBehaviour
{
    [Header("데미지")]
    public float Damage = 10f;

    [Header("콜라이더 설정")]
    private BoxCollider damageCollider;

    [Header("지연 시간")]
    public float damageDelay = 0.5f;

    void Start()
    {
        damageCollider = gameObject.AddComponent<BoxCollider>();
        if (damageCollider == null)
            Debug.Log("콜라이더 못찾음");

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
        if (other.CompareTag("Monster"))
        {
            MonsterState monsterState = other.GetComponent<MonsterState>();
            if (monsterState != null)
            {
                UIManager.Instance.AddTargetMonster(monsterState);
                //monsterState.TakeDamage(Damage);
                Debug.Log("몬스터 불꽃 영역에 진입!");
            }
        }
    }
}
