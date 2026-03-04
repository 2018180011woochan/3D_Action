using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // 내 캐릭터가 닿았을 때만 서버에 요청!
        var movement = other.GetComponent<SamuraiMovement>();
        if (movement != null && movement.isMine)
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.SendEnterPortalPacket();
            }
        }
    }
}
