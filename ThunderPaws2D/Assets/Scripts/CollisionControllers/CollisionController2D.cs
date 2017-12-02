using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Physics controller that can be given to any object with a box collider and it can make use of collision detection
/// </summary>
public class CollisionController2D : RaycastController {
    /// <summary>
    /// LayerMask to determine which objects we want THIS to collide with
    /// </summary>
    public LayerMask CollisionMask;
    /// <summary>
    /// Struct containing collision info
    /// </summary>
    public CollisionInfo Collisions;
    /// <summary>
    /// Sound name to play when we land
    /// </summary>
    public string JumpLanding = "JumpLanding";
    /// <summary>
    /// AudiManager reference for playing sounds
    /// </summary>
    //private AudioManager _audioManager;

    /// <summary>
    /// Maximum angle upward we can traverse up
    /// </summary>
    private float _maxClimbAngle = 80f;
    /// <summary>
    /// Maximum angle downward we can traverse
    /// </summary>
    private float _maxDescendAngle = 75f;

    /// <summary>
    /// Player input
    /// </summary>
    public Vector2 PlayerInput;

    public override void Start() {
        base.Start();
        //_audioManager = AudioManager.instance;
        //if (_audioManager == null) {
        //    throw new MissingComponentException("No AudioManager found");
        //}
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
    /// <param name="velocity"></param>
    public void Move(Vector3 velocity, Vector2 input) {
        PlayerInput = input;
        UpdateRaycasyOrigins();
        bool wasGrounded = Collisions.FromBelow;
        Collisions.Reset();
        //Only check for descending slope if we're moving downward. 
        if (velocity.y < 0) {
            DescendSlope(ref velocity);
        }
        if (velocity.x != 0) {
            CalculateHorizontalCollisions(ref velocity);
        }
        if (velocity.y != 0) {
            CalculateVerticalCollisions(ref velocity);
        }
        if (wasGrounded != Collisions.FromBelow && Collisions.FromBelow) {
            //_audioManager.playSound(JumpLanding);
        }
        //Only calculaate near ledge if we're standing on something. 
        if (velocity.x != 0 && Collisions.FromBelow) {
            CalculateNearLedge(ref velocity);
        }
        //Move the object
        transform.Translate(velocity);
    }

    /// <summary>
    /// Starting at either bottom left or right depending on which horizontal direction object is moving
    /// Draw ray from origin on the object out to see if it collides with anything
    /// If it does - set ray distance to that for all rays left to cast
    /// Set velocity.x to the distance to the nearest collision
    /// </summary>
    /// <param name="velocity"></param>
    public void CalculateHorizontalCollisions(ref Vector3 velocity) {
        //get direction of x velocity + up  - down
        float directionX = Mathf.Sign(velocity.x);
        //length of ray
        float rayLength = Mathf.Abs(velocity.x) + SkinWidth;

        for (int i = 0; i < HorizontalRayCount; ++i) {
            //check in which direction we're moving
            //down = start bottom left, up = start top left
            Vector2 rayOrigin = (directionX == -1) ? RayOrigins.BottomLeft : RayOrigins.BottomRight;
            //moves it along the x values - top and bottom faces
            rayOrigin += Vector2.up * (HorizontalRaySpacing * i);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, CollisionMask);
            if (hit) {
                //Check if we're within a platform - I.E. jumping up or falling down through one way platforms
                if (hit.distance == 0) {
                    continue;
                }

                //Get the angle of the surface we hit  (handle slopes)
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                //only do this for the bottom most
                if (i == 0 && slopeAngle <= _maxClimbAngle) {
                    //Don't start moving upward at an angle until we hit the actual slope (this fixes a teeny gap between collider and surface)
                    float distanceToSlopeStart = 0f;
                    if (slopeAngle != Collisions.SlopeAnglePrevFrame) {
                        distanceToSlopeStart = hit.distance - SkinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                //Only continue checking collisions if we're not climbing the slope
                if (!Collisions.ClimbingSlope || slopeAngle > _maxClimbAngle) {

                    //distance from us to the object <= velocity.x so set it to that
                    velocity.x = (hit.distance - SkinWidth) * directionX;
                    //change ray length once we hit the first thing because we shouldn't cast rays FURTHER than this min one
                    rayLength = hit.distance;

                    //Must update velocity on the Y axis since we're moving at an upwards angle
                    if (Collisions.ClimbingSlope) {
                        velocity.y = Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    //Set collision info
                    Collisions.FromLeft = (directionX == -1);
                    Collisions.FromRight = (directionX == 1);
                }
            }
        }
    }

    /// <summary>
    /// Starting at either bottom or top depending on which vertical direction object is moving
    /// Draw ray from origin on the object out to see if it collides with anything
    /// If it does - set ray distance to that for all rays left to cast
    /// Set velocity.y to the distance to the nearest collision
    /// </summary>
    /// <param name="velocity"></param>
    public void CalculateVerticalCollisions(ref Vector3 velocity) {
        //get direction of y velocity + up  - down
        float directionY = Mathf.Sign(velocity.y);
        //length of ray
        float rayLength = Mathf.Abs(velocity.y) + SkinWidth;

        for (int i = 0; i < VerticalRayCount; ++i) {
            //check in which direction we're moving
            //down = start bottom left, up = start top left
            Vector2 rayOrigin = (directionY == -1) ? RayOrigins.BottomLeft : RayOrigins.TopLeft;
            //moves it along the x values - top and bottom faces
            rayOrigin += Vector2.right * (VerticalRaySpacing * i + velocity.x);//addition of velocity.x allows the ray to be drawn where we WILL move to - otherwise by the time the ray is drawn we'd move past it
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, CollisionMask);
            if (hit) {
                //Check for one way platforms - or completely through ones
                if (hit.collider.tag == "OBSTACLE-THROUGH") {
                    if (directionY == 1 || hit.distance == 0) {
                        continue;
                    }
                }
                //Do not collide if we're currently falling through the platform
                if (Collisions.FallingThroughPlatform) {
                    continue;
                }
                //Give the player half a second chance to fall through the platform
                if (PlayerInput.y == -1 && hit.collider.tag == "OBSTACLE-THROUGH") {
                    Collisions.FallingThroughPlatform = true;
                    Invoke("ResetFallingThroughPlatform", 0.25f);
                    continue;
                }

                //distance from us to the object <= velocity.y so set it to that
                velocity.y = (hit.distance - SkinWidth) * directionY;
                //change ray length once we hit the first thing because we shouldn't cast rays FURTHER than this min one
                rayLength = hit.distance;

                //Set collision info
                Collisions.FromBelow = (directionY == -1);
                Collisions.FromAbove = (directionY == 1);
            }
        }

        //Check for a new slope angle while on the current slope
        if (Collisions.ClimbingSlope) {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + SkinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? RayOrigins.BottomLeft : RayOrigins.BottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, CollisionMask);
            if (hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                //We have collided with a new slope whilst climbing a slope
                if (slopeAngle != Collisions.SlopeAngle) {
                    velocity.x = (hit.distance - SkinWidth) * directionX;
                    Collisions.SlopeAngle = slopeAngle;
                }
            }
        }
    }

    /// <summary>
    /// Recalculate velocity based off the angle of slope we're hitting
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="slopeAngle"></param>
    private void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        if (velocity.y <= climbVelocityY) {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            //Must manually set collisions below to true since we're tachnically moving upward on the slope so our Velocity.y > 0
            Collisions.FromBelow = true;
            Collisions.ClimbingSlope = true;
            Collisions.SlopeAngle = slopeAngle;
        }
    }

