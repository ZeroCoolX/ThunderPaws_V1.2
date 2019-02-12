using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mausubot_GL3 : GroundBaddieLifeform {
    private const float GRAVITY = -25.08f;
    private const int MOVE_SPEED = 7;
    private const float VISION_LENGTH = 10;
    private const float EXPLODE_RANGE = 1f;
    private const float MAX_STOP_SECONDS = 0.25f;

    private bool _awakened = false;

    void Start () {
        base.Start();

        GroundPositionData.MoveSpeed = MOVE_SPEED;
        VisionRayLength = VISION_LENGTH;
        GroundPositionData.MaxStopSeconds = MAX_STOP_SECONDS;
        Gravity = GRAVITY;
    }

    void Update () {
        base.Update();

        if (!CheckTargetsExist()) {
            return;
        }

        if (!_awakened) {
            print("Haven't seen target yet");
            CheckForTargetInRange();
        } else {
            print("ACTIVATED");
            ExplodeIfNear();

            var directionToTarget = transform.position.x - Target.position.x;
            CalculateFacingDirection(directionToTarget);

            CalculateVelocity();

            Move();
        }
    }

    private void OnDrawGizmos() {
        Debug.DrawRay(transform.position, Vector2.right * (FacingRight ? 1 : -1) * VisionRayLength, Color.red);
        Debug.DrawRay(transform.position, Vector2.right * (FacingRight ? 1 : -1) * EXPLODE_RANGE, Color.green);
    }

    private void ExplodeIfNear() {
        RaycastHit2D ray = Physics2D.Raycast(transform.position, Vector2.right * (FacingRight ? 1 : -1), EXPLODE_RANGE, 1 << GameConstants.Layer_Player);
        if(ray.collider != null) {
            StartCoroutine(Explode());
        }
    }

    private IEnumerator Explode() {
        yield return new WaitForSeconds(MAX_STOP_SECONDS);
        print("Explode");
        Damage(999);
    }

    private void CheckForTargetInRange() {
        RaycastHit2D raycast = Physics2D.Raycast(transform.position, Vector2.left, VisionRayLength, 1 << GameConstants.Layer_Player);
        if(raycast.collider == null) {
            raycast = Physics2D.Raycast(transform.position, Vector2.right, VisionRayLength, 1 << GameConstants.Layer_Player);
        }
        if(raycast.collider != null) {
            _awakened = true;
        }
    }

    private void CalculateVelocity() {
        float targetVelocityX = GroundPositionData.MoveSpeed * (FacingRight ? 1 : -1);
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref GroundPositionData.VelocityXSmoothing, Controller2d.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        ApplyGravity();
    }
}
