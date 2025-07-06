using UnityEngine;
using UnityEngine.Events;

public class PlayerState : MonoBehaviour
{
    [Header("체력 설정")]
    public float maxHP = 100f;
    public float currentHP { get; private set; }

    public UnityEvent<float> onHealthChanged = new UnityEvent<float>();

    [Header("스태미너 설정")]
    public float maxStamina = 100f;
    public float currentStamina { get; private set; }

    public UnityEvent<float> onStaminaChanged = new UnityEvent<float>();

    void Awake()
    {
        currentHP = maxHP;
        currentStamina = maxStamina;
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

    public void ConsumeStamina(float amount)
    {
        currentStamina = Mathf.Max(currentStamina - amount, 0f);
        onStaminaChanged.Invoke(currentStamina / maxStamina);
    }

    public void RecoverStamina(float amount)
    {
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        onStaminaChanged.Invoke(currentStamina / maxStamina);
    }
}
