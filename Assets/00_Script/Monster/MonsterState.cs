using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MonsterState : MonoBehaviour
{
    [Header("ü�� ����")]
    public float maxHP = 100f;
    public string monsterName;
    public float currentHP { get; private set; }
    public bool isDead = false;

    public UnityEvent<float> onHealthChanged = new UnityEvent<float>();
    Animator animator;

    [Header("�ߵ��� ������ �޾��� ��")]
    public GameObject BattoHitEffect;
    private Coroutine battoCoroutine;
    void Awake()
    {
        currentHP = maxHP;
        animator = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(float dmg)
    {
        currentHP = Mathf.Max(currentHP - dmg, 0f);
        onHealthChanged.Invoke(currentHP / maxHP);

        animator.SetTrigger("GetHit");

        if (currentHP <= 0f)
        {
            isDead = true;
            var monAI = GetComponent<MonsterAI>();
            monAI.currentState = MonsterAI.State.Dead;
            animator.SetTrigger("Dead");
            UIManager.Instance.RemoveTargetMonster(this);
        }
    }

    public void TakeBatto(float dmg)
    {
        if (battoCoroutine != null)
            return;
        UIManager.Instance.AddTargetMonster(this);
        battoCoroutine = StartCoroutine(BattoDamageOverTime(dmg));
    }

    IEnumerator BattoDamageOverTime(float dmgPerTick)
    {
        // �ߵ��� ��Ʈ ����Ʈ ����
        GameObject hitEffect = Instantiate(
            BattoHitEffect,
            transform.position,
            Quaternion.identity
        );

        hitEffect.transform.SetParent(transform);

        // 5�� ���� 1�ʸ��� ������
        for (int i = 0; i < 5; i++)
        {
            // ������ ����
            currentHP = Mathf.Max(currentHP - dmgPerTick, 0f);
            onHealthChanged.Invoke(currentHP / maxHP);

            animator.SetTrigger("GetHit");

            // ��� üũ
            if (currentHP <= 0f && !isDead)
            {
                isDead = true;
                var monAI = GetComponent<MonsterAI>();
                if (monAI != null)
                    monAI.currentState = MonsterAI.State.Dead;

                animator.SetTrigger("Dead");
                UIManager.Instance.RemoveTargetMonster(this);

                // ����Ʈ ��� ����
                if (hitEffect != null)
                    Destroy(hitEffect);

                battoCoroutine = null;
                yield break;
            }

            // ������ ƽ�� �ƴϸ� 1�� ���
            if (i < 4)
                yield return new WaitForSeconds(1f);
        }

        // 3�� �� ����Ʈ ����
        if (hitEffect != null)
            Destroy(hitEffect);

        battoCoroutine = null;
    }
}
