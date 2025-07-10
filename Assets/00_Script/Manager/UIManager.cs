using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    private Coroutine fireCooldownCoroutine;

    [Header("BattoSkill")]
    [SerializeField] private Image battoSkillTimer;
    private Coroutine battoCooldownCoroutine;

    [Header("Monster HP Bar")]
    [SerializeField] public MonsterHPBar monsterHPBarPrefab;
    [SerializeField] public Transform monsterHPParent;
    private List<MonsterHPBar> activeBars = new List<MonsterHPBar>();
    private int maxBars = 2;

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
        battoSkillTimer.fillAmount = 0f;
        battoSkillTimer.gameObject.SetActive(false);
    }

    public void StartFireSkillCooldown(float cooldownTime)
    {
        if (fireCooldownCoroutine != null) StopCoroutine(fireCooldownCoroutine);
        fireCooldownCoroutine = StartCoroutine(FireSkillCooldownRoutine(cooldownTime));
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

    public void StartBattoSkillCooldown(float cooldownTime)
    {
        if (battoCooldownCoroutine != null) StopCoroutine(battoCooldownCoroutine);
        battoCooldownCoroutine = StartCoroutine(BattoSkillCooldownRoutine(cooldownTime));
    }

    IEnumerator BattoSkillCooldownRoutine(float cooldownTime)
    {
        battoSkillTimer.gameObject.SetActive(true);
        battoSkillTimer.fillAmount = 1f;

        float elapsed = 0f;

        while (elapsed < cooldownTime)
        {
            elapsed += Time.deltaTime;
            battoSkillTimer.fillAmount = 1f - (elapsed / cooldownTime);
            yield return null;
        }

        battoSkillTimer.fillAmount = 0f;
        battoSkillTimer.gameObject.SetActive(false);
    }

    void OnHealthChanged(float normalizedHP)
    {
        // �չ� ���
        hpFillImage.fillAmount = normalizedHP;

        // �޹� �ִϸ��̼�
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

    public IEnumerator AnimateBar(Image barImage, float targetFill)
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

    public void AddTargetMonster(MonsterState m)
    {
        // �̹� ǥ�� ���̸� �н�
        if (activeBars.Exists(b => b.nameText.text == m.monsterName))
            return;

        // ���� ������ ������ ���� ������ �� ����
        if (activeBars.Count >= maxBars)
        {
            var old = activeBars[0];
            activeBars.RemoveAt(0);
            Destroy(old.gameObject);
        }

        // ������ �ν��Ͻ�ȭ
        var bar = Instantiate(monsterHPBarPrefab, monsterHPParent);
        bar.Initialize(m);
        activeBars.Add(bar);
    }

    // ���Ͱ� ����ϰų� Ÿ�ٿ��� ������ �� ȣ��
    public void RemoveTargetMonster(MonsterState m)
    {
        var bar = activeBars.Find(b => b.nameText.text == m.monsterName);
        if (bar != null)
        {
            activeBars.Remove(bar);
            Destroy(bar.gameObject);
        }
    }

}
