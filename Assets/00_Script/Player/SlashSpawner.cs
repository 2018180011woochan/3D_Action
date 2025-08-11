using UnityEngine;

public class SlashSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawnPoint;          // 칼 본/빈오브젝트
    public GameObject slashPrefab;        // 슬래시 VFX 프리팹
    public float lifeTime = 1.2f;         // 파티클 자동 파괴 시간
    public Vector3 posOffset;             // 로컬 오프셋(필요시)
    public Vector3 rotOffsetEuler;        // 로컬 회전 오프셋(필요시)

    public void SpawnSlash()
    {
        if (!slashPrefab) return;
        Transform t = spawnPoint ? spawnPoint : transform;

        // 월드에 그대로 두고(부모X) 칼의 방향으로 회전
        Quaternion rot = t.rotation * Quaternion.Euler(rotOffsetEuler);
        Vector3 pos = t.position + t.TransformVector(posOffset);

        var go = Object.Instantiate(slashPrefab, pos, rot);
        if (go != null)
            Debug.Log("이펙트 생성 완료");
        if (lifeTime > 0) Object.Destroy(go, lifeTime);
    }
}