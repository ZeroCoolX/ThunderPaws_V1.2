using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundBaddieLifeform : BaddieLifeform {

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
    protected float VisionRayLength;
    protected string OptionalAttackAnimation;

    private const int PLAYER_LAYER = 8;

    protected struct GroundPositionModel {
        public float ShotDelay;
        public float FireDelay;
        public float TimeSinceLastFire;
        public float TimeStopped;
        public float MaxStopSeconds;
        public int MoveSpeed;
        public Vector2 MoveDirection;
        public float VelocityXSmoothing;
    }
    protected GroundPositionModel GroundPositionData;



    /// <summary>
    /// Determine if we are on the same horizontal plane as the Target.
    /// This ensures we can only see the Target if they're on our level
    /// and the baddies aren't trying to shoot at nothing if the Target
    /// is above them for example
    /// </summary>
    protected void HaltAndFire() {
        if (Time.time > GroundPositionData.TimeSinceLastFire) {
            // Update the amount of time stopped to ensure we wait if necessary
            // Not all baddies use this - but setting it won't effect those who don't care
            GroundPositionData.TimeStopped = Time.time + GroundPositionData.MaxStopSeconds;
            GroundPositionData.TimeSinceLastFire = Time.time + GroundPositionData.ShotDelay;
            Velocity.x = 0f;
            if (Animator != null && !string.IsNullOrEmpty(OptionalAttackAnimation)) {
                try {
                    Animator.SetBool(OptionalAttackAnimation, true);
                }catch(Exception e) {
                    print("Failed assigning animation value : " + OptionalAttackAnimation);
                }
            }
            Invoke("Fire", GroundPositionData.FireDelay);
        }
    }

    protected RaycastHit2D FireRaycast() {
        // Specify the Player layer as the target (8)
        var targetLayer = 1 << PLAYER_LAYER;
        // Just useful for debugging
         Debug.DrawRay(ProjectileData.FirePoint.position, (FacingRight ? Vector2.right : Vector2.left) * VisionRayLength, Color.red);
        // Fire raycast for collision check
        return Physics2D.Raycast(ProjectileData.FirePoint.position, FacingRight ? Vector2.right : Vector2.left, VisionRayLength, targetLayer);
    }

    protected void Fire() {
        Transform clone = Instantiate(BulletPrefab, ProjectileData.FirePoint.position, ProjectileData.FirePoint.rotation) as Transform;
        // Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
        AbstractProjectile projectile = clone.GetComponent<BulletProjectile>();

        // Set layermask of parent (either player or baddie)
        projectile.SetLayerMask(ProjectileData.WhatToHit);
        projectile.Damage = 5;
        projectile.MoveSpeed = 15;
        projectile.MaxLifetime = 10;
        projectile.Fire((FacingRight ? Vector2.right : Vector2.left), Vector2.up);
        if (Animator != null && !string.IsNullOrEmpty(OptionalAttackAnimation)) {
            try {
                // After the fire has occurred stop the attack animation
                Animator.SetBool(OptionalAttackAnimation, false);
            } catch (Exception e) {
                print("Failed assigning animation value to false : " + OptionalAttackAnimation);
            }
        }
    }
}
