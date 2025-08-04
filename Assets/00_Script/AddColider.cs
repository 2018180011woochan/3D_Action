using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class AddColider : MonoBehaviour
{
    [ContextMenu("모든 메시에 BoxCollider 추가")]
    void AddBoxCollidersToAllMeshes()
    {
#if UNITY_EDITOR

        MeshRenderer[] allMeshRenderers = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer meshRenderer in allMeshRenderers)
        {
            if (meshRenderer.GetComponent<Collider>() != null)
            {
                continue;
            }

            BoxCollider boxCol = meshRenderer.gameObject.AddComponent<BoxCollider>();

            Bounds bounds = meshRenderer.bounds;
            boxCol.center = meshRenderer.transform.InverseTransformPoint(bounds.center);
            boxCol.size = bounds.size / meshRenderer.transform.lossyScale.x;
        }


        // 변경사항 저장
        EditorUtility.SetDirty(gameObject);
#endif
    }
}
