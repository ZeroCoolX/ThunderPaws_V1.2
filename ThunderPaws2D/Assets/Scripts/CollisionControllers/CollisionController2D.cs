using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Physics controller that can be given to any object with a box collider and it can make use of collision detection
/// </summary>
public class CollisionController2D : RaycastController {
    public LayerMask CollisionMask;
    public bool NotifyOnCollision = false;
    public delegate void NotifyPlayerBouncebackDelegate(float directionFrom);
    public NotifyPlayerBouncebackDelegate NotifyCollision;
    public struct CollisionInfo {
        public bool FromAbove, FromBelow;
        public bool FromLeft, FromRight;

        public bool NearLedge;
        public bool FallingThroughPlatform;

        public bool ClimbingSlope, DescendingSlope;
        public float SlopeAngle, SlopeAnglePrevFrame;

        public void Reset() {
            FromAbove = FromBelow = false;
            FromLeft = FromRight = false;

            NearLedge = false;
            FallingThroughPlatform = false;

            ClimbingSlope = false;
            DescendingSlope = false;
            SlopeAnglePrevFrame = SlopeAngle;
            SlopeAngle = 0f;
        }
    }
    public CollisionInfo Collisions;

    private Vector2 _playerInput;
    private float _maxSlopeClimbAngle = 80f;
    private float _maxSlopeDescendAngle = 75f;



    public override void Start() {
        base.Start();
    }

    public void Move(Vector3 velocity) {
        Move(velocity, Vector2.zero);
    }

    /// <summary>
    /// Update raycast origins to where we're moving to.
    /// Reset collisions.
    /// Calculate both vertical and horizontal collisions.
    /// Move object
    /// Optional input parameter is for one way platforms. Need a reference to the player input to know if we should drop through platforms
    /// </summary>
    public void Move(Vector3 velocity, Vector2 input, string playerJoystickId = "") {
        _playerInput = input;

        UpdateRaycasyOrigins();

        bool wasGrounded = Collisions.FromBelow;
        Collisions.Reset();

        // Only check for descending slope if we're moving downward. 
        if (velocity.y < 0) {
            DescendSlope(ref velocity);
        }
        if (velocity.x != 0) {
            CalculateHorizontalCollisions(ref velocity);
        }
        if (velocity.y != 0) {
            CalculateVerticalCollisions(ref velocity, playerJoystickId);
        }
        // Only calculaate near ledge if we're standing on something. 
        if (velocity.x != 0 && Collisions.FromBelow) {
            CalculateNearLedge(ref velocity);
        }

        // Move the object
        transform.Translate(velocity);
    }

    /// <summary>
    /// Starting at either bottom left or right depending on which horizontal direction object is moving
    /// Draw ray from origin on the object out to see if it collides with anything
    /// If it does - set ray distance to that for all rays left to cast
    /// Set velocity.x to the distance to the nearest collision
    /// </summary>
    /// <param name="velocity"></param>
    private void CalculateHorizontalCollisions(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SkinWidth;
        var notifyPlayerBounceback = false;
        for (int i = 0; i < HorizontalRayCount; ++i) {
            // Check in which direction we're moving to that we always start at the furthest advaancing point
            Vector2 rayOrigin = (directionX == -1) ? RayOrigins.BottomLeft : RayOrigins.BottomRight;
            rayOrigin += Vector2.up * (HorizontalRaySpacing * i);
            // Debug only
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, CollisionMask);
            if (hit) {
                // Check if we're within a platform - I.E. jumping up or falling down through one way platforms
                if (hit.distance == 0) {
                    continue;
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                // Only do this for the bottom most
                if (i == 0 && slopeAngle <= _maxSlopeClimbAngle) {
                    // Don't start moving upward at an angle until we hit the actual slope (this fixes a teeny gap between collider and surface)
                    float distanceToSlopeStart = 0f;
                    if (slopeAngle != Collisions.SlopeAnglePrevFrame) {
                        distanceToSlopeStart = hit.distance - SkinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }

                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                // Only continue checking collisions if we're not climbing the slope
                if (!Collisions.ClimbingSlope || slopeAngle > _maxSlopeClimbAngle) {

                    // Distance from us to the object <= velocity.x so set it to that
                    velocity.x = (hit.distance - SkinWidth) * directionX;
                    if (NotifyOnCollision && (hit.distance - SkinWidth) < SkinWidth) {
                        // This indicates that a baddies collided with a player and the player should bounce back
                        if (hit.transform.gameObject.tag == GameConstants.Tag_Baddie) {
                            notifyPlayerBounceback = true;
                        }
                    }

                    // Change ray length once we hit the first thing because we shouldn't cast rays FURTHER than this min one
                    rayLength = hit.distance;

                    // Must update velocity on the Y axis since we're moving at an upwards angle
                    if (Collisions.ClimbingSlope) {
                        velocity.y = Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    Collisions.FromLeft = (directionX == -1);
                    Collisions.FromRight = (directionX == 1);
                }
            }
        }
        if (notifyPlayerBounceback) {
            NotifyCollision.Invoke(directionX);
        }
    }

    /// <summary>
    /// Starting at either bottom or top depending on which vertical direction object is moving
    /// Draw ray from origin on the object out to see if it collides with anything
    /// If it does - set ray distance to that for all rays left to cast
    /// Set velocity.y to the distance to the nearest collision
    /// </summary>
    /// <param name="velocity"></param>
    private void CalculateVerticalCollisions(ref Vector3 velocity, string joystickId = "J1-") {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SkinWidth;

        for (int i = 0; i < VerticalRayCount; ++i) {
            // Check in which direction we're moving to that we always start at the furthest advancing point
            Vector2 rayOrigin = (directionY == -1) ? RayOrigins.BottomLeft : RayOrigins.TopLeft;
            // Addition of velocity.x allows the ray to be drawn where we WILL move to - otherwise by the time the ray is drawn we'd move past it
            rayOrigin += Vector2.right * (VerticalRaySpacing * i + velocity.x);
            // Debug only
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, CollisionMask);
            if (hit) {

                if(CheckForThroughPlatforms(hit, directionY, joystickId)) {
                    continue;
                }

                velocity.y = (hit.distance - SkinWidth) * directionY;
                rayLength = hit.distance;

                // Set collision info
                Collisions.FromBelow = (directionY == -1);
                Collisions.FromAbove = (directionY == 1);
            }
        }

        // Check for a new slope angle while on the current slope
        if (Collisions.ClimbingSlope) {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + SkinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? RayOrigins.BottomLeft : RayOrigins.BottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, CollisionMask);
            if (hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                // We have collided with a new slope whilst climbing a slope
                if (slopeAngle != Collisions.SlopeAngle) {
                    velocity.x = (hit.distance - SkinWidth) * directionX;
                    Collisions.SlopeAngle = slopeAngle;
                }
            }
        }
    }

