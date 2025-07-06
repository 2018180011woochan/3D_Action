using UnityEngine;
using UnityEngine.Events;

public class PlayerState : MonoBehaviour
{
    [Header("체력 설정")]
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
        onHealthChanged.Invoke(currentHP / maxHP);
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        onHealthChanged.Invoke(currentHP / maxHP);
    }
}
