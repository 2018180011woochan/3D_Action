using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BloodScreen : MonoBehaviour
{
    [Header("BloodOverlay Image")]
    public Image overlay;       // 에디터에서 드래그할 BloodOverlay Image

    [Header("페이드 세팅")]
    public float flashAlpha = 0.6f;  // 최대 붉기
    public float fadeInTime = 0.05f; // 페이드 인 시간
    public float fadeOutTime = 0.5f; // 페이드 아웃 시간

    private Coroutine routine;

    void Awake()
    {
        // 초기에는 완전 투명
        if (overlay != null)
        {
            var c = overlay.color;
            c.a = 0f;
            overlay.color = c;
        }
    }

    public void Flash()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        float t = 0f;
        Color c = overlay.color;

        // 1) 페이드 인
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, flashAlpha, t / fadeInTime);
            overlay.color = c;
            yield return null;
        }

        // 2) 잠깐 대기 (원하면 yield return new WaitForSeconds(0.05f);)

        // 3) 페이드 아웃
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(flashAlpha, 0f, t / fadeOutTime);
            overlay.color = c;
            yield return null;
        }

        // 4) 마무리
        c.a = 0f;
        overlay.color = c;
        routine = null;
    }
}
