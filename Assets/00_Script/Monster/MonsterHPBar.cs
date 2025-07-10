using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterHPBar : MonoBehaviour
{
    [SerializeField] public Image hpFill;
    [SerializeField] public Image hpDamage;
    [SerializeField] public  TextMeshProUGUI nameText;

    private MonsterState target;
    private Coroutine damageAni;

    public void Initialize(MonsterState monster)
    {
        // 이전 구독 해제
        if (target != null)
            target.onHealthChanged.RemoveListener(OnHPChanged);

        target = monster;
        target.onHealthChanged.AddListener(OnHPChanged);

        nameText.text = target.monsterName;
        UpdateUIInstant(target.currentHP / target.maxHP);
    }

    private void OnHPChanged(float norm)
    {
        UpdateUIInstant(norm);
        if (damageAni != null) StopCoroutine(damageAni);
        damageAni = StartCoroutine(
            UIManager.Instance.AnimateBar(hpDamage, norm)
        );
    }

    private void UpdateUIInstant(float norm)
    {
        hpFill.fillAmount = norm;
        hpDamage.fillAmount = norm;
    }

    private void OnDestroy()
    {
        if (target != null)
            target.onHealthChanged.RemoveListener(OnHPChanged);
    }
}
