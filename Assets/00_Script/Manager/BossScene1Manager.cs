using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;
using System.Collections;

public class BossScene1Manager : MonoBehaviour
{
    public static BossScene1Manager Instance { get; private set; }
    public int GhostDeathCnt = 0;
    private bool cleared;

    [Header("보스 등장 컷씬")]
    public int targetGhostCount = 3;
    public CinemachineCamera playerCamera;   
    public CinemachineCamera bossCam;        
    public float showSeconds = 6.0f;
    bool playing;

    [Header("보스 스폰")]
    public GameObject bossPrefab;
    public GameObject bossSpawnPointPrefab;
    public Transform bossSpawnPoint;
    GameObject bossInstance;
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ReportGhostDeath()
    {
        if (cleared) return;

        GhostDeathCnt++;
        if (GhostDeathCnt >= 3 && !playing)
        {
            StartCoroutine(ShowBossCamRoutine());
        }
    }

    IEnumerator ShowBossCamRoutine()
    {
        playing = true;

        int oldPlayerPrio = playerCamera ? playerCamera.Priority : 0;
        int oldBossPrio = bossCam ? bossCam.Priority : 0;

        if (bossCam) bossCam.Priority = 100;

        Instantiate(bossSpawnPointPrefab, bossSpawnPoint.position, Quaternion.identity);
        yield return new WaitForSeconds(2f);
        SpawnBoss();

        yield return new WaitForSeconds(showSeconds);

        // 다시 플레이어로 복귀
        if (bossCam) bossCam.Priority = oldBossPrio;
        if (playerCamera) playerCamera.Priority = Mathf.Max(10, oldPlayerPrio);

        cleared = true;
        playing = false;
    }
    void SpawnBoss()
    {
        if (bossInstance != null) return;

        bossInstance = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
    }
}
