using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Robot_FL1 : FlyingBaddieLifeform {
    /// <summary>
    /// Struct that holds all data necessary to the actions this baddie can perform.
    /// Over a player, dropping a bomb, waiting to attack, should recalculate bounds
    /// </summary>
    private ActionData _actionData;

    private new void Awake() {
        base.Awake();
        // Zero out all ActionData properties
        _actionData.BombDropInitiated = _actionData.OverPlayer = _actionData.RecalculateBounds = false;
    }

    /// <summary>
    /// Assign Layermasks for collision.
    /// Find players and assign target.
    /// Initiate movement bounds.
    /// </summary>
    protected new void Start() {
        base.Start();

        CalculateBounds();

        FlyingPositionData.MoveSpeed = 2f;

        // TODO: this is bad - half the time TargetY is treated like an actual
        // coordinate point in space, and the other half its treated strictly 
        // as a -1 or 1 vertical direction indicator
        FlyingPositionData.TargetYDirection = ChooseRandomHeight();
    }

    /// <summary>
    /// Track the target.
    /// Locate its position.
    /// Move towards it horizontally until the x coordinates are within (some) range and stop
    /// </summary>
    private void Update() {
        base.Update();

        if (!CheckTargetsExist()) {
            return;
        }

        // Every 2 seconds recalcualte the min and max just in case the playewr is in a much different spot vertically than before
        // Right now RecalculateBounds is always false...
        if (_actionData.RecalculateBounds) {
            _actionData.RecalculateBounds = false;
            Invoke("CalculateBounds", 2f);
        }

        // Ensure we're within bounds
        MaxBoundsCheck();

        // Collect distance from this to target
        var rayLength = Vector2.Distance(transform.position, Target.position);
        Debug.DrawRay(transform.position, Vector2.down * rayLength, Color.red);

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
    /// <returns></returns>
    private bool OverPlayer() {
        _actionData.OverPlayer = Mathf.Abs(transform.position.x - Target.position.x) < GameConstants.Data_VerticalPrecision;
        if (_actionData.OverPlayer && !_actionData.BombDropInitiated) {
            Invoke("DropBomb", 0.1f);
            _actionData.BombDropInitiated = true;
        }
        return _actionData.OverPlayer;
    }

    /// <summary>
    /// Used to delay bomb dropping if continuously over the player
    /// so its not just a constant stream of bombs.
    /// </summary>
    private void ResetBombDrop() {
        _actionData.BombDropInitiated = false;
    }

    /// <summary>
    /// Create a bomb prefab and drop it straight down.
    /// Schedule a reset of the bomb drop variable
    /// </summary>
    private void DropBomb() {
        // Shortcircuit if we're no longer over the player
        if (!_actionData.OverPlayer) {
            Invoke("ResetBombDrop", 0f);
            return;
        }

        // Create bomb
        Transform clone = Instantiate(BulletPrefab, ProjectileData.FirePoint.position, ProjectileData.FirePoint.rotation) as Transform;
        // Parent the bomb to who shot it so we know what to hit (parents LayerMask whatToHit)
        AbstractProjectile projectile = clone.GetComponent<BulletProjectile>();

        // Set layermask of parent (either player or baddie)
        projectile.SetLayerMask(ProjectileData.WhatToHit);
        projectile.Damage = 10;
        projectile.MoveSpeed = 10;
        projectile.MaxLifetime = 10;
        projectile.Fire(Vector2.down, (FacingRight ? Vector2.right : Vector2.left));
        // Make sure we don't drop another bomb till at LEAST 2 seconds
        Invoke("ResetBombDrop", 0.5f);
    }

    /// <summary>
    /// Calculate the velocity based off where the player is and our random vertical values
    /// </summary>
    private void CalculateVelocity() {
        float targetVelocityX = FlyingPositionData.MoveSpeed * (FacingRight ? 1 : -1);
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref FlyingPositionData.VelocityXSmoothing, 0.2f);
        if (Time.time > _actionData.TimeToFindNewSpeed) {
            // random time between 1 and 3 seconds!
            _actionData.TimeToFindNewSpeed = Time.time + Random.Range(1f, 4f);
            CalculateVerticalThreshold();
        }
        Velocity.y = Mathf.SmoothDamp(Velocity.y, FlyingPositionData.TargetYDirection * FlyingPositionData.MoveSpeed, ref FlyingPositionData.VelocityYSmoothing, 1f);
    }

    /// <summary>
    /// Encapsultes all the data needed for actions like attacking, moving, bounds..etc
    /// </summary>
    private struct ActionData {
        public bool OverPlayer, BombDropInitiated, RecalculateBounds;
        public float TimeToFindNewSpeed;
    }
}
