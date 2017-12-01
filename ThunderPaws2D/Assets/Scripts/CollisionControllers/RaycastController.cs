using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {
    /// <summary>
    /// how much to shrink the box collider bounds by
    /// </summary>
    public const float SkinWidth = 0.015f;

    /// <summary>
    /// number of rays used horizontally on each side
    /// </summary>
    public int HorizontalRayCount = 4;
    /// <summary>
    /// number of rays used vertically on each side
    /// </summary>
    public int VerticalRayCount = 4;

    /// <summary>
    /// space in between each horizontal ray
    /// </summary>
    [HideInInspector]
    public float HorizontalRaySpacing;
    /// <summary>
    /// space in between each vertical ray
    /// </summary>
    [HideInInspector]
    public float VerticalRaySpacing;

    /// <summary>
    /// Box collider on the object
    /// </summary>
    [HideInInspector]
    public BoxCollider2D BoxCollider;
    /// <summary>
    /// Struct containing 4 corner raycast data
    /// </summary>
    public RaycastOrigins RayOrigins;

    public virtual void Start() {
        //box collider on the object
        BoxCollider = GetComponent<BoxCollider2D>();
        //Only calculate on changing of the values
        CalculateRaySpacing();
    }

    /// <summary>
    /// get the bounds of the box collider, shrink by skinwidth, and update raycast origin coordinates
    /// </summary>
    public void UpdateRaycasyOrigins() {
        Bounds bounds = BoxCollider.bounds;
        //shrink in the bounds by -2 on all sides
        bounds.Expand(SkinWidth * -2);

        RayOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        RayOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
        RayOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        RayOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    /// <summary>
    /// Determine the spacing needed to evenly spread rays 
    /// </summary>
    public void CalculateRaySpacing() {
        Bounds bounds = BoxCollider.bounds;
        //shrink in the bounds by -2 on all sides
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
