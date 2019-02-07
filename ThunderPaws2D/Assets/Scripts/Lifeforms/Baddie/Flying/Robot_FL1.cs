using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Robot_FL1 : FlyingBaddieLifeform {
    /// <summary>
    /// Each Flying baddie has their own implementation of the ActionData struct
    /// FL1 Specifc data.
    /// Encapsultes all the data needed for actions like attacking, moving, bounds..etc
    /// </summary>
    private struct ActionData {
        public bool OverPlayer, BombDropInitiated, RecalculateBounds;
        public float TimeToFindNewSpeed;
    }
    private ActionData _actionData;
    private float _timeToCalculateNewBounds;

    private const float MOVE_SPEED = 2f;
    private const float RECALCULATE_BOUNDS_DELAY = 2f;

    private new void Awake() {
        base.Awake();
        _actionData.BombDropInitiated = _actionData.OverPlayer = _actionData.RecalculateBounds = false;
        MIN_DISTANCE_FROM_TARGET = 3f;
        MAX_DISTANCE_FROM_TARGET = 7f;
    }

    protected new void Start() {
        base.Start();
        if(Target != null) {
            CalculateBounds(MIN_DISTANCE_FROM_TARGET, MAX_DISTANCE_FROM_TARGET);
        }

        FlyingPositionData.MoveSpeed = MOVE_SPEED;

        FlyingPositionData.TargetYDirection = ChooseRandomHeight();
        _timeToCalculateNewBounds = Time.time + RECALCULATE_BOUNDS_DELAY;
    }

    private void Update() {
        base.Update();

        if (!CheckTargetsExist()) {
            return;
        }

        if (Time.time > _timeToCalculateNewBounds) {
            CalculateBounds(MIN_DISTANCE_FROM_TARGET, MAX_DISTANCE_FROM_TARGET);
            _timeToCalculateNewBounds = Time.time + RECALCULATE_BOUNDS_DELAY;
        }


        MaxBoundsCheck();

        // Collect distance from this to target
        var rayLength = Vector2.Distance(transform.position, Target.position);
        Debug.DrawRay(transform.position, Vector2.down * rayLength, Color.red);

        TrackTargetIfNecessary();
    }

    private void TrackTargetIfNecessary() {
        // Determine if we need to track the target
        if (!OverPlayer() && !_actionData.BombDropInitiated) {
            // Find out where the target is in reference to this.
            var directionToTarget = transform.position.x - Target.position.x;
            CalculateFacingDirection(directionToTarget);
            CalculateVelocity();
            Move();
        }
    }

    /// <summary>
    /// Determines if we're over the player and schedule a bomb drop to occur
    /// </summary>
    private bool OverPlayer() {
        _actionData.OverPlayer = Mathf.Abs(transform.position.x - Target.position.x) < GameConstants.Data_VerticalPrecision;
        if (_actionData.OverPlayer && !_actionData.BombDropInitiated) {
            Invoke("DropBomb", 0.1f);
            _actionData.BombDropInitiated = true;
        }
        return _actionData.OverPlayer;
    }

    /// <summary>
    /// Used to delay bomb dropping if continuously over the player so its not just a constant stream of bombs.
    /// </summary>
    private void ResetBombDrop() {
        _actionData.BombDropInitiated = false;
    }

    private void DropBomb() {
        CheckForOverPlayerShortCircuit();

        InitiateAttack();

        Transform clone = Instantiate(BulletPrefab, ProjectileData.FirePoint.position, ProjectileData.FirePoint.rotation) as Transform;
        // Parent the bomb to who shot it so we know what to hit (parents LayerMask whatToHit)
        AbstractProjectile projectile = clone.GetComponent<BulletProjectile>();

        // Set layermask of parent (either player or baddie)
        projectile.SetLayerMask(ProjectileData.WhatToHit);
        projectile.Damage = 10;
        projectile.MoveSpeed = 10;
        projectile.MaxLifetime = 10;
        projectile.Fire(Vector2.down, (FacingRight ? Vector2.right : Vector2.left));

        Invoke("ResetBombDrop", 0.5f);
        Invoke("ResetAttack", 0.5f);
    }

    private void CheckForOverPlayerShortCircuit() {
        if (!_actionData.OverPlayer) {
            Invoke("ResetBombDrop", 0f);
            return;
        }
    }

    private void CalculateVelocity() {
        float targetVelocityX = FlyingPositionData.MoveSpeed * (FacingRight ? 1 : -1);
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref FlyingPositionData.VelocityXSmoothing, 0.2f);
        if (Time.time > _actionData.TimeToFindNewSpeed) {
            _actionData.TimeToFindNewSpeed = Time.time + Random.Range(1f, 4f);
            CalculateVerticalThreshold();
        }
        Velocity.y = Mathf.SmoothDamp(Velocity.y, FlyingPositionData.TargetYDirection * FlyingPositionData.MoveSpeed, ref FlyingPositionData.VelocityYSmoothing, 1f);
    }
}
