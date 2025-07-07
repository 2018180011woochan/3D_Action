using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BloodScreen : MonoBehaviour
{
    [Header("BloodOverlay Image")]
    public Image overlay;       // �����Ϳ��� �巡���� BloodOverlay Image

    [Header("���̵� ����")]
    public float flashAlpha = 0.6f;  // �ִ� �ӱ�
    public float fadeInTime = 0.05f; // ���̵� �� �ð�
    public float fadeOutTime = 0.5f; // ���̵� �ƿ� �ð�

    private Coroutine routine;

    void Awake()
    {
        // �ʱ⿡�� ���� ����
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

        // 1) ���̵� ��
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, flashAlpha, t / fadeInTime);
            overlay.color = c;
            yield return null;
        }

        // 2) ��� ��� (���ϸ� yield return new WaitForSeconds(0.05f);)

        // 3) ���̵� �ƿ�
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(flashAlpha, 0f, t / fadeOutTime);
            overlay.color = c;
            yield return null;
        }

        // 4) ������
        c.a = 0f;
        overlay.color = c;
        routine = null;
    }
}
