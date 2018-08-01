using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Robot_GL1 : GroundBaddieLifeform {

    [Header("Ledge Bound Properties")]

    public bool LedgeBound = false;
    private bool _turnAround = true;
    /// <summary>
    /// Used only by ledge bound baddies.
    /// Indicates we have seen the target and should halt movement
    /// This allows a delay even after the target is out of collision
    /// sight contact so it looks like the baddie "knows" the target
    /// was just here and might return - before returning to wandering
    /// </summary>
    private float _alertTimeThreshold = -1f;

    private const float GRAVITY = -25.08f;
    private const float HEALTH = 5f;
    private const float SHOT_DELAY = 3f;
    private const int MOVE_SPEED = 5;
    private const float VISION_LENGTH = 10f;
    private const float MAX_STOP_SECONDS = 2f;



    public void Start() {
        base.Start();

        // Set baddie specific data
        GroundPositionData.ShotDelay = SHOT_DELAY;
        GroundPositionData.MoveSpeed = MOVE_SPEED;
        GroundPositionData.MoveDirection = Vector2.left;
        GroundPositionData.MaxStopSeconds = MAX_STOP_SECONDS;
        GroundPositionData.FireDelay = 1f;
        VisionRayLength = VISION_LENGTH;
        Gravity = GRAVITY;
        Health = HEALTH;
    }

    public void Update() {
        base.Update();

        if (!CheckTargetsExist()) {
            return;
        }

        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - Target.position.x;

        // Check if we can shoot at the target
        var hCollider = FireRaycast();
        if (hCollider.collider != null) {
            _alertTimeThreshold = LedgeBound ? Time.time + 1f : 0f;
            HandleCollision();
        }

        if (LedgeBound) {
            // We previously saw the target but no longer see him.
            // Wait the max amount of time until going back to wandering
            if (_alertTimeThreshold >= Time.time) {
                Velocity = Vector2.zero;
                return;
            }

            HandleLedgeboundMovement();

        } else {
            // Face that direction
            CalculateFacingDirection(directionToTarget);

            // Move in that direction
            if (Time.time > GroundPositionData.TimeStopped) {
                CalculateVelocity();
            }
        }

        Move();
    }

    private void HandleLedgeboundMovement() {
        CalculateFacingDirection(GroundPositionData.MoveDirection.x * -1);

        // Handle ledges
        if (Controller2d.Collisions.NearLedge && _turnAround) {
            GroundPositionData.MoveDirection.x = Vector2.right.x * (FacingRight ? -1f : 1f);
            _turnAround = false;
            Invoke("ResetTurnAround", 0.5f);
        }
        float targetVelocityX = GroundPositionData.MoveSpeed * GroundPositionData.MoveDirection.x;
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref GroundPositionData.VelocityXSmoothing, Controller2d.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
    }

    private void ResetTurnAround() {
        _turnAround = true;
    }

    private void CalculateVelocity() {
        float targetVelocityX = GroundPositionData.MoveSpeed * (FacingRight ? 1 : -1);
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref GroundPositionData.VelocityXSmoothing, Controller2d.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        ApplyGravity();
    }
}