    private bool CheckForThroughPlatforms(RaycastHit2D hit, float directionY, string joystickId) {
        if (gameObject.tag.Contains(GameConstants.Tag_Baddie) && hit.collider.tag == GameConstants.Tag_ObstacleThrough) {
            print("I do no think this is used");
            return true;
        }

        // Allow the player to pass through obstacles that they can jump up or fall through
        if (hit.collider.tag == GameConstants.Tag_ObstacleThrough) {
            if (directionY == 1 || hit.distance == 0) {
                return true;
            }
        }
        // Do not collide if we're currently falling through the platform
        if (Collisions.FallingThroughPlatform) {
            return true;
        }
        // Give the player half a second chance to fall through the platform
        if ((Input.GetKey(KeyCode.S) || (_playerInput.y < -0.25 && Input.GetButton(joystickId + GameConstants.Input_Jump)))
            && hit.collider.tag == GameConstants.Tag_ObstacleThrough) {
            Collisions.FallingThroughPlatform = true;
            Invoke("ResetFallingThroughPlatform", 0.25f);
            return true;
        }
        return false;
    }

    private void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        if (velocity.y <= climbVelocityY) {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            // Must manually set collisions below to true since we're technically moving upward on the slope so our Velocity.y > 0
            Collisions.FromBelow = true;
            Collisions.ClimbingSlope = true;
            Collisions.SlopeAngle = slopeAngle;
        }
    }

    private void DescendSlope(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = ((directionX == -1) ? RayOrigins.BottomRight : RayOrigins.BottomLeft);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, CollisionMask);
        if (hit) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0f && slopeAngle <= _maxSlopeDescendAngle) {
                // Indicates we're moving down the slope because the slopeAngle.x is moving in the same direction as we are
                if (Mathf.Sign(hit.normal.x) == directionX) {
                    // If the distance to the slope is less than how far we need to move based on the slopeangle, we're close enough for the slope to effect us
                    if (hit.distance - SkinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) {
                        float slopeMoveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeMoveDistance;
                        velocity.y -= descendVelocityY;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * slopeMoveDistance * Mathf.Sign(velocity.x);

                        Collisions.SlopeAngle = slopeAngle;
                        Collisions.DescendingSlope = true;
                        Collisions.FromBelow = true;
                    }
                }
            }
        }
    }

    private void ResetFallingThroughPlatform() {
        Collisions.FallingThroughPlatform = false;
    }

    /// <summary>
    /// Specific vertical collision checking used for AI determining if they're near a ledge
    /// </summary>
    private void CalculateNearLedge(ref Vector3 velocity) {
        // Check in which direction we're moving to that we always start at the furthest advancing point
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SkinWidth * 2;

        Vector2 rayOrigin = (Mathf.Sign(velocity.x) == -1) ? RayOrigins.BottomLeft : RayOrigins.BottomRight;
        // We either need the bottom left or bottom right
        rayOrigin += Vector2.right * velocity.x;
        Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.green);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, CollisionMask);
        if (!hit) {
            Collisions.NearLedge = true;
        }
    }
}
