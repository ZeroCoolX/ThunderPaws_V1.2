using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieBoss : BaddieLifeform {
    [Header("Movement Pattern")]
    public Transform[] Attack1Points;

    [Header("Vertical Attack")]
    public bool Vattack = false;
    private bool _vAttackInitiated = false;
    public delegate void VerticalHeavyAttackDelegate();
    public VerticalHeavyAttackDelegate PlayVerticalHeavyAttack;

    [Header("Horizontal Attack")]
    public bool Hattack = false;
    private bool _hAttackInitiated = false;
    public delegate void HorizontalHeavyAttackDelegate();
    public HorizontalHeavyAttackDelegate PlayHorizontalHeavyAttack;

    [Header("Default Attack")]
    public Transform FirePoint0;
    public Transform FirePoint45;
    public Transform FirePoint90;
    public bool Dattack = false;
    private bool _dAttackInitiated = false;
    private RaycastHit2D[] AttackHits = new RaycastHit2D[3];
    private float[] AttackHitDistances = new float[3];
    private List<Transform> _currentFirePoints;
    private float _attackDelay;
    private float _attackTimeToWait;

    private bool _allowPlayerfacing = true;
    private float smoothTime = 1F;
    private float yVelocity = 0.3F;
    private float xVelocity = 0.3F;
    private Vector3 _currentAttackPoint;
    private float _moveSpeed = 1f;
    private float _moveTrigger;
    private float _delayBetweenMoves = 1f;

    // Deterministic attack values
    private bool _verticalAttackChance;
    private bool _horizontalAttackChance;

    private float _delayBetweenSpecialAttacks = 0;

    private AttackType _lastAttackType;

    private enum AttackType { DEFAULT, VERTICAL, HORIZONAL}
    private AttackType _currentAttackType;

    public Transform GetTarget() {
        return Target;
    }

    private new void Start() {
        base.Start();
        MaxHealth = Health;
        Gravity = 0.0f;
        RandomlySelectAttackPoint();
        _moveTrigger = Time.time + _delayBetweenMoves;

        transform.GetComponent<VerticalHeavyAttack>().OnComplete += ResumeBasicAttack;
        transform.GetComponent<VerticalHeavyAttack>().ApplyDamageModifierForWeakSpot += ApplyDamageModifier;

        transform.GetComponent<HorizontalHeavyAttack>().OnComplete += ResumeBasicAttack;
        transform.GetComponent<HorizontalHeavyAttack>().ToggleFacingLock += ToggleFacingLock;
        transform.GetComponent<HorizontalHeavyAttack>().ApplyDamageModifierForWeakSpot += ApplyDamageModifier;

        ResetAttackDelay();
    }

    private void ApplyDamageModifier(int multiplier) {
        DamageMultiplier = multiplier;
    }

    private void ToggleFacingLock(bool allowFacing) {
        _allowPlayerfacing = allowFacing;
    }

    private void ResumeBasicAttack() {
        ApplyDamageModifier(1);
        Vattack = false;
        _vAttackInitiated = false;
        Hattack = false;
        _hAttackInitiated = false;
        _allowPlayerfacing = true;
        ResetAttackDelay();
        _currentAttackType = AttackType.DEFAULT;
    }

    private new void Update() {
        base.Update();
        if (!CheckTargetsExist()) {
            return;
        }
        BaddieHudManager.Instance.SetHealthStatus(Health, MaxHealth);

        var directionToTarget = transform.position.x - Target.position.x;
        if (_allowPlayerfacing) {
            CalculateFacingDirection(directionToTarget);
        }


        if (_currentAttackType == AttackType.VERTICAL && !_vAttackInitiated) {
            _vAttackInitiated = true;
            PlayVerticalHeavyAttack.Invoke();
        }
        if (_vAttackInitiated) {
            return;
        }

        if (_currentAttackType == AttackType.HORIZONAL && !_hAttackInitiated) {
            _hAttackInitiated = true;
            PlayHorizontalHeavyAttack.Invoke();
        }
        if (_hAttackInitiated) {
            return;
        }


        CheckIfCanAttack();
        CalculateAttackFirepoint();
        if (Dattack && !_dAttackInitiated) {
            Attack();
        }

        if (Time.time > _moveTrigger) {
            RandomlySelectAttackPoint();
            _moveTrigger = Time.time + _delayBetweenMoves;
        }
        CalculateVelocity();

        DetermineNextAttackType();
    }

    private void CheckIfCanAttack() {
        if (Time.time > _attackTimeToWait) {
            Dattack = true;
            _attackDelay = Random.Range(3f, 10f);
            _attackTimeToWait = Time.time + _attackDelay;
        }
    }

    private void CalculateAttackFirepoint() {
        CalculateRaycastHits();

        DrawForTesting();

        if (!Dattack) {
            return;
        }

        CalculateHitDistancesToTarget();

        AddAllFirepoints(AssignClosesFirePoint());
    }

    private void CalculateRaycastHits() {
        // Shoot out a raycast from each fire point till it collides with something on the obstacle  or player layer
        Vector2 rotation0 = new Vector3(0.5f * (FacingRight ? 1 : -1), -0.1f, 0);
        Vector2 rotation45 = new Vector3(0.5f * (FacingRight ? 1 : -1), -0.5f, 0);
        Vector2 rotation90 = new Vector3(0f, -1f, 0);

        // Player or obstacle
        var layermask = (1 << 8) | (1 << 10);

        AttackHits[0] = (Physics2D.Raycast(FirePoint0.position, rotation0, 50, layermask));
        AttackHits[1] = (Physics2D.Raycast(FirePoint45.position, rotation45, 50, layermask));
        AttackHits[2] = (Physics2D.Raycast(FirePoint90.position, rotation90, 50, layermask));
    }

    private void DrawForTesting() {
        Debug.DrawRay(FirePoint0.position, new Vector3(0.5f * (FacingRight ? 1 : -1), -0.1f, 0) * 50, Color.green);
        Debug.DrawRay(FirePoint45.position, new Vector3(0.5f * (FacingRight ? 1 : -1), -0.5f, 0) * 50, Color.green);
        Debug.DrawRay(FirePoint90.position, new Vector3(0f, -1f, 0) * 50, Color.green);
    }

    private void CalculateHitDistancesToTarget() {
        for (int i = 0; i < AttackHitDistances.Length; ++i) {
            if (AttackHits[i].collider == null) {
                AttackHitDistances[i] = 1000;
            } else {
                AttackHitDistances[i] = Vector3.Distance(AttackHits[i].collider.transform.position, Target.position);
            }
        }
    }

    private Transform AssignClosesFirePoint() {
        var min = Mathf.Min(AttackHitDistances[0], AttackHitDistances[1], AttackHitDistances[2]);
        _currentFirePoints = new List<Transform>();
        if (min == AttackHitDistances[0]) {
            return FirePoint0;
        } else if (min == AttackHitDistances[1]) {
            return FirePoint45;
        } else {
            return FirePoint90;
        }
    }

    private void AddAllFirepoints(Transform firepointParent) {
        foreach (Transform child in firepointParent) {
            _currentFirePoints.Add(child);
        }
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

    private void ResetAttackDelay() {
        _delayBetweenSpecialAttacks = Time.time + Random.Range(5, 10);
    }

    private void DetermineNextAttackType() {
        if(Time.time <= _delayBetweenSpecialAttacks) {
            return;
        }

        _currentAttackType = AttackType.DEFAULT;

        var rand = Random.Range(0, 10);

        // If health is above 3/4 - 75% chance a special attack will happen
        if(Health >= MaxHealth * 0.75) {
            if(rand > 3) {
                _currentAttackType = GenerateAttackType();
            }
            // Normal Default speed
        } else if (Health >= MaxHealth * 0.5f) { // 50% chance special attack happens
            // Middle speed
            if(rand != 0 && rand % 2 == 0) {
                _currentAttackType = GenerateAttackType();
            }
        } else { // 25% chance special attack happens
            // Fastest, most violent
            if(rand < 3) {
                _currentAttackType = GenerateAttackType();
            }
        }
    }

    private AttackType GenerateAttackType() {
        var rand = Random.Range(0, 10);
        print("Random : " + rand);
        return rand % 2 == 0 ? AttackType.VERTICAL : AttackType.HORIZONAL;
    }
}
