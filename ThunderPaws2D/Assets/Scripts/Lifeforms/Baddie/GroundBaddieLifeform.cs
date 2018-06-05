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

    protected void CheckForHorizontalEquality(float fireDelay) {
        CheckForHorizontalEquality(fireDelay, "", false);
    }

    protected void CheckForHorizontalEquality(float fireDelay, bool collisionFunc) {
        CheckForHorizontalEquality(fireDelay, "", false, collisionFunc);
    }

    protected void CheckForHorizontalEquality(float fireDelay, string fireAnim, bool animBool) {
        CheckForHorizontalEquality(fireDelay, fireAnim, animBool, true);
    }

    /// <summary>
    /// Determine if we are on the same horizontal plane as the Target.
    /// This ensures we can only see the Target if they're on our level
    /// and the baddies aren't trying to shoot at nothing if the Target
    /// is above them for example
    /// </summary>
    /// <param name="fireDelay"></param>
    /// <param name="optionalAnim"></param>
    protected void CheckForHorizontalEquality(float fireDelay, string optionalAnim, bool animBool, bool collisionFunc) {
        // Specify the Player layer as the target (8)
        var targetLayer = 1 << 8;
        // Just useful for debugging
        Debug.DrawRay(ProjectileData.FirePoint.position, (FacingRight ? Vector2.right : Vector2.left) * VisionRayLength, Color.red);
        // Fire raycast for collision check
        RaycastHit2D horizontalCheck = Physics2D.Raycast(ProjectileData.FirePoint.position, FacingRight ? Vector2.right : Vector2.left, VisionRayLength, targetLayer);

        if (horizontalCheck.collider != null && collisionFunc) {
            print("Hit!");
            // Shoot a projectile towards the target in 1 second
            GroundPositionData.TimeSinceLastFire = Time.time + GroundPositionData.ShotDelay;
            Velocity.x = 0f;
            if (!string.IsNullOrEmpty(optionalAnim)) {
                try {
                    Animator.SetBool(optionalAnim, animBool);
                }catch(Exception e) {
                    print("Failed assigning animation value to true : " + optionalAnim);
                }
            }
            Invoke("Fire", fireDelay);
        }
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
        try {
            if(Animator != null) {
                Animator.SetBool("ChargeAndFire", false);
            }
        } catch(Exception e) {

        }
    }

    /// <summary>
    /// Struct holding data necessary to ground baddie types
    /// </summary>
    protected struct GroundPositionModel {
        /// <summary>
        /// Random number of value to this means we should stop breifly
        /// </summary>
        public float ShotDelay;
        /// <summary>
        /// Random number of value to this means we should fire
        /// </summary>
        public float TimeSinceLastFire;
        /// <summary>
        /// How fast the baddie moves
        /// </summary>
        public int MoveSpeed;
        /// <summary>
        /// Needed for Mathf.SmoothDamp function when flying
        /// </summary>
        public float VelocityXSmoothing;
    }
}
