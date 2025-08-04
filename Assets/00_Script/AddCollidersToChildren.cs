using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AddCollidersToChildren : MonoBehaviour
{
    [ContextMenu("Add Colliders to All Children")]
    void AddColliders()
    {
#if UNITY_EDITOR
        // MeshRenderer가 있는 자식들에게만 콜라이더 추가
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        int addedCount = 0;

        foreach (MeshRenderer mr in meshRenderers)
        {
            if (mr.GetComponent<Collider>() == null)
            {
                // MeshFilter가 있으면 MeshCollider, 없으면 BoxCollider 추가
                if (mr.GetComponent<MeshFilter>() != null)
                {
                    mr.gameObject.AddComponent<MeshCollider>();
                }
                else
                {
                    mr.gameObject.AddComponent<BoxCollider>();
                }
                addedCount++;
            }
        }

        Debug.Log($"Added colliders to {addedCount} objects out of {meshRenderers.Length} total objects");
#endif
    }

    [ContextMenu("Remove All Child Colliders")]
    void RemoveColliders()
    {
#if UNITY_EDITOR
        Collider[] colliders = GetComponentsInChildren<Collider>();
        int removedCount = 0;

        foreach (Collider col in colliders)
        {
            if (col.gameObject != this.gameObject) // 자기 자신의 콜라이더는 제외
            {
                DestroyImmediate(col);
                removedCount++;
            }
        }

        Debug.Log($"Removed {removedCount} colliders");
#endif
    }
}