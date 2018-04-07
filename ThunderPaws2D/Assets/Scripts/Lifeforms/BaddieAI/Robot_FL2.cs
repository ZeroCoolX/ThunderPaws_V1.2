using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot_FL2 : DamageableLifeform {
    /// <summary>
    /// What the baddie is tracking
    /// </summary>
    private Transform _target;
    /// <summary>
    /// Reference to where bullets spawn
    /// </summary>
    private Transform _firePoint;
    /// <summary>
    /// Indicates what the bullets should collide with
    /// </summary>
    private LayerMask _whatToHit;
    /// <summary>
    /// The lowest this baddie can fly
    /// </summary>
    private float _minY;
    /// <summary>
    /// The highest this baddie can fly
    /// </summary>
    private float _maxY;
    /// <summary>
    /// How fast can the baddie move
    /// </summary>
    private float _moveSpeed = 3.5f;
    /// <summary>
    /// Indicates if this is facing right
    /// </summary>
    private bool _facingRight = false;
    private float targetY;
    /// <summary>
    /// References to wheree to fire the raycast angles
    /// -45degree down, 90degree down, 45degree down
    /// </summary>
    private Vector2[] _raycastAngles = new Vector2[] { new Vector2(-1, -1), Vector2.down, new Vector2(1, -1) };

    /// <summary>
    /// Reference to the bullet prefab
    /// </summary>
    public Transform BulletPrefab;

    /// <summary>
    /// Delay in between shooting
    /// </summary>
    private float _timeToFire;

    /// <summary>
    /// Find the player and begin tracking
    /// </summary>
    private void Start() {
        GameObject target = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);
        if (target != null) {
            _target = target.transform;
        }

        _firePoint = transform.Find(GameConstants.ObjectName_FirePoint);
        if (_firePoint == null) {
            Debug.LogError("AbstractWeapon.cs: No firePoint found");
            throw new UnassignedReferenceException();
        }

        var playerLayer = 1 << 8;
        var obstacleLayer = 1 << 10;
        _whatToHit = playerLayer | obstacleLayer;

        //Phsyics controller used for all collision detection
        Controller = transform.GetComponent<CollisionController2D>();
        if (Controller == null) {
            throw new MissingComponentException("There is no CollisionController2D on this object");
        }
        _maxY = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane)).y - 2;
        _minY = _target.position.y + 6;
        print("min = " + _minY + " max = " + _maxY);

        _timeToFire = Time.time + 5f;
    }

    /// <summary>
    /// Should just NOT be within a 45 degree angle nor 90 degrees above player.
    /// If you're not in one of these stay still.
    /// If you are - "flip a coin" to see if you move in the direction we're facing or backwards.
    /// ALWAYS face the player no matter what
    /// </summary>
    private void Update() {
        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - _target.position.x;
        CalcualteFacingDirection(directionToTarget);

        var rayLength = Vector2.Distance(transform.position, _target.position);
        CalculateAngleCollisions(rayLength);
        Debug.DrawRay(transform.position, (_target.position - transform.position), Color.red);

        CalculateFire();
    }

    private void CalculateFire() {
        if(Time.time > _timeToFire) {
            // Wait 5 seconds in between each shot
            _timeToFire = Time.time + 3f;
            Invoke("Fire", 0.1f);
            Invoke("Fire", 0.15f);
            Invoke("Fire", 0.2f);
        }
    }

    private void Fire() {
        Transform clone = Instantiate(BulletPrefab, _firePoint.position, _firePoint.rotation) as Transform;
        //Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
        AbstractProjectile projectile = clone.GetComponent<BulletProjectile>();

        //Set layermask of parent (either player or baddie)
        projectile.SetLayerMask(_whatToHit);
        projectile.Damage = 5;
        projectile.MoveSpeed = 12;
        projectile.MaxLifetime = 10;
        projectile.Fire(_target.position - transform.position, Vector2.up);
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

    private void CalculateAngleCollisions(float rayLength) {
        var targetLayer = 1 << 8;
        foreach (var angle in _raycastAngles) {
            Debug.DrawRay(transform.position, angle * rayLength, Color.green);
            RaycastHit2D collisionCheck = Physics2D.Raycast(transform.position, angle, rayLength, targetLayer);
        }
    }
}
