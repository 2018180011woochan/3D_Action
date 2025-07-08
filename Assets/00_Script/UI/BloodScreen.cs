using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BloodScreen : MonoBehaviour
{
    public Image overlay;      

    [Header("페이드 세팅")]
    public float flashAlpha = 0.6f;  
    public float fadeInTime = 0.05f; 
    public float fadeOutTime = 0.5f; 

    private Coroutine routine;

    void Awake()
    {
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

        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, flashAlpha, t / fadeInTime);
            overlay.color = c;
            yield return null;
        }

        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(flashAlpha, 0f, t / fadeOutTime);
            overlay.color = c;
            yield return null;
        }

        c.a = 0f;
        overlay.color = c;
        routine = null;
    }
}
