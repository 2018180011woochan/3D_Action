using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // 플레이어와 충돌했는지 확인
        if (other.CompareTag("Player"))
        {
            if (GameDataManager.instance == null)
            {
                Debug.LogError("[Portal] GameDataManager.instance가 null입니다!");

                // 긴급 생성 시도
                GameObject gdm = new GameObject("GameDataManager");
                gdm.AddComponent<GameDataManager>();
                Debug.Log("[Portal] GameDataManager 긴급 생성");
            }

            SaveAllData();
            //StopAllCoroutines();

            Debug.Log("플레이어가 포탈에 접촉했습니다!");
            SceneManager.LoadScene("LoadingScene");
        }
    }

    void SaveAllData()
    {
        Debug.Log("[Portal] 데이터 저장 시작");

        // 플레이어 체력 저장
        PlayerState player = GameObject.FindWithTag("Player").GetComponent<PlayerState>();
        player.SaveData();

        // 인벤토리 저장
        InventoryManager.instance.SaveData();

        Debug.Log("모든 데이터 저장 완료!");
    }
}