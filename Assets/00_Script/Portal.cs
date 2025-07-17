using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // 플레이어와 충돌했는지 확인
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어가 포탈에 접촉했습니다!");
            SceneManager.LoadScene("LoadingScene");
        }
    }
}