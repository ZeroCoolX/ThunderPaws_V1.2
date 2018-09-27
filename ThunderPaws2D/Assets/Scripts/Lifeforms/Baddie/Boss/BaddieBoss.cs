using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieBoss : BaddieLifeform {
    public Transform[] Attack1Points;

    public Transform FirePoint0;
    public Transform FirePoint45;
    public Transform FirePoint90;

    [Header("Play Vertical Attack")]
    public bool Vattack = false;
    private bool _vAttackInitiated = false;
    [Header("Play Horizontal Attack")]
    public bool Hattack = false;
    private bool _hAttackInitiated = false;
    [Header("Play Default Attack")]
    public bool Dattack = false;
    private bool _dAttackInitiated = false;

    private bool _allowPlayerfacing = true;

    private float smoothTime = 1F;
    private float yVelocity = 0.3F;
    private float xVelocity = 0.3F;
    private Vector3 _currentAttackPoint;
    private float _moveSpeed = 1f;
    private float _moveTrigger;
    private float _delayBetweenMoves = 1f;

    public delegate void VerticalHeavyAttackDelegate();
    public VerticalHeavyAttackDelegate PlayVerticalHeavyAttack;

    public delegate void HorizontalHeavyAttackDelegate();
    public HorizontalHeavyAttackDelegate PlayHorizontalHeavyAttack;

    private RaycastHit2D[] AttackHits = new RaycastHit2D[3];
    private float[] AttackHitDistances = new float[3];

    private List<Transform> _currentFirePoints;

    public Transform GetTarget() {
        return Target;
    }

    private new void Start() {
        base.Start();
        Gravity = 0.0f;
        RandomlySelectAttackPoint();
        _moveTrigger = Time.time + _delayBetweenMoves;

        transform.GetComponent<VerticalHeavyAttack>().OnComplete += ResumeBasicAttack;
        transform.GetComponent<HorizontalHeavyAttack>().OnComplete += ResumeBasicAttack;
        transform.GetComponent<HorizontalHeavyAttack>().ToggleFacingLock += ToggleFacingLock;
    }

    private void ToggleFacingLock(bool allowFacing) {
        _allowPlayerfacing = allowFacing;
    }

    private void ResumeBasicAttack() {
        Vattack = false;
        _vAttackInitiated = false;
        Hattack = false;
        _hAttackInitiated = false;
        _allowPlayerfacing = true;
    }

    // Update is called once per frame
    private new void Update() {
        base.Update();
        if (!CheckTargetsExist()) {
            return;
        }

        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - Target.position.x;
        if (_allowPlayerfacing) {
            CalculateFacingDirection(directionToTarget);
        }

        if (Vattack && !_vAttackInitiated) {
            _vAttackInitiated = true;
            PlayVerticalHeavyAttack.Invoke();
        }
        if (_vAttackInitiated) {
            return;
        }

        if (Hattack && !_hAttackInitiated) {
            _hAttackInitiated = true;
            PlayHorizontalHeavyAttack.Invoke();
        }

        CalculateAttackFirepoint();
        if (Dattack && !_dAttackInitiated) {
            Attack();
        }

        if (Time.time > _moveTrigger) {
            RandomlySelectAttackPoint();
            _moveTrigger = Time.time + _delayBetweenMoves;
        }
        CalculateVelocity();
    }

    private void CalculateVelocity() {
        float newX = Mathf.SmoothDamp(transform.position.x, _currentAttackPoint.x, ref yVelocity, smoothTime);
        float newY = Mathf.SmoothDamp(transform.position.y, _currentAttackPoint.y, ref xVelocity, smoothTime);
        transform.position = new Vector3(newX, newY, transform.position.z);
    }

    private void RandomlySelectAttackPoint() {
        var rand = Random.Range(0, 5);
        _currentAttackPoint = Attack1Points[rand].position;
    }

    private void Attack() {
        var fireTime = 0f;
        for (var i = 0; i < 25; ++i) {
            Invoke("Fire", fireTime);
            fireTime += 0.075f;
        }

        Dattack = false;
        _dAttackInitiated = false;
    }

    private void Fire() {
        try {
            foreach(var fp in _currentFirePoints) {
                Transform clone = Instantiate(BulletPrefab, fp.position, Quaternion.identity) as Transform;
                // Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
                AbstractProjectile projectile = clone.GetComponent<BulletProjectile>();

                // Set layermask of parent (either player or baddie)
                projectile.SetLayerMask(ProjectileData.WhatToHit);
                projectile.Damage = 5;
                projectile.MoveSpeed = 12;
                projectile.MaxLifetime = 5;
                projectile.Fire(Target.position - transform.position, Vector2.up);
            }
        } catch (System.Exception e) {
            print("Caught Exception trying to Fire from Baddie " + gameObject.name + " Exception : " + e.Message);
        }
    }

    private void CalculateAttackFirepoint() {
        // Shoot out a raycast from each fire point till it collides with something on the obstacle  or player layer
        Vector2 rotation0 = new Vector3(0.5f * (FacingRight ? 1 : -1), -0.1f, 0);
        Vector2 rotation45 = new Vector3(0.5f * (FacingRight ? 1 : -1), -0.5f, 0);
        Vector2 rotation90 = new Vector3(0f, -1f, 0);

        // Player or obstacle
        var layermask = (1 << 8) | (1 << 10);

        AttackHits[0] = (Physics2D.Raycast(FirePoint0.position, rotation0, 50, layermask));
        AttackHits[1] = (Physics2D.Raycast(FirePoint45.position, rotation45, 50, layermask));
        AttackHits[2] = (Physics2D.Raycast(FirePoint90.position, rotation90, 50, layermask));

        Debug.DrawRay(FirePoint0.position, new Vector3(0.5f * (FacingRight ? 1 : -1), -0.1f, 0) * 50, Color.green);
        Debug.DrawRay(FirePoint45.position, new Vector3(0.5f *(FacingRight ? 1 : -1), -0.5f, 0) * 50, Color.green);
        Debug.DrawRay(FirePoint90.position, new Vector3(0f, -1f, 0) * 50 , Color.green);

        if (!Dattack) {
            return;
        }
        if (AttackHits[0].collider == null) {
            AttackHitDistances[0] = 1000;
        }else {
            AttackHitDistances[0] = Vector3.Distance(AttackHits[0].collider.transform.position, Target.position);
        }
        if (AttackHits[1].collider == null) {
            AttackHitDistances[1] = 1000;
        }else {
            AttackHitDistances[1] = Vector3.Distance(AttackHits[1].collider.transform.position, Target.position);
        }
        if (AttackHits[2].collider == null) {
            AttackHitDistances[2] = 1000;
        }else {
            AttackHitDistances[2] = Vector3.Distance(AttackHits[2].collider.transform.position, Target.position);
        }

        var min = Mathf.Min(AttackHitDistances[0], AttackHitDistances[1], AttackHitDistances[2]);
        _currentFirePoints = new List<Transform>();
        Transform currentFirePoint;
        if (min == AttackHitDistances[0]) {
            currentFirePoint = FirePoint0;
        } else if(min == AttackHitDistances[1]) {
            currentFirePoint = FirePoint45;
        } else {
            currentFirePoint = FirePoint90;
        }

        foreach(Transform child in currentFirePoint) {
            _currentFirePoints.Add(child);
        }
    }
}
