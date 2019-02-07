using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Robot_FL2 : FlyingBaddieLifeform {
    /// <summary>
    /// Each Flying baddie has their own implementation of the ActionData struct
    /// FL2 Specifc data.
    /// Encapsultes all the data needed for actions like attacking, and moving both vertical and horizontal
    /// </summary>
    private struct ActionData {
        public float HorizontalMoveSpeed;
        public float TimeToFire;
        public float MoveDuration;
    }
    private ActionData _actionData;

    private float _timeToCalculateNewBounds;
    private const float MIN_DISTANCE_FROM_TARGET = 3f;
    private const float MAX_DISTANCE_FROM_TARGET = 8f;
    private const float RECALCULATE_BOUNDS_DELAY = 2f;
    /// <summary>
    /// References to where to fire the raycast angles
    /// -45degree down, 90degree down, 45degree down
    /// </summary>
    private readonly Vector2[] _raycastAngles = new Vector2[] {
        new Vector2(-1, -1),
        Vector2.down,
        new Vector2(1, -1)
    };

    private const int PLAYER_LAYER = 8;



    private void Start() {
        base.Start();

        if (Target != null) {
            CalculateBounds(MIN_DISTANCE_FROM_TARGET, MAX_DISTANCE_FROM_TARGET);
        }

        FlyingPositionData.MoveSpeed = 3.5f;

        FlyingPositionData.TargetYDirection = ChooseRandomHeight();

        _actionData.TimeToFire = Time.time + 1f;
        _timeToCalculateNewBounds = Time.time + RECALCULATE_BOUNDS_DELAY;
    }

    /// <summary>
    /// Should just NOT be within a 45 degree angle nor 90 degrees above player.
    /// If you're not in one of these stay still.
    /// If you are - "flip a coin" to see if you move in the direction we're facing or backwards.
    /// ALWAYS face the player no matter what
    /// </summary>
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

        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - Target.position.x;
        CalculateFacingDirection(directionToTarget);

        if (Beserk) {
            SuicideDiveTarget();
            return;
        }

        // Collect distance from this to target
        var rayLength = Vector2.Distance(transform.position, Target.position);
        Debug.DrawRay(transform.position, (Target.position - transform.position), Color.red);

        // If we need to be moving do that instead of checking sightline
        // This ensures the baddie is elusive by valuing movement over firing
        if (_actionData.MoveDuration > Time.time || !StillAbovePlayerCheck()) {
            CalculateVelocity();
        }else {
            Velocity.x = 0;
            Velocity.y = 0f;
            CalculateAngleCollisions(rayLength);
        }

        Move();
        CalculateFire();
    }

    /// <summary>
    /// Determine if the baddie should fire based off the distance from this to the target.
    /// Fire mode is a 3 round burst
    /// </summary>
    private void CalculateFire() {
        if(Time.time > _actionData.TimeToFire && Vector2.Distance(transform.position, Target.position) <= GameConstants.Data_SightDistance) {
            // Wait 0.05 seconds in between each shot
            _actionData.TimeToFire = Time.time + 3f;
            Invoke("InitiateAttack", 0.1f);
            Invoke("Fire", 0.1f);
            Invoke("Fire", 0.25f);
            Invoke("Fire", 0.4f);
            Invoke("ResetAttack", 0.5f);
        }
    }

    private void Fire() {
        try {
            Transform clone = Instantiate(BulletPrefab, ProjectileData.FirePoint.position, ProjectileData.FirePoint.rotation) as Transform;
            // Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
            AbstractProjectile projectile = clone.GetComponent<BulletProjectile>();

            // Set layermask of parent (either player or baddie)
            projectile.SetLayerMask(ProjectileData.WhatToHit);
            projectile.Damage = BulletDamage;//5;
            projectile.MoveSpeed = BulletSpeed;//10;
            projectile.MaxLifetime = 10;
            projectile.Fire(Target.position - transform.position, Vector2.up);
        }catch(System.Exception e) {
            print("Caught exception for delayed firing because the player died literally inbetween 0.1 and 0.15 seconds. Catch and move on");
        }
    }

    private bool StillAbovePlayerCheck() {
        return Mathf.Sign(transform.position.y - Target.position.y) > 0;
    }

    private void CalculateVelocity() {
        Velocity.x = Mathf.SmoothDamp(Velocity.x, _actionData.HorizontalMoveSpeed, ref FlyingPositionData.VelocityXSmoothing, 0.2f);
        CalculateVerticalThreshold();
        Velocity.y = Mathf.SmoothDamp(Velocity.y, FlyingPositionData.TargetYDirection * Random.Range(1.5f, FlyingPositionData.MoveSpeed), ref FlyingPositionData.VelocityYSmoothing, 1f);
    }

    /// <summary>
    /// Determine where to move if the target is within the angled sightlines.
    /// Angled sightlines : +-45 degrees and directly under.
    /// </summary>
    private void CalculateAngleCollisions(float rayLength) {
        var targetLayer = 1 << PLAYER_LAYER;
        foreach (var angle in _raycastAngles) {
            Debug.DrawRay(transform.position, angle * rayLength, Color.green);
            RaycastHit2D collisionCheck = Physics2D.Raycast(transform.position, angle, rayLength, targetLayer);
            // If we are NOT within +- 45 degrees or right above we should attempt to get there
            if (collisionCheck.collider == null) {
                TrackTarget();
            }
            // If we are within +- 45 degrees or right above, evade
            //if(collisionCheck.collider != null) {
            //    EvadeTarget();
            //}
        }
    }

    /// <summary>
    /// We are either directly above or within the 45degree angle of the player and should move!
    /// </summary>
    private void EvadeTarget() {
        _actionData.MoveDuration = Time.time + (Random.Range(1, 4));

        // pos = we are on players right
        // neg = we are on players left
        var rf1 = ((Random.Range(2, 11) % 2 == 0) ? -1 : 1);
        var rf2 = Mathf.Sign(transform.position.x - Target.position.x);
        var rf3 = 0f;

        if (rf1 < 0 && rf2 < 0) {
            // If we should move left, and are already left of player we should have a 75% change of moving right
            // 25% chance to keep moving left
            rf3 = ((Random.Range(2, 11) % 6 == 0) ? -1 : 1);
        } else if (rf1 > 0 && rf2 > 0) {
            // If we should move right, and are already right of player we should have a 75% change of moving left
            // 25% chance to keep moving right
            rf3 = ((Random.Range(2, 11) % 6 == 0) ? 1 : -1);
        } else {
            // Otherwise, we're on the opposite side of the player from where we're about to move so do that
            rf3 = rf1;
        }

        _actionData.HorizontalMoveSpeed = FlyingPositionData.MoveSpeed * rf3;
        CalculateVerticalThreshold();
    }

    private void TrackTarget() {
        _actionData.MoveDuration = Time.time + (Random.Range(1, 4));

        // pos = baddie is on players right
        // neg = baddie is on players left
        var directionFromTarget = Mathf.Sign(transform.position.x - Target.position.x);
        print("baddie is on side : " + directionFromTarget);
        var horizontalMoveDirection = 1;

        if (directionFromTarget > 0) {
            horizontalMoveDirection = -1;
        }

        _actionData.HorizontalMoveSpeed = FlyingPositionData.MoveSpeed * horizontalMoveDirection;
        CalculateVerticalThreshold();
    }
}
