using UnityEngine;

public class Potion : MonoBehaviour
{
    public Item potion;
    public int droppedMonsterId;

    void OnTriggerEnter(Collider other)
    {
        var movement = other.GetComponent<SamuraiMovement>();
        if (movement != null && movement.isMine)
        {
            if (NetworkManager.Instance != null && potion != null)
            {
                NetworkManager.Instance.SendPickupItemPacket(potion.itemId, droppedMonsterId);
            }

            Destroy(gameObject);
        }
    }
}
