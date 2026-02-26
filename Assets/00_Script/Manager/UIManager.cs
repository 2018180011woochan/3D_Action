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

    void Start()
    {
        StartCoroutine(FindMyPlayerCoroutine());
    }

    private IEnumerator FindMyPlayerCoroutine()
    {
        while (playerState == null)
        {
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject p in allPlayers)
            {
                SamuraiMovement movement = p.GetComponent<SamuraiMovement>();
                if (movement != null && movement.isMine)
                {
                    playerState = p.GetComponent<PlayerState>();
                    Debug.Log("UIManager: 캐릭터 스폰 확인 UI 연결 성공");
                    break;
                }
            }

            if (playerState == null)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        playerState.onHealthChanged.AddListener(OnHealthChanged);
        playerState.onStaminaChanged.AddListener(OnStaminaChanged);

        OnHealthChanged(playerState.currentHP / playerState.maxHP);
        OnStaminaChanged(playerState.currentStamina / playerState.maxStamina);
    }

    void OnHealthChanged(float normalizedHP)
    {
        hpFillImage.fillAmount = normalizedHP;

        if (hpCoroutine != null) StopCoroutine(hpCoroutine);
        hpCoroutine = StartCoroutine(AnimateBar(hpDamageImage, normalizedHP));
    }

    void OnStaminaChanged(float normalizedStam)
    {
        stamFillImage.fillAmount = normalizedStam;

        if (stamCoroutine != null) StopCoroutine(stamCoroutine);
        stamCoroutine = StartCoroutine(AnimateBar(stamDamageImage, normalizedStam));
    }

    public IEnumerator AnimateBar(Image barImage, float targetFill)
    {
        yield return new WaitForSeconds(damageDelay);

        float startFill = barImage.fillAmount;
        float elapsed = 0f;

        while (elapsed < damageDuration)
        {
            elapsed += Time.deltaTime;
            barImage.fillAmount = Mathf.Lerp(startFill, targetFill, elapsed / damageDuration);
            yield return null;
        }

        barImage.fillAmount = targetFill;
    }

    public void AddTargetMonster(MonsterState m)
    {
        if (activeBars.Exists(b => b.nameText.text == m.monsterName)) return;

        if (activeBars.Count >= maxBars)
        {
            var old = activeBars[0];
            activeBars.RemoveAt(0);
            Destroy(old.gameObject);
        }

        var bar = Instantiate(monsterHPBarPrefab, monsterHPParent);
        bar.Initialize(m);
        activeBars.Add(bar);
    }

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