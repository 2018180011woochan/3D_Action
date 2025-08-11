using System.Collections;
using UnityEngine;

public class NewDissolveOnDeath : MonoBehaviour
{
    [Header("Hook")]
    public MonoBehaviour monsterStateComponent; // MonsterState를 드래그 (또는 비워두면 폴링)

    [Header("Renderers to dissolve")]
    public Renderer[] targets;                  // RockGolemMesh(SkinnedMeshRenderer) 드래그

    [Header("Dissolve Settings")]
    public string dissolveProperty = "_Dissolve"; // 머터리얼에서 본 정확한 이름으로!
    public float startDelay = 3f;   // 죽고 3초 후 시작
    public float duration = 1.5f;   // 녹는 시간
    public bool destroyAtEnd = true;

    int _propId;
    bool _started;
    bool _wasDead;

    void Awake()
    {
        _propId = Shader.PropertyToID(dissolveProperty);

        if (targets == null || targets.Length == 0)
            targets = GetComponentsInChildren<Renderer>(true);

        foreach (var r in targets)
        {
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = new Material(mats[i]);
                if (mats[i].HasProperty(_propId)) mats[i].SetFloat(_propId, 0f);
            }
            r.materials = mats;
        }
    }

    void Update()
    {
        if (_started) return;

        bool deadNow = IsDead();
        if (!_wasDead && deadNow)
        {
            _started = true;
            StartCoroutine(DissolveRoutine());
        }
        _wasDead = deadNow;
    }

    IEnumerator DissolveRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float v = Mathf.Clamp01(t / duration); // 0→1
            foreach (var r in targets)
                foreach (var m in r.materials)
                    if (m.HasProperty(_propId)) m.SetFloat(_propId, v);
            yield return null;
        }

        if (destroyAtEnd) Destroy(gameObject);
        else
        {
            foreach (var r in targets) r.enabled = false;
        }
    }

    bool IsDead()
    {
        if (monsterStateComponent == null) 
            return false;

        var t = monsterStateComponent.GetType();
        var f = t.GetField("isDead");
        if (f != null && f.FieldType == typeof(bool))
            return (bool)f.GetValue(monsterStateComponent);

        var p = t.GetProperty("isDead");
        if (p != null && p.PropertyType == typeof(bool))
            return (bool)p.GetValue(monsterStateComponent);

        return false;
    }
}
