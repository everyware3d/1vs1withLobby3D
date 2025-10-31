using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
public class Box2DDebug : MonoBehaviour
{
    void OnDrawGizmos()
    {
        var box = GetComponent<BoxCollider2D>();
        if (!box.enabled) return;

        Gizmos.color = Color.purple;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(box.offset, box.size);
        Debug.Log("DRAWING GIZMO");
    }
}