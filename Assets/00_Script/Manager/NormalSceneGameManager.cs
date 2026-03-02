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
        if (NormalBoss)
            bossState = NormalBoss.GetComponentInChildren<MonsterState>();

        if (PortalPrefab) PortalPrefab.SetActive(false);
    }

    private void Update()
    {
        if (bossState == null) return;

        if (!portalOpened && bossState.isDead)
        {
            OpenPortal();
        }
    }

    private void OpenPortal()
    {
        PortalPrefab.SetActive(true);
        portalOpened = true;
    }
}
