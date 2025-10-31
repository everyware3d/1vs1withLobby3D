using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class PongResizeListener : MonoBehaviour
{
    public static PongResizeListener Instance;
    PongResizeListener()
    {
        Instance = this;
    }
    private RectTransform rectTransform;
    private Vector2 lastSize;

    public Camera cam; // optional; will fallback to Camera.main
    public BoxCollider2D topWall, bottomWall;
    public BoxCollider2D leftGoal, rightGoal;

    [HideInInspector]
    public GameObject leftPlayer;
    [HideInInspector]
    public GameObject rightPlayer;

    public float wallThickness = 0.5f;      // world units
    public float padding = 0.5f;              // inset from edges (world units)
    public float goalThickness = 0.2f;     // post width (world units)
    public float goalHeightRatio = 1;   // % of screen height (0..1)
    public float goalInset = 0.5f;          // how far in from the left/right edges

    public float playerInset = 0.5f;          // how far in from the left/right edges

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        lastSize = new Vector2();
        // lastSize = rectTransform.rect.size;
        OnRectTransformDimensionsChange();
    }

    // This is automatically called by Unity
    void OnRectTransformDimensionsChange()
    {
        if (!rectTransform) return;

        Vector2 newSize = rectTransform.rect.size;

        if (newSize != lastSize)
        {
            lastSize = newSize;
            // Debug.Log($"RectTransform changed to: {newSize.x} x {newSize.y}");

            // Example: respond to the new size
            AdjustUIToNewSize(newSize);
        }
    }

    public void fireAdjustUIToSize()
    {
        AdjustUIToNewSize(lastSize);
    }

    /* Normalize between -1 and 1 to allow default to be 0 */
    public Vector3 normalToWorldPosition(Vector3 pos)
    {
        Vector3 ret = new Vector3(pos.x, pos.y, pos.z);
        ret.x = (ret.x * width) + left;
        ret.y = (ret.y * height) + bottom;
        // ret.x = (((1f + ret.x) / 2f) * width) + left;
        return ret;
    }
    public Vector3 worldToNormalPosition(Vector3 pos)
    {
        Vector3 ret = new Vector3(pos.x,pos.y,pos.z);
        ret.x = (ret.x - left) / width;
        ret.y = (ret.y - bottom) / height;
        // ret.x = 2f * ((ret.x - left) / width) - 1f;
        return ret;
    }
    float left, right, bottom, top, width, height;
    void AdjustUIToNewSize(Vector2 newSize)
    {
        // Do something with the new width/height
        // e.g., resize children, reposition, update layout
        // Debug.Log("Adjusting layout to match new screen size...");
        var c = cam != null ? cam : Camera.main;
        if (!c) return;
        var bl = c.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        var tr = c.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
        left = bl.x + padding;
        right = tr.x - padding;
        bottom = bl.y + padding;
        top = tr.y - padding;

        width = right - left;
        height = top - bottom;
        float midX = (left + right) * 0.5f;
        float midY = (bottom + top) * 0.5f;

        // --- Walls (BoxCollider2D sizes are in local space; assumes uniform scale = 1) ---
        if (topWall)
        {
            topWall.size = new Vector2(width, wallThickness);
            topWall.offset = Vector2.zero;
            topWall.transform.position = new Vector3(midX, top + wallThickness * 0.5f, 0f);
        }

        if (bottomWall)
        {
            bottomWall.size = new Vector2(width, wallThickness);
            bottomWall.offset = Vector2.zero;
            bottomWall.transform.position = new Vector3(midX, bottom - wallThickness * 0.5f, 0f);
        }

        // --- Goalposts (vertical posts centered vertically on each side) ---
        float goalHeight = Mathf.Clamp01(goalHeightRatio) * height;

        if (leftGoal)
        {
            leftGoal.size = new Vector2(goalThickness, goalHeight);
            leftGoal.offset = Vector2.zero;
            BoxCollider2D bc = leftGoal.GetComponent<BoxCollider2D>();
            bc.offset = new Vector2(-bc.size.x / 2.0f, 0.0f);
            leftGoal.transform.position = new Vector3(left + goalInset, midY, 0f);
        }

        if (rightGoal)
        {
            rightGoal.size = new Vector2(goalThickness, goalHeight);
            rightGoal.offset = Vector2.zero;
            BoxCollider2D bc = rightGoal.GetComponent<BoxCollider2D>();
            bc.offset = new Vector2(bc.size.x / 2.0f, 0.0f);
            rightGoal.transform.position = new Vector3(right - goalInset, midY, 0f);
        }
        if (leftPlayer)
        {
            Vector3 oldPos = leftPlayer.transform.position;
            leftPlayer.transform.position = new Vector3(left + playerInset, oldPos.y, oldPos.z);
        }
        if (rightPlayer)
        {
            Vector3 oldPos = rightPlayer.transform.position;
            rightPlayer.transform.position = new Vector3(right - playerInset, oldPos.y, oldPos.z);
        }
        
    }
}
