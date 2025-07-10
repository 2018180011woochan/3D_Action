using System.Collections;
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
    Animator animator;
    public BloodScreen bloodScreen;

    public GameObject BlockEffect;
    private ParticleSystem blockParticle;
    void Awake()
    {
        currentHP = maxHP;
        currentStamina = maxStamina;
        animator = GetComponentInChildren<Animator>();

        if (BlockEffect != null)
            blockParticle = BlockEffect.GetComponent<ParticleSystem>();
    }

    public void TakeDamage(float dmg)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Block"))
        {
            BlockEffect.SetActive(true);
            blockParticle?.Play();

            StartCoroutine(DisableBlockEffect());
            return;
        }
        currentHP = Mathf.Max(currentHP - dmg, 0f);
        onHealthChanged.Invoke(currentHP / maxHP);

        animator.SetTrigger("GetHit");
        bloodScreen?.Flash();
    }
    IEnumerator DisableBlockEffect()
    {
        yield return new WaitForSeconds(1f); 
        BlockEffect.SetActive(false);
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
