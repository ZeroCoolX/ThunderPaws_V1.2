using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot_GL2 : DamageableLifeform {

    /// <summary>
    /// Max time the baddie should stop foe
    /// </summary>
    private float _maxStopSeconds = 2f;
    /// <summary>
    /// Keeps a record of how long we've been stopped
    /// </summary>
    private float _timeStopped = 0f;

    /// <summary>
    /// Random number of value to this means we should stop breifly
    /// </summary>
    private float _shotDelay = 4f;

    /// <summary>
    /// Random number of value to this means we should fire
    /// </summary>
    private float _timeSinceLastFire;

    /// <summary>
    /// Who the baddie is focused on attacking
    /// </summary>
    private Transform _target;

    /// <summary>
    /// Prefab for what this baddie shoots
    /// </summary>
    public Transform BulletPrefab;

    /// <summary>
    /// Reference to where we should spawn the bullet from
    /// </summary>
    private Transform _firePoint;

    /// <summary>
    /// Indicates which direction we're facing
    /// </summary>
    private bool _facingRight;

    /// <summary>
    /// Indicates what the bullet should hit
    /// </summary>
    private LayerMask _whatToHit;

    /// <summary>
    /// Reference to the animator
    /// </summary>
    private Animator _animator;


    /// <summary>
    /// How far out the baddie searches to see if it's on the same horizontal plane as the baddie
    /// This indicates it should start shooting in the direction of the target
    /// </summary>
    private float _visionRaylength = 20f;

    public void Start() {
        var playerLayer = 1 << 8;
        var obstacleLayer = 1 << 10;
        _whatToHit = playerLayer | obstacleLayer;
        //Phsyics controller used for all collision detection
        Controller = transform.GetComponent<CollisionController2D>();
        if (Controller == null) {
            throw new MissingComponentException("There is no CollisionController2D on this object");
        }

        // Find the player and store the target reference
        FindPlayer();

        _animator = transform.GetComponent<Animator>();
        if(_animator == null) {
            throw new MissingComponentException("There is no animator on this baddie");
        }

        _firePoint = transform.Find(GameConstants.ObjectName_FirePoint);
        if (_firePoint == null) {
            Debug.LogError("AbstractWeapon.cs: No firePoint found");
            throw new UnassignedReferenceException();
        }

        Gravity = -25.08f;
        Health = 15;

        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - _target.position.x;
        CalcualteFacingDirection(directionToTarget);
    }

    public void Update() {
        base.Update();
        if (_target == null) {
            FindPlayer();
            return;
        }
        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - _target.position.x;
        // Check if we can shoot at the target
        CheckForHorizontalEquality(directionToTarget);
    }

    private void FindPlayer() {
        // Find the player and store the target reference
        GameObject target = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);
        if (target != null) {
            _target = target.transform;
        }
    }

    private void CheckForHorizontalEquality(float dirToTarget) {
        var targetLayer = 1 << 8;

        Debug.DrawRay(_firePoint.position, (_facingRight ? Vector2.right : Vector2.left) * _visionRaylength, Color.red);

        RaycastHit2D horizontalCheck = Physics2D.Raycast(_firePoint.position, _facingRight ? Vector2.right : Vector2.left, _visionRaylength, targetLayer);

        if (horizontalCheck.collider != null && Time.time > _timeSinceLastFire) {
            print("Hit!");
            // Has a chance to fire a bullet
            _timeStopped = Time.time + _maxStopSeconds;
            // Shoot a projectile towards the target in 1 second
            _timeSinceLastFire = Time.time + _shotDelay;
            Velocity.x = 0f;
            _animator.SetBool("ChargeAndFire", true);
            Invoke("Fire", 1f);
        }
    }

    private void Fire() {
        print("Fire!");
        Transform clone = Instantiate(BulletPrefab, _firePoint.position, _firePoint.rotation) as Transform;
        //Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
        AbstractProjectile projectile = clone.GetComponent<BulletProjectile>();

        //Set layermask of parent (either player or baddie)
        projectile.SetLayerMask(_whatToHit);
        projectile.Damage = 5;
        projectile.MoveSpeed = 15;
        projectile.MaxLifetime = 10;
        projectile.Fire((_facingRight ? Vector2.right : Vector2.left), Vector2.up);
        _animator.SetBool("ChargeAndFire", false);
    }



    /// <summary>
    /// Mirror the player graphics by inverting the .x local scale value
    /// </summary>
    private void CalcualteFacingDirection(float dirToTarget) {
        if (dirToTarget == 0 || Mathf.Sign(transform.localScale.x) == Mathf.Sign(dirToTarget)) { return; }

        // Switch the way the player is labelled as facing.
        _facingRight = Mathf.Sign(dirToTarget) <= 0;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
