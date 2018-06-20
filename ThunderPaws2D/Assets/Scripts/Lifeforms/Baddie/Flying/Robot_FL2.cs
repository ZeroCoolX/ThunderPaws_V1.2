using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Robot_FL2 : FlyingBaddieLifeform {
    /// <summary>
    /// Struct that holds all data necessary to the actions this baddie can perform.
    /// Horizontal move speed, time between firing, and how long to move in certain directions
    /// </summary>
    private ActionData _actionData;
    /// <summary>
    /// References to wheree to fire the raycast angles
    /// -45degree down, 90degree down, 45degree down
    /// </summary>
    private readonly Vector2[] _raycastAngles = new Vector2[] { new Vector2(-1, -1), Vector2.down, new Vector2(1, -1) };


    /// <summary>
    /// Find the player and begin tracking
    /// </summary>
    private void Start() {
        base.Start();

        if (Target != null) {
            CalculateBounds(3f, 8f);
        }

        FlyingPositionData.MoveSpeed = 3.5f;

        // TODO: this is bad - half the time TargetY is treated like an actual
        // coordinate point in space, and the other half its treated strictly 
        // as a -1 or 1 vertical direction indicator
        FlyingPositionData.TargetYDirection = ChooseRandomHeight();

        _actionData.TimeToFire = Time.time + 1f;
    }

    /// <summary>
    /// Should just NOT be within a 45 degree angle nor 90 degrees above player.
    /// If you're not in one of these stay still.
    /// If you are - "flip a coin" to see if you move in the direction we're facing or backwards.
    /// ALWAYS face the player no matter what
    /// </summary>
    private void Update() {
        base.Update();

        // Make sure the target exists
        if (!CheckTargetsExist()) {
            return;
        }

        // Ensure we're within bounds
        MaxBoundsCheck();

        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - Target.position.x;
        // Face that direction
        CalculateFacingDirection(directionToTarget);

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
            Invoke("Fire", 0.1f);
            Invoke("Fire", 0.15f);
            Invoke("Fire", 0.2f);
        }
    }

    /// <summary>
    /// Create a bullet prefab and send set the appropriate
    /// properties so that it fires.
    /// </summary>
    private void Fire() {
        try {
            Transform clone = Instantiate(BulletPrefab, ProjectileData.FirePoint.position, ProjectileData.FirePoint.rotation) as Transform;
            //Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
            AbstractProjectile projectile = clone.GetComponent<BulletProjectile>();

            //Set layermask of parent (either player or baddie)
            projectile.SetLayerMask(ProjectileData.WhatToHit);
            projectile.Damage = 5;
            projectile.MoveSpeed = 10;
            projectile.MaxLifetime = 10;
            projectile.Fire(Target.position - transform.position, Vector2.up);
        }catch(System.Exception e) {
            print("Caught exception for delayed firing because the player died literally inbetween 0.1 and 0.15 seconds. Catch and move on");
        }
    }

    /// <summary>
    /// Helper method which returns true if the distance between the player and 
    /// targets .Y positional element > 0
    /// </summary>
    /// <returns></returns>
    private bool StillAbovePlayerCheck() {
        return Mathf.Sign(transform.position.y - Target.position.y) > 0;
    }

    /// <summary>
    /// Calculate velocity both horizontal and vertical
    /// </summary>
    private void CalculateVelocity() {
        Velocity.x = Mathf.SmoothDamp(Velocity.x, _actionData.HorizontalMoveSpeed, ref FlyingPositionData.VelocityXSmoothing, 0.2f);
        CalculateVerticalThreshold();
        Velocity.y = Mathf.SmoothDamp(Velocity.y, FlyingPositionData.TargetYDirection, ref FlyingPositionData.VelocityYSmoothing, 1f);
    }

    /// <summary>
    /// Determine where to move if the target is within the angled sightlines.
    /// Angled sightlines : +-45 degrees and directly under.
    /// </summary>
    /// <param name="rayLength">
    /// Distance from this to target
    /// </param>
    private void CalculateAngleCollisions(float rayLength) {
        var targetLayer = 1 << 8;
        foreach (var angle in _raycastAngles) {
            Debug.DrawRay(transform.position, angle * rayLength, Color.green);
            RaycastHit2D collisionCheck = Physics2D.Raycast(transform.position, angle, rayLength, targetLayer);
            if(collisionCheck.collider != null) {
                // We are either directly above or within the 45degree angle of the player and should move!

                // Right now move between 1 and 3 seconds
                _actionData.MoveDuration = Time.time + (Random.Range(1, 4));

                // -1 = move left 
                // 1 = move right
                var rf1 = ((Random.Range(2, 11) % 2 == 0) ? -1 : 1);

                // pos = we are on players right
                // neg = we are on players left
                var rf2 = Mathf.Sign(transform.position.x - Target.position.x);

                var rf3 = 0f;
                if (rf1 < 0 && rf2 < 0 ){
                    // If we should move left, and are already left of player we should have a 75% change of moving right
                    // 25% chance to keep moving left
                    rf3 = ((Random.Range(2, 11) % 6 == 0) ? -1 : 1);
                } else if(rf1 > 0 && rf2 > 0) {
                    // If we should move right, and are already right of player we should have a 75% change of moving left
                    // 25% chance to keep moving right
                    rf3 = ((Random.Range(2, 11) % 6 == 0) ? 1 : -1);
                }else {
                    // Otherwise, we're on the opposite side of the player from where we're about to move so do that
                    rf3 = rf1;
                }

                _actionData.HorizontalMoveSpeed = FlyingPositionData.MoveSpeed * rf3;
                CalculateVerticalThreshold();
            }
        }
    }

    /// <summary>
    /// Each Flying baddie has their own implementation of the ActionData struct
    /// FL2 Specifc data.
    /// Encapsultes all the data needed for actions like attacking, and moving both vertical and horizontal
    /// </summary>
    private struct ActionData {
        /// <summary>
        /// Stores the value of the horizontal movement this iteration of movement
        /// </summary>
        public float HorizontalMoveSpeed;
        /// <summary>
        /// Delay in between initiating Fire() attack
        /// </summary>
        public float TimeToFire;
        /// <summary>
        /// Indicates how long to move for
        /// </summary>
        public float MoveDuration;
    }
}
