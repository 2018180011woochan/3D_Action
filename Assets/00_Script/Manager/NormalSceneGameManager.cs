using UnityEngine;

public class NormalSceneGameManager : MonoBehaviour
{
    public static NormalSceneGameManager Instance;
    public GameObject PortalPrefab;
    public int GameEndCnt = 0;
    void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (GameEndCnt >= 2)
        {
            PortalPrefab.SetActive(true);
        }
    }
}
