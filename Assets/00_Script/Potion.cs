using UnityEngine;

public class Potion : MonoBehaviour
{
    public Item potion;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (InventoryManager.instance.AddItem(potion))
            {
                Destroy(gameObject);
            }
        }
    }

    public void Heal()
    {
        Debug.Log("힐 시작!");

        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // PlayerState 컴포넌트 가져오기
            PlayerState ps = player.GetComponent<PlayerState>();
            if (ps != null)
            {
                // 5초 동안 매초 10씩 회복
                ps.StartHealOverTime(10f, 5f);
            }
            else
            {
                Debug.LogError("PlayerState 컴포넌트를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogError("플레이어를 찾을 수 없습니다!");
        }
    }
}
