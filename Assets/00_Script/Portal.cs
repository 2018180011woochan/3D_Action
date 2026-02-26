using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("포탈 목적지 씬 이름")]
    public string destinationScene = "BossScene1";   // 포탈마다 Inspector에서 지정

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // (선택) 세이브
        EnsureGDM();
        SaveAllData();

        SceneBridge.NextSceneName = destinationScene;
        SceneManager.LoadScene("LoadingScene");
    }

    void EnsureGDM()
    {
        if (GameDataManager.instance == null)
        {
            var gdm = new GameObject("GameDataManager");
            gdm.AddComponent<GameDataManager>();
        }
    }

    void SaveAllData()
    {
        var player = GameObject.FindWithTag("Player")?.GetComponent<PlayerState>();
        player?.SaveData();
        //InventoryManager.instance?.SaveData();
    }
}
