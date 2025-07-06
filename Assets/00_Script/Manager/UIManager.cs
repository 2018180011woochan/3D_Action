using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HP Bar")]
    [SerializeField] private Image fillImage;

    [Header("References")]
    [SerializeField] private PlayerState playerHealth;

    void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.onHealthChanged.AddListener(UpdateHPBar);
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.onHealthChanged.RemoveListener(UpdateHPBar);
    }

    void Start()
    {
        if (fillImage == null)
            Debug.LogError("Fill Image 슬롯이 비어있습니다!");
        if (playerHealth == null)
            Debug.LogError("Player Health 슬롯이 비어있습니다!");

        // 초기값 세팅
        if (playerHealth != null)
            UpdateHPBar(playerHealth.currentHP / playerHealth.maxHP);
    }

    void UpdateHPBar(float normalizedHP)
    {
        Debug.Log($"[UIManager] UpdateHPBar → {normalizedHP}");
        fillImage.fillAmount = normalizedHP;
    }
}
