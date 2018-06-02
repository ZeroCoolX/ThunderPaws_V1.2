using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Robot_FL1 : BaddieLifeform {

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
    protected new void Start() {
        base.Start();

        // Assign the layermask for WhatToHit to be the Player(8) and Obstacle(10)
        AssignLayermask(8, 10);

        CalculateBounds();
        targetY = ChooseRandomHeight();
    }

    private void CalculateBounds() {
        // print("min = " + _minY + " max = " + _maxY + " for baddie : " + gameObject.name);
        if (IsHorde2Hack) {
            _minY = -85.89f;
            _maxY = -77.9f;
        }else {
            _minY = Target.position.y + 2f;
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
        }else if (Mathf.Sign(transform.position.y - Target.position.y) < 0) {
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

        CheckTargetsExist();

        // Every 2 seconds recalcualte the min and max just in case the playewr is in a much different spot vertically than before
        if (RecalculateBounds) {
            RecalculateBounds = false;
            Invoke("CalculateBounds", 2f);
        }

        // always check for bounds
        MaxBoundsCheck();

        var rayLength = Vector2.Distance(transform.position, Target.position);
        Debug.DrawRay(transform.position, Vector2.down * rayLength, Color.red);

        if (!OverPlayer() && !_bombDropInitiated) {
            // Find out where the target is in reference to this.
            var directionToTarget = transform.position.x - Target.position.x;
            CalculateFacingDirection(directionToTarget);

            CalculateVelocity();
            Move();
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
        _overPlayer = Mathf.Abs(transform.position.x - Target.position.x) < _overThreshold;
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
            if (Mathf.Sign(transform.position.y - Target.position.y) < 0) {
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
        Transform clone = Instantiate(BulletPrefab, ProjectileData.FirePoint.position, ProjectileData.FirePoint.rotation) as Transform;
        //Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
        AbstractProjectile projectile = clone.GetComponent<BulletProjectile>();

        //Set layermask of parent (either player or baddie)
        projectile.SetLayerMask(ProjectileData.WhatToHit);
        projectile.Damage = 10;
        projectile.MoveSpeed = 10;
        projectile.MaxLifetime = 10;
        projectile.Fire(Vector2.down, (FacingRight ? Vector2.right : Vector2.left));
        // Make sure we don't drop another bomb till at LEAST 2 seconds
        Invoke("ResetBombDrop", 0.5f);
    }

    /// <summary>
    /// Calculate the velocity
    /// </summary>
    private void CalculateVelocity() {
        float targetVelocityX = _moveSpeed * (FacingRight ? 1 : -1);
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref _velocityXSmoothing, 0.2f);
        if (Time.time > _timeToFindNewSpeed) {
            // random time between 1 and 3 seconds!
            _timeToFindNewSpeed = Time.time + Random.Range(1f, 4f);
            CalculateVerticalThreshold();
        }
        Velocity.y = Mathf.SmoothDamp(Velocity.y, targetY * _moveSpeed, ref _velocityYSmoothing, 1f);
    }
}
