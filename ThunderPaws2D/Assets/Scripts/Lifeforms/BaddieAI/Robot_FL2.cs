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
    /// Stores the value of the horizontal movement this iteration of movement
    /// </summary>
    private float _horizontalMovespeed;
    /// <summary>
    /// Indicates if this is facing right
    /// </summary>
    private bool _facingRight = false;
    /// <summary>
    /// Needed to determine where in the y direction to go based off max and min values so we stay in frame
    /// </summary>
    private float targetY;

    /// <summary>
    /// Only needed for the Math.SmoothDamp function
    /// </summary>
    private float _velocityXSmoothing;
    private float _velocityYSmoothing;
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
    /// Indicates how long to move for
    /// </summary>
    private float _moveDuration;
    /// <summary>
    /// Indicates how far the player can move away from the enemy until it stops firing
    /// </summary>
    private float _distanceThreshold = 15f;

    private bool RecalculateBounds = false;

    private bool MaxBoundsOverride = false;

    /// <summary>
    /// Find the player and begin tracking
    /// </summary>
    private void Start() {
        FindPlayer();

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
        CalculateBounds();

        targetY = ChooseRandomHeight();
        print("min = " + _minY + " max = " + _maxY);

        _timeToFire = Time.time + 1f;
    }

    private void CalculateBounds() {
        _minY = _target.position.y + 2f;
        _maxY = _minY + 6f;
        RecalculateBounds = true;
    }

    /// <summary>
    /// Should just NOT be within a 45 degree angle nor 90 degrees above player.
    /// If you're not in one of these stay still.
    /// If you are - "flip a coin" to see if you move in the direction we're facing or backwards.
    /// ALWAYS face the player no matter what
    /// </summary>
    private void Update() {
        base.Update();
        if (_target == null) {
            FindPlayer();
            return;
        }

        // Every 2 seconds recalcualte the min and max just in case the playewr is in a much different spot vertically than before
        //if (RecalculateBounds) {
        //    RecalculateBounds = false;
        //    Invoke("CalculateBounds", 2f);
        //}

        MaxBoundsCheck();

        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - _target.position.x;
        CalcualteFacingDirection(directionToTarget);

        var rayLength = Vector2.Distance(transform.position, _target.position);

        // If we need to be moving do that instead of checking sightline
        if (_moveDuration > Time.time || !StillAbovePlayerCheck()) {
            CalculateVelocity();
        }else {
            Velocity.x = 0;
            Velocity.y = 0f;
            CalculateAngleCollisions(rayLength);
        }
        Controller.Move(Velocity * Time.deltaTime);
        Debug.DrawRay(transform.position, (_target.position - transform.position), Color.red);

        CalculateFire();
    }

    private void FindPlayer() {
        // Find the player and store the target reference
        GameObject target = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);
        if (target != null) {
            _target = target.transform;
        }
    }

    private void CalculateFire() {
        if(Time.time > _timeToFire && Vector2.Distance(transform.position, _target.position) <= _distanceThreshold) {
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

    /// <summary>
    /// Choose a random height between 1 unit above the player, and the highest we can go without going out of the viewport
    /// </summary>
    /// <returns></returns>
    private float ChooseRandomHeight() {
        var randY = Random.Range(_minY, _maxY);
        //print("Random Y = " + randY);
        return randY;
    }

    private void MaxBoundsCheck() {
        if (transform.position.y >= _maxY) {
            MaxBoundsOverride = true;
          //  print("Send it to the min");
            targetY = -1;
        } else if (transform.position.y <= _minY) {
            MaxBoundsOverride = true;
           // print("Send it to the max");
            targetY = 1;
        } else if (Mathf.Sign(transform.position.y - _target.position.y) < 0) {
            MaxBoundsOverride = true;
            targetY = 1;
        }
    }

    /// <summary>
    /// whileMovementCheck just ensures that while we're moving we're keeping track of the bounds. 
    /// True indicates we want to se a new min and max if we hit the edges only.
    /// False indicates the same above, but also to choose a random height if not at the max or min
    /// </summary>
    /// <param name="whileMovementCheck"></param>
    private void CalculateVerticalThreshold() {
        if (transform.position.y >= _maxY) {
          //  print("Send it to the min");
            targetY = -1;
        } else if (transform.position.y <= _minY) {
          //  print("Send it to the max");
            targetY = 1;
        } else {
            if (Mathf.Sign(transform.position.y - _target.position.y) < 0) {
                targetY = 1;
            } else {
                targetY = Mathf.Sign(ChooseRandomHeight());
            }
        }
    }

    private bool StillAbovePlayerCheck() {
        return Mathf.Sign(transform.position.y - _target.position.y) > 0;
    }

    private void CalculateVelocity() {
        Velocity.x = Mathf.SmoothDamp(Velocity.x, _horizontalMovespeed, ref _velocityXSmoothing, 0.2f);
        CalculateVerticalThreshold();
        Velocity.y = Mathf.SmoothDamp(Velocity.y, targetY, ref _velocityYSmoothing, 1f);
    }

    // new Vector2(-1, -1), Vector2.down, new Vector2(1, -1)
    private void CalculateAngleCollisions(float rayLength) {
        var targetLayer = 1 << 8;
        foreach (var angle in _raycastAngles) {
            Debug.DrawRay(transform.position, angle * rayLength, Color.green);
            RaycastHit2D collisionCheck = Physics2D.Raycast(transform.position, angle, rayLength, targetLayer);
            if(collisionCheck.collider != null) {
                // We are either directly above or within the 45degree angle of the player and should move!

                // Right now move between 1 and 3 seconds
                _moveDuration = Time.time + (Random.Range(1, 4));

                // -1 = move left 
                // 1 = move right
                var rf1 = ((Random.Range(2, 11) % 2 == 0) ? -1 : 1);

                // pos = we are on players right
                // neg = we are on players left
                var rf2 = Mathf.Sign(transform.position.x - _target.position.x);

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

                _horizontalMovespeed = _moveSpeed * rf3;
                CalculateVerticalThreshold();
            }
        }
    }
}
