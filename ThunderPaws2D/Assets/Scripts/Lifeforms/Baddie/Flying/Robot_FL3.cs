using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot_FL3 : FlyingBaddieLifeform {
    /// <summary>
    /// References to wheree to fire the raycast angles
    /// -45degree down, 90degree down, 45degree down
    /// </summary>
    private Vector2[] _raycastAngles = new Vector2[] 
    {
        Vector2.right,
        new Vector2(1, 0.5f),
        new Vector2(1, 1),
        new Vector2(0.5f, 1f),
        Vector2.up,
        new Vector2(-0.5f, 1f),
        new Vector2(-1, 1),
        new Vector2(-1f, 0.5f),
        Vector2.left,
        new Vector2(-1f, -0.5f),
        new Vector2(-1, -1),
        new Vector2(-0.5f, -1f),
        Vector2.down,
        new Vector2(0.5f, -1f),
        new Vector2(1, -1),
        new Vector2(1f, -0.5f)
    };

    /// <summary>
    /// Each Flying baddie has their own implementation of the ActionData struct
    /// FL3 Specifc data.
    /// Encapsultes all the data needed for actions like attacking, and moving both vertical and horizontal
    /// </summary>
    private struct ActionData {
        public float HorizontalMoveSpeed;
        public float TimeToFire;
        public float MoveDuration;
        public int AngleIndex;
        public bool FiringAttack;
        /// <summary>
        /// Indicates if its a left, or right starting spiral - or a whole circle
        /// 1  = counter clockwise
        /// -1 = clockwise
        /// 0  = whole 
        /// </summary>
        public int AttackMode;
    }
    private ActionData _actionData;

    private const float DELAY_BETWEEN_SHOT = 5f;
    private const float MIN_DISTANCE_FROM_TARGET = 3f;
    private const float MAX_DISTANCE_FROM_TARGET = 12f;

    private void Start() {
        base.Start();

        _actionData.FiringAttack = false;
        _actionData.TimeToFire = Time.time + DELAY_BETWEEN_SHOT;

        if (Target != null) {
            CalculateBounds(MIN_DISTANCE_FROM_TARGET, MAX_DISTANCE_FROM_TARGET);
        }
        FlyingPositionData.MoveSpeed = 2.5f;

        FlyingPositionData.TargetYDirection = ChooseRandomHeight();
    }

    /// <summary>
    /// Should just NOT be within a 45 degree angle nor 90 degrees above player.
    /// If you're not in one of these stay still.
    /// If you are - "flip a coin" to see if you move in the direction we're facing or backwards.
    /// ALWAYS face the player no matter what
    /// </summary>
    private void Update() {
        base.Update();

        if (!CheckTargetsExist()) {
            return;
        }

        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - Target.position.x;
        CalculateFacingDirection(directionToTarget);

        // If we need to be moving do that instead of checking sightline
        if (!_actionData.FiringAttack) {
            CalculateVelocity();
        } else {
            Velocity.x = 0;
            Velocity.y = 0f;
            CalculateMovementDirection();
        }

        MaxBoundsCheck();

        Move();
        // Just useful for debugging
        Debug.DrawRay(transform.position, (Target.position - transform.position), Color.red);
        CalculateFire();
    }

    /// <summary>
    /// Based off the attack mode fire a pattern attack.
    /// Either we're firing a completely 360 area attack, or a spiral either
    /// counter/clockwise
    /// </summary>
    private void CalculateFire() {
        if (Time.time > _actionData.TimeToFire && !_actionData.FiringAttack) {
            _actionData.FiringAttack = true;
            _actionData.TimeToFire = Time.time + Random.Range(2,6);

            // Generate a random attack to keep this baddie unpredictable
            _actionData.AttackMode = DetermineRandomAttackMode();
            FireBasedOffPattern();
        }
    }

    /// <summary>
    /// 0 = Whole 360 Attack
    /// 1 or -1 are a counter or clockwise spiral attack
    /// </summary>
    private int DetermineRandomAttackMode() {
        var rf1 = (int)Random.Range(0, 8);
        // 1/4th chance to Whole 360 attack
        if(rf1 == 0 || rf1 == 7) {
            return 0;
        }else {
            // 50 / 50 for counter vs clockwise
            if(rf1 % 2 == 0) {
                return -1;
            }else {
                return 1;
            }
        }
    }

    private void FireBasedOffPattern() {
        InitiateAttack();
        if (_actionData.AttackMode == 0) {
                FireWhole();
            }else {
                FireSpiral();
            }
        Invoke("ResetAttack", 1f);
    }

    private void FireSpiral() {
        var fireTime = 0.05f;
        // Either start at the front or end of the array to easily generate clockwise or counter clockwise animation effect
        _actionData.AngleIndex = (_actionData.AttackMode > 0 ? 0 : _raycastAngles.Length - 1);
        for (var i = 0; i < _raycastAngles.Length; ++i) {
            Invoke("Fire", fireTime);
            fireTime += 0.05f;
        }
    }

    /// <summary>
    /// Generate an explosive 360 degree circular shot outward without stopping baddie movement
    /// </summary>
    private void FireWhole() {
        foreach(var angle in _raycastAngles) {
            Transform clone = Instantiate(BulletPrefab, ProjectileData.FirePoint.position, ProjectileData.FirePoint.rotation) as Transform;
            // Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
            AbstractProjectile projectile = clone.GetComponent<BulletProjectile>();

            // Set layermask of parent (either player or baddie)
            projectile.SetLayerMask(ProjectileData.WhatToHit);
            projectile.Damage = 5;
            projectile.MoveSpeed = 12;
            projectile.MaxLifetime = 10;
            projectile.Fire(angle, Vector2.up);
        }
        _actionData.FiringAttack = false;
    }

    /// <summary>
    /// Based off the attack type (counter or clockwise) generate a spiral attack
    /// starting at 0degrees and moving 45 degrees to 360.
    /// This attack stops baddie movement
    /// </summary>
    private void Fire() {
        try {
            Transform clone = Instantiate(BulletPrefab, ProjectileData.FirePoint.position, ProjectileData.FirePoint.rotation) as Transform;
            // Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
            AbstractProjectile projectile = clone.GetComponent<BulletProjectile>();

            // Set layermask of parent (either player or baddie)
            projectile.SetLayerMask(ProjectileData.WhatToHit);
            projectile.Damage = 5;
            projectile.MoveSpeed = 12;
            projectile.MaxLifetime = 10;
            projectile.Fire(_raycastAngles[_actionData.AngleIndex], Vector2.up);
            // _attackMode is either 1 or -1, so this allows for dynamic forward and backwards traversal of the array
            _actionData.AngleIndex = _actionData.AngleIndex + _actionData.AttackMode;
            if (_actionData.AngleIndex % 16 == 0 || _actionData.AngleIndex < 0) {
                _actionData.AngleIndex = 0;
                _actionData.FiringAttack = false;
            }
        }catch(System.Exception e) {
            print("Caught Exception trying to Fire from Baddie " + gameObject.name + " Exception : " + e.Message);
        }
    }
    
    private void CalculateVelocity() {
        Velocity.x = Mathf.SmoothDamp(Velocity.x, _actionData.HorizontalMoveSpeed, ref FlyingPositionData.VelocityXSmoothing, 0.2f);
        if (Time.time > _actionData.MoveDuration) {
            _actionData.MoveDuration = Time.time + Random.Range(1f, 4f);
            CalculateVerticalThreshold();
        }
        Velocity.y = Mathf.SmoothDamp(Velocity.y, FlyingPositionData.TargetYDirection * FlyingPositionData.MoveSpeed, ref FlyingPositionData.VelocityYSmoothing, 0.2f);
    }

    /// <summary>
    /// Similar to FL2 - allows for "dodge" like motion but also tries to stay relatively over the player.
    /// </summary>
    private void CalculateMovementDirection() {
        _actionData.MoveDuration = Time.time + (Random.Range(1f, 4f));

        // pos = we are on players right
        // neg = we are on players left
        var rf1 = ((Random.Range(2, 11) % 2 == 0) ? -1 : 1);
        var rf2 = Mathf.Sign(transform.position.x - Target.position.x);
        var rf3 = 0f;

        if (rf1 < 0 && rf2 < 0) {
            // If we should move left, and are already left of player we should have a 75% change of moving right
            // 25% chance to keep moving left
            rf3 = ((Random.Range(2, 11) % 6 == 0) ? -1 : 1);
        } else if (rf1 > 0 && rf2 > 0) {
            // If we should move right, and are already right of player we should have a 75% change of moving left
            // 25% chance to keep moving right
            rf3 = ((Random.Range(2, 11) % 6 == 0) ? 1 : -1);
        } else {
            // Otherwise, we're on the opposite side of the player from where we're about to move so do that
            rf3 = rf1;
        }

        _actionData.HorizontalMoveSpeed = FlyingPositionData.MoveSpeed * rf3;
        CalculateVerticalThreshold();
    }

    /// <summary>
    /// Just a useful helper method to see where exactly the bounds are.
    /// Uncomment this from the Update method if you want to see the bounds for a specific baddie
    /// </summary>
    private void ShowVerticalBounds() {
        var minLine = new Vector3(Target.position.x, FlyingPositionData.MinY, transform.position.z);
        Debug.DrawRay(minLine, Vector2.right * 50f, Color.green);
        Debug.DrawRay(minLine, Vector2.left * 50f, Color.green);
        print("minLine = " + minLine);
        var maxLine = new Vector3(Target.position.x, FlyingPositionData.MaxY, transform.position.z);
        Debug.DrawRay(maxLine, Vector2.right * 50f, Color.red);
        Debug.DrawRay(maxLine, Vector2.left * 50f, Color.red);
        print("maxLine = " + maxLine);
    }
}
