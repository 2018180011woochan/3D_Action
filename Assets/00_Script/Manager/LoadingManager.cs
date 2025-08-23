using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public Slider loadingBar;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI tipText;

    private string[] gameTips = {
        "팁: 적의 패턴을 파악하여 공격하세요",
        "팁: 방어 자세로 막을 수 없는 공격도 있습니다",
        "팁: Tap 키를 눌러 락온 기능을 사용할 수 있습니다",

    };

    void Start()
    {
        StartCoroutine(LoadingProcess());
    }

    IEnumerator LoadingProcess()
    {
        float loadingTime = 5f; 
        float elapsedTime = 0f;
        float tipChangeTime = 2f; // 2초마다 팁 변경
        float lastTipChangeTime = 0f;
        int currentTipIndex = 0;

        tipText.text = gameTips[currentTipIndex];

        while (elapsedTime < loadingTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / loadingTime;

            loadingBar.value = progress;
            loadingText.text = $"로딩중... {(progress * 100):F0}%";

            if (elapsedTime - lastTipChangeTime >= tipChangeTime)
            {
                currentTipIndex = (currentTipIndex + 1) % gameTips.Length;
                tipText.text = gameTips[currentTipIndex];
                lastTipChangeTime = elapsedTime;
            }

            yield return null;
        }

        loadingText.text = "로딩 완료! Press Any Key";

        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        var target = SceneBridge.NextSceneName;
        SceneBridge.NextSceneName = null;
        UnityEngine.SceneManagement.SceneManager.LoadScene(target);
    }
}
