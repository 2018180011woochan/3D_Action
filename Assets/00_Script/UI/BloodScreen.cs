using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BloodScreen : MonoBehaviour
{
    public Image overlay;

    [Header("Q Vignette (선택)")]
    public Q_Vignette_Single vignette;     // ← 인스펙터에 드래그
    [Range(0, 1f)] public float vignetteFlashAlpha = 0.45f;
    public float vignetteScalePulse = 0.08f;

    [Header("페이드 세팅")]
    public float flashAlpha = 0.6f;
    public float fadeInTime = 0.05f;
    public float fadeOutTime = 0.5f;

    private Coroutine routine;
    Color baseOverlay;
    Color baseVignette;
    float baseVignetteScale;

    void Awake()
    {
        if (overlay)
        {
            baseOverlay = overlay.color;
            baseOverlay.a = 0f;
            overlay.color = baseOverlay;
        }

        if (vignette)
        {
            baseVignette = vignette.mainColor;
            baseVignette.a = 0f;
            vignette.mainColor = baseVignette;
            baseVignetteScale = vignette.mainScale;

            // UI 가리지 않도록
            foreach (var img in vignette.GetComponentsInChildren<Image>(true))
                img.raycastTarget = false;
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

        // 빠르게 켜짐
        while (t < fadeInTime)
        {
            float k = t / fadeInTime;

            if (overlay)
            {
                var c = overlay.color;
                c.a = Mathf.Lerp(0f, flashAlpha, k);
                overlay.color = c;
            }

            if (vignette)
            {
                var c2 = vignette.mainColor;
                c2.a = Mathf.Lerp(0f, vignetteFlashAlpha, k);
                vignette.mainColor = c2;
                vignette.mainScale = Mathf.Lerp(baseVignetteScale, baseVignetteScale + vignetteScalePulse, k);
            }

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // 서서히 꺼짐
        t = 0f;
        while (t < fadeOutTime)
        {
            float k = t / fadeOutTime;

            if (overlay)
            {
                var c = overlay.color;
                c.a = Mathf.Lerp(flashAlpha, 0f, k);
                overlay.color = c;
            }

            if (vignette)
            {
                var c2 = vignette.mainColor;
                c2.a = Mathf.Lerp(vignetteFlashAlpha, 0f, k);
                vignette.mainColor = c2;
                vignette.mainScale = Mathf.Lerp(baseVignetteScale + vignetteScalePulse, baseVignetteScale, k);
            }

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // 완전 초기화
        if (overlay) { var c = overlay.color; c.a = 0f; overlay.color = c; }
        if (vignette) { var c2 = vignette.mainColor; c2.a = 0f; vignette.mainColor = c2; vignette.mainScale = baseVignetteScale; }
        routine = null;
    }
}
