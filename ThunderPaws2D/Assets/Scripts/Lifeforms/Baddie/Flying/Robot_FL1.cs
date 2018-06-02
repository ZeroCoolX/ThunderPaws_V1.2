using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Robot_FL1 : FlyingBaddieLifeform {
    /// <summary>
    /// Need a reference to how close in the x direction we're trying to get
    /// </summary>
    private float _overThreshold = 0.1f;
    /// <summary>
    /// Trigger to indicate we're over the player 
    /// </summary>
    private bool _overPlayer = false;
    /// <summary>
    /// Indicates that a bomb is being dropped - so don't drop anymore until we're through
    /// </summary>
    private bool _bombDropInitiated = false;

    private float _heightAbovePlayer;

    private bool moveToNewY = false;
    //private float targetY;

    private bool RecalculateBounds = false;

    private float _timeToFindNewSpeed;

    //public bool IsHorde2Hack = false;

    /// <summary>
    /// Find the player and begin tracking
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
        if (RecalculateBounds) {
            RecalculateBounds = false;
            Invoke("CalculateBounds", 2f);
        }

        // always check for bounds
        MaxBoundsCheck();

        var rayLength = Vector2.Distance(transform.position, Target.position);
        Debug.DrawRay(transform.position, Vector2.down * rayLength, Color.red);

        if (!OverPlayer() && !_bombDropInitiated) {
            // Find out where the target is in reference to this.
            var directionToTarget = transform.position.x - Target.position.x;
            CalculateFacingDirection(directionToTarget);

            CalculateVelocity();
            Move();
        }
    }

    private bool OverPlayer() {
        _overPlayer = Mathf.Abs(transform.position.x - Target.position.x) < _overThreshold;
        if (_overPlayer && !_bombDropInitiated) {
            // Wait 0.25seconds then drop bomb.
            Invoke("DropBomb", 0.1f);
            _bombDropInitiated = true;
        }
        return _overPlayer;
    }

    private void ResetBombDrop() {
        _bombDropInitiated = false;
    }

    private void DropBomb() {
        if (!_overPlayer) {
            Invoke("ResetBombDrop", 0f);
            return;
        }
        print("Fire!");
        Transform clone = Instantiate(BulletPrefab, ProjectileData.FirePoint.position, ProjectileData.FirePoint.rotation) as Transform;
        //Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
        AbstractProjectile projectile = clone.GetComponent<BulletProjectile>();

        //Set layermask of parent (either player or baddie)
        projectile.SetLayerMask(ProjectileData.WhatToHit);
        projectile.Damage = 10;
        projectile.MoveSpeed = 10;
        projectile.MaxLifetime = 10;
        projectile.Fire(Vector2.down, (FacingRight ? Vector2.right : Vector2.left));
        // Make sure we don't drop another bomb till at LEAST 2 seconds
        Invoke("ResetBombDrop", 0.5f);
    }

    /// <summary>
    /// Calculate the velocity
    /// </summary>
    private void CalculateVelocity() {
        float targetVelocityX = FlyingPositionData.MoveSpeed * (FacingRight ? 1 : -1);
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref FlyingPositionData.VelocityXSmoothing, 0.2f);
        if (Time.time > _timeToFindNewSpeed) {
            // random time between 1 and 3 seconds!
            _timeToFindNewSpeed = Time.time + Random.Range(1f, 4f);
            CalculateVerticalThreshold();
        }
        Velocity.y = Mathf.SmoothDamp(Velocity.y, FlyingPositionData.TargetYDirection * FlyingPositionData.MoveSpeed, ref FlyingPositionData.VelocityYSmoothing, 1f);
    }
}
