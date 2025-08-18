using System.Collections;
using UnityEngine;

public class ZombiDissolve : MonoBehaviour
{
    [Header("Shader property")]
    public string dissolveProp = "_Dissolve";
    public float delay = 2f;
    public float duration = 2f;
    public bool destroyOnComplete = true;   

    Renderer[] rends;
    MaterialPropertyBlock mpb;
    int idDissolve;

    void Awake()
    {
        rends = GetComponentsInChildren<Renderer>(true);
        mpb = new MaterialPropertyBlock();
        idDissolve = Shader.PropertyToID(dissolveProp);
        SetValue(0f);
    }

    void SetValue(float v)
    {
        for (int i = 0; i < rends.Length; i++)
        {
            var r = rends[i];
            r.GetPropertyBlock(mpb);
            mpb.SetFloat(idDissolve, v);
            r.SetPropertyBlock(mpb);
        }
    }

    public void Play()
    {
        StopAllCoroutines();
        StartCoroutine(CoPlay());
    }

    IEnumerator CoPlay()
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        if (duration <= 0f)
        {
            SetValue(1f);
            if (destroyOnComplete) Destroy(gameObject);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            SetValue(Mathf.Clamp01(t / duration));
            yield return null;
        }

        SetValue(1f);
        if (destroyOnComplete) Destroy(gameObject);  
    }
}
