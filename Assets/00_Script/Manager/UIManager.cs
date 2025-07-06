using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HP Bar")]
    [SerializeField] private Image hpFillImage;     
    [SerializeField] private Image hpDamageImage;
    public float damageDelay = 0.5f;
    public float damageDuration = 0.5f;
    private Coroutine damageCoroutine;

    [SerializeField] private PlayerState playerState;

    void OnEnable()
    {
        if (playerState != null)
            playerState.onHealthChanged.AddListener(OnHealthChanged);
    }

    void OnDisable()
    {
        if (playerState != null)
            playerState.onHealthChanged.RemoveListener(OnHealthChanged);
    }

    void Start()
    {
        // �ʱ� ����
        float norm = playerState.currentHP / playerState.maxHP;
        hpFillImage.fillAmount = norm;
        hpDamageImage.fillAmount = norm;
    }

    void OnHealthChanged(float normalizedHP)
    {
        // 1) �չ�(����)�� ���
        hpFillImage.fillAmount = normalizedHP;

        // 2) �ڹ�(���)�� ���� �� õõ��
        if (damageCoroutine != null) StopCoroutine(damageCoroutine);
        damageCoroutine = StartCoroutine(AnimateDamageBar(normalizedHP));
    }

    IEnumerator AnimateDamageBar(float targetFill)
    {
        // ��� ��ٷȴٰ�
        yield return new WaitForSeconds(damageDelay);

        float startFill = hpDamageImage.fillAmount;
        float elapsed = 0f;

        while (elapsed < damageDuration)
        {
            elapsed += Time.deltaTime;
            hpDamageImage.fillAmount =
                Mathf.Lerp(startFill, targetFill, elapsed / damageDuration);
            yield return null;
        }

        hpDamageImage.fillAmount = targetFill;
        damageCoroutine = null;
    }
}
