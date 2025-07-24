using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerState : MonoBehaviour
{ 
    public float maxHP = 100f;
    public float currentHP { get; private set; }

    public UnityEvent<float> onHealthChanged = new UnityEvent<float>();

    public float maxStamina = 100f;
    public float currentStamina { get; private set; }

    public UnityEvent<float> onStaminaChanged = new UnityEvent<float>();
    Animator animator;
    public BloodScreen bloodScreen;

    public GameObject BlockEffect;
    private ParticleSystem blockParticle;

    private Rigidbody rb;
    private PlayerController playerController;

    public float knockbackForce = 10f;

    private Coroutine healCoroutine;
    public GameObject HealEffect;

    void Awake()
    {
        currentHP = maxHP;
        currentStamina = maxStamina;
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
        if (BlockEffect != null)
            blockParticle = BlockEffect.GetComponent<ParticleSystem>();
    }


    public bool TakeDamage(float dmg)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Block"))
        {
            BlockEffect.SetActive(true);
            blockParticle?.Play();

            StartCoroutine(DisableBlockEffect());
            return false;
        }
        currentHP = Mathf.Max(currentHP - dmg, 0f);
        onHealthChanged.Invoke(currentHP / maxHP);

        animator.SetTrigger("GetHit");
        bloodScreen?.Flash();
        return true;
    }

    public bool TakeCriticalDamage(float dmg, Vector3 dir)
    {
        currentHP = Mathf.Max(currentHP - dmg, 0f);
        onHealthChanged.Invoke(currentHP / maxHP);

        animator.SetTrigger("GetCritical");
        bloodScreen?.Flash();

        if (playerController != null)
        {
            playerController.ApplyKnockback(dir.normalized, knockbackForce);
        }
        return true;
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

    public void StartHealOverTime(float healPerSecond, float duration)
    {
        if (healCoroutine != null)
        {
            StopCoroutine(healCoroutine);
            Destroy(HealEffect); 
        }

        healCoroutine = StartCoroutine(HealCoroutine(healPerSecond, duration, HealEffect));
    }

    IEnumerator HealCoroutine(float healPerSecond, float duration, GameObject healEffectPrefab)
    {
        float elapsedTime = 0f;

        HealEffect = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
        HealEffect.transform.SetParent(transform);
        HealEffect.transform.localPosition = new Vector3(0, 0, 0);

        while (elapsedTime < duration)
        {
            if (currentHP >= maxHP) break;
            
            Heal(healPerSecond);

            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
        }

        if (HealEffect != null)
        {
            Destroy(HealEffect);
        }

        healCoroutine = null;
    }
}
