using UnityEngine;
using UnityEngine.Events;

public class MonsterState : MonoBehaviour
{
    [Header("체력 설정")]
    public float maxHP = 100f;
    public string monsterName;
    public float currentHP { get; private set; }
    public bool isDead = false;

    public UnityEvent<float> onHealthChanged = new UnityEvent<float>();
    Animator animator;

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
}
