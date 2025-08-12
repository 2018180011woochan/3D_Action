using UnityEngine;
using System.Collections.Generic;

public class BossScene1Manager : MonoBehaviour
{
    public static BossScene1Manager Instance { get; private set; }
    public int GhostDeathCnt = 0;
    private bool cleared;
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ReportGhostDeath()
    {
        if (cleared) return;

        GhostDeathCnt++;
        Debug.Log("유령 하나 사망");
        if (GhostDeathCnt >= 3)
        {
            cleared = true;
            // 보스 등장 컷씬 부터
            Debug.Log("유령 전부 사망");
        }
    }

}
