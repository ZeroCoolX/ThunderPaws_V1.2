using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Robot_GL2 : BaddieLifeform {

    /// <summary>
    /// Max time the baddie should stop foe
    /// </summary>
    private float _maxStopSeconds = 2f;

    /// <summary>
    /// Random number of value to this means we should stop breifly
    /// </summary>
    private float _shotDelay = 2f;

    /// <summary>
    /// Random number of value to this means we should fire
    /// </summary>
    private float _timeSinceLastFire;

    /// <summary>
    /// How far out the baddie searches to see if it's on the same horizontal plane as the baddie
    /// This indicates it should start shooting in the direction of the target
    /// </summary>
    private float _visionRaylength = 20f;

    public void Start() {
        base.Start();

        Animator = transform.GetComponent<Animator>();
        if(Animator == null) {
            throw new MissingComponentException("There is no animator on this baddie");
        }

        Gravity = -25.08f;
        Health = 15;

        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - Target.position.x;
        CalculateFacingDirection(directionToTarget);
    }

    public void Update() {
        base.Update();

        if (!CheckTargetsExist()) {
            return;
        }

        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - Target.position.x;
        // Check if we can shoot at the target
        CheckForHorizontalEquality(directionToTarget);
    }

    private void CheckForHorizontalEquality(float dirToTarget) {
        var targetLayer = 1 << 8;

        Debug.DrawRay(ProjectileData.FirePoint.position, (FacingRight ? Vector2.right : Vector2.left) * _visionRaylength, Color.red);

        RaycastHit2D horizontalCheck = Physics2D.Raycast(ProjectileData.FirePoint.position, FacingRight ? Vector2.right : Vector2.left, _visionRaylength, targetLayer);

        if (horizontalCheck.collider != null && Time.time > _timeSinceLastFire) {
            print("Hit!");
            // Shoot a projectile towards the target in 1 second
            _timeSinceLastFire = Time.time + _shotDelay;
            Velocity.x = 0f;
            Animator.SetBool("ChargeAndFire", true);
            Invoke("Fire", 0.5f);
        }
    }

    private void Fire() {
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
        Animator.SetBool("ChargeAndFire", false);
    }
}
