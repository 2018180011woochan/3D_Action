using System.Collections;
using UnityEngine;

public class DissolveOnDeath : MonoBehaviour
{
    [Header("Shader property name")]
    public string dissolveProperty = "_DissolveAmount";

    [Header("Timing")]
    public float waitBefore = 1f;   // 죽고 기다릴 시간
    public float duration = 1f;     // 디졸브 진행 시간

    [Header("Finish")]
    public bool deactivateOnEnd = true;   // true면 SetActive(false), false면 Destroy
    public GameObject disableTarget;      // 비우면 this.gameObject

    Renderer[] _renderers;
    MaterialPropertyBlock _block;
    int _propId;
    bool _running;

    void Awake()
    {
        if (!disableTarget) disableTarget = gameObject;
        _renderers = GetComponentsInChildren<Renderer>(true); // BODY만 있어도, 여러 파츠여도 OK
        _block = new MaterialPropertyBlock();
        _propId = Shader.PropertyToID(dissolveProperty);
    }

    void OnEnable() => SetValueAll(0f); // 풀 재사용 대비 원복

    public void Trigger()
    {
        if (!_running) StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        _running = true;
        if (waitBefore > 0) yield return new WaitForSeconds(waitBefore);

        float t = 0f;
        while (t < duration)
        {
            SetValueAll(Mathf.Lerp(0f, 1f, t / duration));
            t += Time.deltaTime;
            yield return null;
        }
        SetValueAll(1f);

        if (deactivateOnEnd) disableTarget.SetActive(false);
        else Destroy(disableTarget);

        _running = false;
    }

    void SetValueAll(float v)
    {
        if (_renderers == null) return;
        foreach (var r in _renderers)
        {
            if (!r) continue;
            int sub = r.sharedMaterials?.Length ?? 1;
            for (int i = 0; i < sub; i++)
            {
                _block.Clear();
                _block.SetFloat(_propId, v);
                r.SetPropertyBlock(_block, i);
            }
        }
    }
}
