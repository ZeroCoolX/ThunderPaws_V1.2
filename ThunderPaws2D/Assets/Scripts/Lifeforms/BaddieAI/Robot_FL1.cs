using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot_FL1 : DamageableLifeform {
    /// <summary>
    /// Reference to bullet prefab
    /// </summary>
    public Transform BulletPrefab;
    /// <summary>
    /// Reference to where we should spawn the bullet from
    /// </summary>
    private Transform _firePoint;
    /// <summary>
    /// Indicates what the bullet should hit
    /// </summary>
    private LayerMask _whatToHit;

    /// <summary>
    /// Indicates what the baddie chases after
    /// </summary>
    private Transform _target;

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
    private float _moveSpeed = 2f;
    /// <summary>
    /// Indicates if this is facing right
    /// </summary>
    private bool _facingRight = false;

    /// <summary>
    /// Only needed for the Math.SmoothDamp function
    /// </summary>
    private float _velocityXSmoothing;
    private float _velocityYSmoothing;
    /// <summary>
    /// Need a reference to how close in the x direction we're trying to get
    /// </summary>
    private float _overThreshold = 0.1f;
    /// <summary>
    /// Trigger to indicate we're over the player 
    /// </summary>
    private bool _overPlayer = false;
    /// <summary>
    /// Indicates that a bomb is being dropped - so don't drop anymore until we're through
    /// </summary>
    private bool _bombDropInitiated = false;

    private float _heightAbovePlayer;

    private bool moveToNewY = false;
    private float targetY;

    private bool RecalculateBounds = false;

    private float _timeToFindNewSpeed;

    public bool IsHorde2Hack = false;

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
    }

    private void CalculateBounds() {
        // print("min = " + _minY + " max = " + _maxY + " for baddie : " + gameObject.name);
        if (IsHorde2Hack) {
            _minY = -85.89f;
            _maxY = -77.9f;
        }else {
            _minY = _target.position.y + 2f;
            _maxY = _minY + 6f;
        }
    }

    private void MaxBoundsCheck() {
        if (transform.position.y >= _maxY) {
            //print("Send it to the min");
            targetY = -1;
        } else if (transform.position.y <= _minY) {
            //print("Send it to the max");
            targetY = 1;
        }else if (Mathf.Sign(transform.position.y - _target.position.y) < 0) {
            targetY = 1;
        }
    }

    /// <summary>
    /// Track the target.
    /// Locate its position.
    /// Move towards it horizontally until the x coordinates are within (some) range and stop
    /// </summary>
    private void Update() {
        base.Update();
        if (_target == null) {
            FindPlayer();
            return;
        }

        // Every 2 seconds recalcualte the min and max just in case the playewr is in a much different spot vertically than before
        if (RecalculateBounds) {
            RecalculateBounds = false;
            Invoke("CalculateBounds", 2f);
        }

        // always check for bounds
        MaxBoundsCheck();

        var rayLength = Vector2.Distance(transform.position, _target.position);
        Debug.DrawRay(transform.position, Vector2.down * rayLength, Color.red);

        if (!OverPlayer() && !_bombDropInitiated) {
            // Find out where the target is in reference to this.
            var directionToTarget = transform.position.x - _target.position.x;
            CalcualteFacingDirection(directionToTarget);

            CalculateVelocity();
            Controller.Move(Velocity * Time.deltaTime);
        }
    }

    private void FindPlayer() {
        // Find the player and store the target reference
        GameObject target = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);
        if (target != null) {
            _target = target.transform;
        }else {
            // This most likely means that he died!!! He will be respawning at a checkpoint so turn off this script
            enabled = false;
            // The BaddieActivator will turn you back on - don't worry
        }
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

    private bool OverPlayer() {
        _overPlayer = Mathf.Abs(transform.position.x - _target.position.x) < _overThreshold;
        if (_overPlayer && !_bombDropInitiated) {
            // Wait 0.25seconds then drop bomb.
            Invoke("DropBomb", 0.1f);
            _bombDropInitiated = true;
        }
        return _overPlayer;
    }

    private void ResetBombDrop() {
        _bombDropInitiated = false;
    }

    private void CalculateVerticalThreshold() {
        if (transform.position.y >= _maxY) {
            print("Send it to the min");
            targetY = -1;
        } else if (transform.position.y <= _minY) {
            print("Send it to the max");
            targetY = 1;
        } else {
            if (Mathf.Sign(transform.position.y - _target.position.y) < 0) {
                targetY = 1;
            }else {
                targetY = Mathf.Sign(ChooseRandomHeight());
            }
        }
    }

    private void DropBomb() {
        if (!_overPlayer) {
            Invoke("ResetBombDrop", 0f);
            return;
        }
        print("Fire!");
        Transform clone = Instantiate(BulletPrefab, _firePoint.position, _firePoint.rotation) as Transform;
        //Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
        AbstractProjectile projectile = clone.GetComponent<BulletProjectile>();

        //Set layermask of parent (either player or baddie)
        projectile.SetLayerMask(_whatToHit);
        projectile.Damage = 10;
        projectile.MoveSpeed = 10;
        projectile.MaxLifetime = 10;
        projectile.Fire(Vector2.down, (_facingRight ? Vector2.right : Vector2.left));
        // Make sure we don't drop another bomb till at LEAST 2 seconds
        Invoke("ResetBombDrop", 0.5f);
    }

    /// <summary>
    /// Calculate the velocity
    /// </summary>
    private void CalculateVelocity() {
        float targetVelocityX = _moveSpeed * (_facingRight ? 1 : -1);
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref _velocityXSmoothing, 0.2f);
        if (Time.time > _timeToFindNewSpeed) {
            // random time between 1 and 3 seconds!
            _timeToFindNewSpeed = Time.time + Random.Range(1f, 4f);
            CalculateVerticalThreshold();
        }
        Velocity.y = Mathf.SmoothDamp(Velocity.y, targetY * _moveSpeed, ref _velocityYSmoothing, 1f);
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