    /// <summary>
    /// Calculate the velocity based off going down a slope
    /// </summary>
    /// <param name="velocity"></param>
    private void DescendSlope(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = ((directionX == -1) ? RayOrigins.BottomRight : RayOrigins.BottomLeft);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, CollisionMask);
        if (hit) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0f && slopeAngle <= _maxDescendAngle) {
                //Indicates we're moving down the slope because the slopeAngle.x is moving in the same direction as we are
                if (Mathf.Sign(hit.normal.x) == directionX) {
                    //If the distance to the slope is less than how far we need to move based on the slopeangle, we're close enough for the slope to effect us
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

    /// <summary>
    /// After a set interval reset the collisions
    /// </summary>
    void ResetFallingThroughPlatform() {
        Collisions.FallingThroughPlatform = false;
    }

    /// <summary>
    /// Specific vertical collision checking used for AI determining if they're near a ledge
    /// </summary>
    /// <param name="velocity"></param>
    public void CalculateNearLedge(ref Vector3 velocity) {
        //get direction of y velocity + up  - down
        float directionY = Mathf.Sign(velocity.y);
        //length of ray
        float rayLength = Mathf.Abs(velocity.y) + SkinWidth;

        //check in which direction we're moving
        Vector2 rayOrigin = (Mathf.Sign(velocity.x) == -1) ? RayOrigins.BottomLeft : RayOrigins.BottomRight;
        //moves it along the x values - top and bottom faces
        rayOrigin += Vector2.right * velocity.x;//We either need the bottom left or bottom right
        Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.green);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, CollisionMask);
        if (!hit) {
            //We are standing on a ledge where the bottom left or bottom right is hanging over the edge
            Collisions.NearLedge = true;
        }
    }


    /// <summary>
    /// Stores information about the collision - where it occurred..etc
    /// </summary>
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

}
