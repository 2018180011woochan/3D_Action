using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HP Bar")]
    [SerializeField] private Image hpFillImage;     
    [SerializeField] private Image hpDamageImage;
    public float damageDelay = 0.1f;
    public float damageDuration = 0.3f;
    private Coroutine hpCoroutine;

    [Header("Stamina Bar")]
    [SerializeField] private Image stamFillImage;
    [SerializeField] private Image stamDamageImage;
    private Coroutine stamCoroutine;

    [Header("FireSkill")]
    [SerializeField] private Image FireSkillTimer;
    private Coroutine cooldownCoroutine;


    [SerializeField] private PlayerState playerState;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        if (playerState != null)
        {
            playerState.onHealthChanged.AddListener(OnHealthChanged);
            playerState.onStaminaChanged.AddListener(OnStaminaChanged);
        }
    }

    void OnDisable()
    {
        if (playerState != null)
        {
            playerState.onHealthChanged.RemoveListener(OnHealthChanged);
            playerState.onStaminaChanged.RemoveListener(OnStaminaChanged);
        }
    }

    void Start()
    {
        float hpNorm = playerState.currentHP / playerState.maxHP;
        hpFillImage.fillAmount = hpNorm;
        hpDamageImage.fillAmount = hpNorm;

        float stNorm = playerState.currentStamina / playerState.maxStamina;
        stamFillImage.fillAmount = stNorm;
        stamDamageImage.fillAmount = stNorm;

        FireSkillTimer.fillAmount = 0f;
        FireSkillTimer.gameObject.SetActive(false);
    }

    public void StartFireSkillCooldown(float cooldownTime)
    {
        if (cooldownCoroutine != null) StopCoroutine(cooldownCoroutine);
        cooldownCoroutine = StartCoroutine(FireSkillCooldownRoutine(cooldownTime));
    }

    IEnumerator FireSkillCooldownRoutine(float cooldownTime)
    {
        FireSkillTimer.gameObject.SetActive(true);
        FireSkillTimer.fillAmount = 1f;

        float elapsed = 0f;

        while (elapsed < cooldownTime)
        {
            elapsed += Time.deltaTime;
            FireSkillTimer.fillAmount = 1f - (elapsed / cooldownTime);
            yield return null;
        }

        FireSkillTimer.fillAmount = 0f;
        FireSkillTimer.gameObject.SetActive(false);
    }

    void OnHealthChanged(float normalizedHP)
    {
        // 앞바 즉시
        hpFillImage.fillAmount = normalizedHP;

        // 뒷바 애니메이션
        if (hpCoroutine != null) StopCoroutine(hpCoroutine);
        hpCoroutine = StartCoroutine(
            AnimateBar(hpDamageImage, normalizedHP));
    }

    void OnStaminaChanged(float normalizedStam)
    {
        stamFillImage.fillAmount = normalizedStam;

        if (stamCoroutine != null) StopCoroutine(stamCoroutine);
        stamCoroutine = StartCoroutine(
            AnimateBar(stamDamageImage, normalizedStam));
    }

    IEnumerator AnimateBar(Image barImage, float targetFill)
    {
        yield return new WaitForSeconds(damageDelay);

        float startFill = barImage.fillAmount;
        float elapsed = 0f;

        while (elapsed < damageDuration)
        {
            elapsed += Time.deltaTime;
            barImage.fillAmount =
                Mathf.Lerp(startFill, targetFill, elapsed / damageDuration);
            yield return null;
        }

        barImage.fillAmount = targetFill;
    }
}
