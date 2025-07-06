using UnityEngine;
using UnityEngine.Events;

public class PlayerState : MonoBehaviour
{
    [Header("ü�� ����")]
    public float maxHP = 100f;
    public float currentHP { get; private set; }

    public UnityEvent<float> onHealthChanged = new UnityEvent<float>();

    void Awake()
    {
        currentHP = maxHP;

    }

    public void TakeDamage(float dmg)
    {
        currentHP = Mathf.Max(currentHP - dmg, 0f);
        Debug.Log($"[PlayerState] TakeDamage �� currentHP = {currentHP}");
        onHealthChanged.Invoke(currentHP / maxHP);
        if (currentHP <= 0f) Die();
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        onHealthChanged.Invoke(currentHP / maxHP);
    }

    void Die()
    {
        // ���� ó�� (�ִ�, ������ ��)
    }
}
