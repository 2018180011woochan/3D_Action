using UnityEngine;

public class NormalSceneGameManager : MonoBehaviour
{
    public static NormalSceneGameManager Instance;
    public GameObject PortalPrefab;
    public GameObject NormalBoss;
    private MonsterState bossState;
    private bool portalOpened = false;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (PortalPrefab) PortalPrefab.SetActive(false);
    }

    public void OpenPortal()
    {
        if (PortalPrefab != null && !PortalPrefab.activeSelf)
        {
            PortalPrefab.SetActive(true);
            Debug.Log("보스 처치! 다음 스테이지 포탈 개방!");
        }
    }
}
