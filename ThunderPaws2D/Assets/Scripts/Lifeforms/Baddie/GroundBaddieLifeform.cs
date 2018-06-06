using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundBaddieLifeform : BaddieLifeform {
    /// <summary>
    /// Specific struct for Ground baddie types
    /// </summary>
    protected GroundPositionModel GroundPositionData;

    /// <summary>
    /// Delay used during Mathf.SmoothDamp for dampening movenents
    /// Things suspended in the air should move horizontal slower
    /// </summary>
    /// 
    protected float AccelerationTimeAirborne = 0.2f;
    /// <summary>
    /// Delay used during Mathf.SmoothDamp for dampening movenents
    /// Things on the ground should move horizontal faster
    /// </summary>
    protected float AccelerationTimeGrounded = 0.1f;

    /// <summary>
    /// How far out the baddie's sightline is
    /// </summary>
    protected float VisionRayLength;

    /// <summary>
    /// Name of the attack animation we should play for an attack.
    /// Its optional as not every baddie has this at the moment
    /// </summary>
    protected string OptionalAttackAnimation;

    /// <summary>
    /// Determine if we are on the same horizontal plane as the Target.
    /// This ensures we can only see the Target if they're on our level
    /// and the baddies aren't trying to shoot at nothing if the Target
    /// is above them for example
    /// </summary>
    /// <param name="fireDelay"></param>
    /// <param name="collisionFunc"></param>
    protected void HandleCollision() {
        if (Time.time > GroundPositionData.TimeSinceLastFire) {
            print("Hit!");
            // Update the amount of time stopped to ensure we wait if necessary
            // Not all baddies use this - but setting it won't effect those who don't care
            GroundPositionData.TimeStopped = Time.time + GroundPositionData.MaxStopSeconds;
            // Shoot a projectile towards the target in 1 second
            GroundPositionData.TimeSinceLastFire = Time.time + GroundPositionData.ShotDelay;
            Velocity.x = 0f;
            if (Animator != null) {
                try {
                    // Play the attack animation
                    Animator.SetBool(OptionalAttackAnimation, true);
                }catch(Exception e) {
                    print("Failed assigning animation value to true : " + OptionalAttackAnimation);
                }
            }
            Invoke("Fire", GroundPositionData.FireDelay);
        }
    }

    /// <summary>
    /// Fire a raycast forward
    /// </summary>
    /// <returns></returns>
    protected RaycastHit2D FireRaycast() {
        // Specify the Player layer as the target (8)
        var targetLayer = 1 << 8;
        // Just useful for debugging
        Debug.DrawRay(ProjectileData.FirePoint.position, (FacingRight ? Vector2.right : Vector2.left) * VisionRayLength, Color.red);
        // Fire raycast for collision check
        return Physics2D.Raycast(ProjectileData.FirePoint.position, FacingRight ? Vector2.right : Vector2.left, VisionRayLength, targetLayer);
    }

    /// <summary>
    /// Generate attack in a forward facing motion 
    /// </summary>
    protected void Fire() {
        print("Fire!");
        Transform clone = Instantiate(BulletPrefab, ProjectileData.FirePoint.position, ProjectileData.FirePoint.rotation) as Transform;
        //Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
        AbstractProjectile projectile = clone.GetComponent<BulletProjectile>();

        //Set layermask of parent (either player or baddie)
        projectile.SetLayerMask(ProjectileData.WhatToHit);
        projectile.Damage = 5;
        projectile.MoveSpeed = 15;
        projectile.MaxLifetime = 10;
        projectile.Fire((FacingRight ? Vector2.right : Vector2.left), Vector2.up);
        if (Animator != null) {
            try {
                // After the fire has occurred stop the attack animation
                Animator.SetBool(OptionalAttackAnimation, false);
            } catch (Exception e) {
                print("Failed assigning animation value to false : " + OptionalAttackAnimation);
            }
        }
    }

    /// <summary>
    /// Struct holding data necessary to ground baddie types
    /// </summary>
    protected struct GroundPositionModel {
        /// <summary>
        /// How long we should take between shots
        /// </summary>
        public float ShotDelay;
        /// <summary>
        /// How long it takess to fire a shot from collision contact
        /// </summary>
        public float FireDelay;
        /// <summary>
        /// Random number of value to this means we should fire
        /// </summary>
        public float TimeSinceLastFire;
        /// <summary>
        /// Keeps track of how long a baddie has been stopped
        /// </summary>
        public float TimeStopped;
        /// <summary>
        /// Max number of seconds the baddie is allowed to be stopped
        /// </summary>
        public float MaxStopSeconds;
        /// <summary>
        /// How fast the baddie moves
        /// </summary>
        public int MoveSpeed;
        /// <summary>
        /// Stores the direction we are moving
        /// </summary>
        public Vector2 MoveDirection;
        /// <summary>
        /// Needed for Mathf.SmoothDamp function when flying
        /// </summary>
        public float VelocityXSmoothing;
    }
}
