using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot_GL1 : DamageableLifeform {
    /// <summary>
    /// Indicates this baddie is bound by ledges
    /// </summary>
    public bool LedgeBound = false;

    // These might be overkill
    /// <summary>
    /// Used for dampening movenents
    /// </summary>
    protected float AccelerationTimeAirborne = 0.2f;
    /// <summary>
    /// Used for dampening movement
    /// </summary>
    protected float AccelerationTimeGrounded = 0.1f;

    // These might be overkill as well
    /// <summary>
    /// Keep a reference to how far we've moved without stopping
    /// </summary>
    private float _unitsTraveledSinceLastStop = 0f;
    /// <summary>
    /// Max distance from last stop this baddie is allowed to go
    /// </summary>
    private float _maxUnitsAllowsToTravelBetweenStops;

    /// <summary>
    /// How fast the baddie moves
    /// </summary>
    private int _moveSpeed = 5;

    /// <summary>
    /// Just used as a reference for the Mathf.SmoothDamp function
    /// </summary>
    private float _velocityXSmoothing;

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
    /// Indicates if this baddie should turn around upon reaching the camera edge or a ledge edge
    /// </summary>
    public bool TurnAround = false;

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

    private bool _turnAround = true;
    private float _alertTimeThreshold = -1f;
    private Vector2 _moveDirection = Vector2.left;

    /// <summary>
    /// How far out the baddie searches to see if it's on the same horizontal plane as the baddie
    /// This indicates it should start shooting in the direction of the target
    /// </summary>
    private float _visionRaylength = 10f;

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

        _firePoint = transform.Find(GameConstants.ObjectName_FirePoint);
        if (_firePoint == null) {
            Debug.LogError("AbstractWeapon.cs: No firePoint found");
            throw new UnassignedReferenceException();
        }

        Gravity = -25.08f;
        Health = 5;
    }

    public void Update() {
        base.Update();
        if (_target == null) {
            FindPlayer();
            return;
        }

        if (LedgeBound) {
            // Check if we can shoot at the target
            CheckForTargetInFront();
            if(_alertTimeThreshold >= Time.time) {
                print("sTOPPING time");
                Velocity = Vector2.zero;
                return;
            }

            CalcualteFacingDirection(_moveDirection.x*-1);
            if (Controller.Collisions.NearLedge && _turnAround) {
                print("NEAR LEDGE!!!");
                _moveDirection.x  = Vector2.right.x * (_facingRight ? -1f : 1f);
                _turnAround = false;
                Invoke("ResetTurnAround", 0.5f);
            }
            // Check if we can shoot at the target
            // CheckForTargetInFront();
            //Move the baddie
            float targetVelocityX = _moveSpeed * _moveDirection.x;
            Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref _velocityXSmoothing, Controller.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
            ApplyGravity();

        } else {
            // Find out where the target is in reference to this.
            var directionToTarget = transform.position.x - _target.position.x;

            // Check if we can shoot at the target
            CheckForHorizontalEquality(directionToTarget);

            // Face that direction
            CalcualteFacingDirection(directionToTarget);

            // Move in that direction
            if (Time.time > _timeStopped) {
                CalculateVelocity();
            }
        }

        Controller.Move(Velocity * Time.deltaTime);
    }

    private void FindPlayer() {
        // Find the player and store the target reference
        GameObject target = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);
        if (target != null) {
            _target = target.transform;
        }
    }

    private void ResetTurnAround() {
        _turnAround = true;
    }

    private void CheckForTargetInFront() {
        var targetLayer = 1 << 8;

        Debug.DrawRay(_firePoint.position, _moveDirection * _visionRaylength, Color.red);

        RaycastHit2D horizontalCheck = Physics2D.Raycast(_firePoint.position, _moveDirection, _visionRaylength, targetLayer);
        if(horizontalCheck.collider != null) {
            _alertTimeThreshold = Time.time + 2f;
        }
        if (horizontalCheck.collider != null && Time.time > _timeSinceLastFire) {
            print("Hit!");
            // Has a chance to fire a bullet
            _timeStopped = Time.time + _maxStopSeconds;
            // Shoot a projectile towards the target in 1 second
            _timeSinceLastFire = Time.time + _shotDelay;
            Velocity.x = 0f;
            Invoke("StopAndFire", 1f);
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
            Invoke("StopAndFire", 1f);
        }
    }

    private void StopAndFire() {
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

    /// <summary>
    /// Calculate the velocity
    /// </summary>
    private void CalculateVelocity() {
        float targetVelocityX = _moveSpeed * (_facingRight ? 1 : -1);
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref _velocityXSmoothing, Controller.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        //CalculateRandomStop();
        ApplyGravity();
    }

    //private void CalculateRandomStop() {
    //    if (Random.Range(1, 10) == _randStopTarget) {
    //        print("Randomly Stopping");
    //        Velocity.x = 0f;
    //        _timeStopped = Time.time + _maxStopSeconds;
    //    }
    //}
}
