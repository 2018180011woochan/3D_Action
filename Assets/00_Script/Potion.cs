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
}
