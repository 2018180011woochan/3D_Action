using UnityEngine;

public class DeathDissolveHook : MonoBehaviour
{
    MonsterState _state;        // 네가 쓰는 스크립트
    DissolveOnDeath _dissolve;
    bool _fired;

    void Awake()
    {
        _state = GetComponent<MonsterState>();            
        if (!_state) _state = GetComponentInParent<MonsterState>();
        _dissolve = GetComponent<DissolveOnDeath>();
    }

    void Update()
    {
        if (!_fired && _state != null && _state.isDead)
        {
            _fired = true;
            // 죽음 처리 중 충돌/AI를 끄고 싶다면 여기서 오프
            // foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = false;

            _dissolve?.Trigger();  // 1초 대기 후 디졸브 시작
        }
    }
}
