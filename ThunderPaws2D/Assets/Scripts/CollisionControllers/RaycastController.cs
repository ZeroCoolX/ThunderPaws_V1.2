using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {
    [HideInInspector]
    public const float SkinWidth = 0.015f;
    [HideInInspector]
    public int HorizontalRayCount = 4;
    [HideInInspector]
    public int VerticalRayCount = 4;
    [HideInInspector]
    public float HorizontalRaySpacing;
    [HideInInspector]
    public float VerticalRaySpacing;
    [HideInInspector]
    public BoxCollider2D BoxCollider;
    [HideInInspector]
    public RaycastOrigins RayOrigins;

    public virtual void Start() {
        BoxCollider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void UpdateRaycasyOrigins() {
        try {
            Bounds bounds = BoxCollider.bounds;
            // Shrink in the bounds by -2 on all sides to give a more realistic collision effect
            bounds.Expand(SkinWidth * -2);

            RayOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            RayOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
            RayOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
            RayOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
        } catch (System.Exception e) {
            print("An Exception was generated while trying to update raycast origins");
        }
    }

    /// <summary>
    /// Determine the spacing needed to evenly spread rays 
    /// </summary>
    public void CalculateRaySpacing() {
        Bounds bounds = BoxCollider.bounds;
        bounds.Expand(SkinWidth * -2);

        //Ensure we have at least 2 rays firing in the horizontal and vertical directions
        HorizontalRayCount = Mathf.Clamp(HorizontalRayCount, 2, int.MaxValue);
        VerticalRayCount = Mathf.Clamp(VerticalRayCount, 2, int.MaxValue);

        //(size of face rays come out of) / (1 less than count desired)
        HorizontalRaySpacing = bounds.size.y / (HorizontalRayCount - 1);
        VerticalRaySpacing = bounds.size.x / (VerticalRayCount - 1);

    }

    /// <summary>
    /// Stores information about our raycasts at the 4 corners of our box collider
    /// </summary>
    public struct RaycastOrigins {
        public Vector2 TopLeft, TopRight;
        public Vector2 BottomLeft, BottomRight;
    }
}
