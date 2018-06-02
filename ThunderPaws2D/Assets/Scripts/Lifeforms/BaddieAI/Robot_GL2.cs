using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Robot_GL2 : DamageableLifeform {

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
    /// Who the baddie is focused on attacking
    /// Turned into array for co-op
    /// </summary>
    private List<Transform> _targets = new List<Transform>();

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
        FindPlayers();
        if (_target == null) {
            ChooseTarget();
        }

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
        if (_targets == null || _targets.Where(t => t != null).ToList().Count == 0) {
            FindPlayers();
            return;
        } else if (_targets.Contains(null)) {
            _targets = _targets.Where(target => target != null).ToList();
            // This means we only 
            print("An item in the _targets list is null meaning a player died");
            // The max spawn time is 10 seconds so in 10 seconds search again for a player
            Invoke("FindPlayers", 10f);
        }

        if (_target == null) {
            ChooseTarget();
        }
        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - _target.position.x;
        // Check if we can shoot at the target
        CheckForHorizontalEquality(directionToTarget);
    }

    private void FindPlayers() {
        // Find the player and store the target reference
        GameObject[] targets = GameObject.FindGameObjectsWithTag(GameConstants.Tag_Player);
        if (targets == null || targets.Where(t => t != null).ToList().Count == 0) {
            return;
        }
        foreach (var target in targets) {
            // Only add the player if its not already in the list
            if (!_targets.Contains(target.transform)) {
                _targets.Add(target.transform);
            }
        }
    }

    private void ChooseTarget() {
        if (_targets == null || _targets.Where(t=>t != null).ToList().Count == 0) {
            return;
        } else if (_targets.Count == 1) {
            _target = _targets.FirstOrDefault();
        } else {
            // choose a random player
            var targets = _targets.Where(t => t != null).ToArray();
            var index = Random.Range(0, targets.Length - 1);
            _target = targets[index];
        }
        print("FOUND NEW TARGET! : " + _target.gameObject.name);
    }

    private void CheckForHorizontalEquality(float dirToTarget) {
        var targetLayer = 1 << 8;

        Debug.DrawRay(_firePoint.position, (_facingRight ? Vector2.right : Vector2.left) * _visionRaylength, Color.red);

        RaycastHit2D horizontalCheck = Physics2D.Raycast(_firePoint.position, _facingRight ? Vector2.right : Vector2.left, _visionRaylength, targetLayer);

        if (horizontalCheck.collider != null && Time.time > _timeSinceLastFire) {
            print("Hit!");
            // Shoot a projectile towards the target in 1 second
            _timeSinceLastFire = Time.time + _shotDelay;
            Velocity.x = 0f;
            _animator.SetBool("ChargeAndFire", true);
            Invoke("Fire", 0.5f);
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
