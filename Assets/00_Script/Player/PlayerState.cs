using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerState : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP { get; private set; }

    public UnityEvent<float> onHealthChanged = new UnityEvent<float>();
    public UnityEvent<float> onStaminaChanged = new UnityEvent<float>();

    public float maxStamina = 100f;
    public float currentStamina { get; private set; }

    Animator animator;
    public BloodScreen bloodScreen;

    public GameObject BlockEffect;
    private ParticleSystem blockParticle;

    private Rigidbody rb;
    private PlayerController playerController;

    public float knockbackForce = 10f;

    private Coroutine healCoroutine;
    public GameObject HealEffectPrefab;

    private GameObject healEffectInstance;

    private SamuraiMovement movement;
    public GameObject StunEffect;

    [Header("»ç¿îµå")]
    public AudioClip blockSfx;
    public AudioClip hitSfx;
    public float Volume = 1f;
    private AudioSource Src;

    void Awake()
    {
        currentHP = maxHP;
        currentStamina = maxStamina;
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
        if (BlockEffect != null)
            blockParticle = BlockEffect.GetComponent<ParticleSystem>();
        movement = GetComponent<SamuraiMovement>();

        Src = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        Src.playOnAwake = false;
        Src.loop = false;
        Src.spatialBlend = 0f;
    }

    void Start()
    {
        onHealthChanged.Invoke(currentHP / maxHP);
        onStaminaChanged.Invoke(currentStamina / maxStamina);
    }

    public void ApplyRemoteDamage(float dmg, float serverHp, bool isBlocked)
    {
        if (isBlocked)
        {
            Src.PlayOneShot(blockSfx, Volume);
            return;
        }

        Src.PlayOneShot(hitSfx, 1f);
        if (StunEffect != null) StunEffect.SetActive(true);

        currentHP = serverHp;
        onHealthChanged.Invoke(currentHP / maxHP);

        animator.SetTrigger("GetHit");
        bloodScreen?.Flash();

        if (movement) movement.Stance = false;
    }

    public void ApplyRemoteUseItem(int itemId, float serverHp)
    {
        currentHP = serverHp;
        onHealthChanged.Invoke(currentHP / maxHP);

        if (itemId == 1)
        {
            if (HealEffectPrefab != null)
            {
                GameObject effect = Instantiate(HealEffectPrefab, transform.position + Vector3.up * 1f, Quaternion.identity, transform);
                Destroy(effect, 2f);
            }
        }
    }

    public bool IsBlocking()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("Block");
    }

    public void TakeCriticalDamage(float dmg)
    {
        StunEffect.SetActive(true);
        Src.PlayOneShot(hitSfx, 1f);
        currentHP = Mathf.Max(currentHP - dmg, 0f);
        onHealthChanged.Invoke(currentHP / maxHP);

        animator.SetTrigger("GetHit");
        bloodScreen?.Flash();

        if (movement) movement.Stance = false;
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
            healCoroutine = null;
        }
        if (healEffectInstance != null)
        {
            Destroy(healEffectInstance);
            healEffectInstance = null;
        }

        healCoroutine = StartCoroutine(HealCoroutine(healPerSecond, duration));
    }

    IEnumerator HealCoroutine(float healPerSecond, float duration)
    {
        float elapsedTime = 0f;

        if (HealEffectPrefab != null)
        {
            healEffectInstance = Instantiate(HealEffectPrefab, transform.position, Quaternion.identity);
            healEffectInstance.transform.SetParent(transform);
            healEffectInstance.transform.localPosition = Vector3.zero;
        }

        while (elapsedTime < duration)
        {
            if (currentHP >= maxHP) break;

            Heal(healPerSecond);

            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
        }

        if (healEffectInstance != null)
        {
            Destroy(healEffectInstance);
            healEffectInstance = null;
        }

        healCoroutine = null;
    }

    public void SetHP(float hp)
    {
        currentHP = hp;
        onHealthChanged.Invoke(currentHP / maxHP);
    }
}