using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Robot_GL1 : GroundBaddieLifeform {
    // These are the baddie specific values for properties that live on the parents
    // This could be extracted out into configs but I don't mind them living here
    // They're just used for initializing the property values that live on the 
    // parent classes
    private readonly float _gravity = -25.08f;
    private readonly float _health = 5f;
    private readonly float _shotDelay = 3f;
    private readonly int _moveSpeed = 5;
    private readonly float _visionLength = 10f;
    private float _maxStopSeconds = 2f;

    /// <summary>
    /// Indicates this baddie is bound by ledges
    /// </summary>
    public bool LedgeBound = false;
    /// <summary>
    /// Used only by ledge bound baddies.
    /// Tells the baddie when they have reached an edge and should turn back around
    /// </summary>
    private bool _turnAround = true;
    /// <summary>
    /// Used only ledge bound baddies.
    /// Indicates we have seen the target and should halt movement
    /// This allows a delay even after the target is out of collision
    /// sight contact so it looks like the baddie "knows" the target
    /// was just here and might return - before returning to wandering
    /// </summary>
    private float _alertTimeThreshold = -1f;

    public void Start() {
        base.Start();

        // Set baddie specific data
        GroundPositionData.ShotDelay = _shotDelay;
        GroundPositionData.MoveSpeed = _moveSpeed;
        GroundPositionData.MoveDirection = Vector2.left;
        GroundPositionData.MaxStopSeconds = _maxStopSeconds;
        GroundPositionData.FireDelay = 1f;
        VisionRayLength = _visionLength;
        Gravity = _gravity;
        Health = _health;
    }

    public void Update() {
        base.Update();

        if (!CheckTargetsExist()) {
            return;
        }

        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - Target.position.x;

        if (LedgeBound) {
            // Check if we can shoot at the target
            var hCollider = FireRaycast();
            if (hCollider.collider != null) {
                _alertTimeThreshold = Time.time + 1f;
                HandleCollision();
            }
            // We previously saw the target but no longer see him.
            // Wait the max amount of time until going back to wandering
            if (_alertTimeThreshold >= Time.time) {
                print("Ledge bound baddie previously spotted an enemy and has stopped moving to investigate");
                Velocity = Vector2.zero;
                return;
            }
            // Update the way the baddie is facing
            CalculateFacingDirection(GroundPositionData.MoveDirection.x*-1);

            // Handle ledges
            if (Controller2d.Collisions.NearLedge && _turnAround) {
                print("Near ledge, prepare to turn around");
                GroundPositionData.MoveDirection.x  = Vector2.right.x * (FacingRight ? -1f : 1f);
                _turnAround = false;
                Invoke("ResetTurnAround", 0.5f);
            }
            //Move the baddie
            float targetVelocityX = GroundPositionData.MoveSpeed * GroundPositionData.MoveDirection.x;
            Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref GroundPositionData.VelocityXSmoothing, Controller2d.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        } else {
            // Check if we can shoot at the target
            var hCollider = FireRaycast();
            if (hCollider.collider != null) {
                HandleCollision();
            }

            // Face that direction
            CalculateFacingDirection(directionToTarget);

            // Move in that direction
            if (Time.time > GroundPositionData.TimeStopped) {
                CalculateVelocity();
            }
        }
        Move();
    }

    /// <summary>
    /// Helper method to reset the turnAround variable by was of a coroutine
    /// </summary>
    private void ResetTurnAround() {
        _turnAround = true;
    }

    /// <summary>
    /// Calculate the velocity
    /// </summary>
    private void CalculateVelocity() {
        float targetVelocityX = GroundPositionData.MoveSpeed * (FacingRight ? 1 : -1);
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref GroundPositionData.VelocityXSmoothing, Controller2d.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        ApplyGravity();
    }
}
