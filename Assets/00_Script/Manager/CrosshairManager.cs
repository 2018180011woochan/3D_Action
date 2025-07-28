using UnityEngine;

public class CrosshairManager : MonoBehaviour
{
    public static CrosshairManager Instance;

    [Header("조준점 UI")]
    public GameObject crosshair;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // 조준점 표시/숨기기
    public void ShowCrosshair(bool show)
    {
        if (crosshair != null)
        {
            crosshair.SetActive(show);
        }
    }
}
