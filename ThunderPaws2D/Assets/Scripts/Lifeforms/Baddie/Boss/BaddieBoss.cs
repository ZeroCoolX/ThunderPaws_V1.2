using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieBoss : BaddieLifeform {
    [Header("Death Spot")]
    public Transform DeathSpot;

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

    [Header("Explosion Prefab")]
    public Transform WeakspotExplosionPrefab;

    private Transform _weakspotExplosion;

    private SimpleCollider _deathSpotCollider;
    private bool _deathInitiated = false;
    private bool _dead = false;

    private float _camShakeAmount = 0.1f;
    private float _camShakeLength = 0.5f;
    private CameraShake _camShake;

    public bool Dattack = false;
    private bool _dAttackInitiated = false;
    private RaycastHit2D[] AttackHits = new RaycastHit2D[3];
    private float[] AttackHitDistances = new float[3];
    private List<Transform> _currentFirePoints;
    private string _defaultFireAngleAnimation = "Attack1-0";
    private float _attackDelay;
    private float _attackTimeToWait;
    private int _lastAttackIndex = 0;
    private AttackType[] _lastTwoAttacks = new AttackType[2] { AttackType.DEFAULT, AttackType.DEFAULT };

    private enum DamageAmount{ NONE, SOME, ALL}
    private DamageAmount _damageTaken = DamageAmount.NONE;
    private int _damageLayer = 0;

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

    private Stack<HealthChunk> _healthChunks = new Stack<HealthChunk>();
    private HealthChunk _currentHealth;
    private int _chunkIndex = 0;

    private struct HealthChunk {
        public float MaxHealth;
        public float Health;
    }

    public Transform GetTarget() {
        return Target;
    }

    private new void Start() {
        base.Start();
        MaxHealth = Health;

        var healthChunk = MaxHealth / 3;

        for(int i = 0; i < 3; ++i) {
            _healthChunks.Push(new HealthChunk { Health = healthChunk, MaxHealth = healthChunk });
        }

        _currentHealth = _healthChunks.Pop();

        Gravity = 0.0f;
        RandomlySelectAttackPoint();
        _moveTrigger = Time.time + _delayBetweenMoves;

        transform.GetComponent<VerticalHeavyAttack>().OnComplete += ResumeBasicAttack;
        transform.GetComponent<VerticalHeavyAttack>().ApplyDamageModifierForWeakSpot += ApplyDamageModifier;
        transform.GetComponent<VerticalHeavyAttack>().ShakeCamera += GenerateCameraShake;

        transform.GetComponent<HorizontalHeavyAttack>().OnComplete += ResumeBasicAttack;
        transform.GetComponent<HorizontalHeavyAttack>().ToggleFacingLock += ToggleFacingLock;
        transform.GetComponent<HorizontalHeavyAttack>().ApplyDamageModifierForWeakSpot += ApplyDamageModifier;
        transform.GetComponent<HorizontalHeavyAttack>().ShakeCamera += GenerateCameraShake;

        _weakspotExplosion = transform.Find("ExplosionOrigin");
        if(_weakspotExplosion == null) {
            throw new MissingComponentException("Baddie Boss was missing a weakspot explosion");
        }

        _camShake = GameMasterV2.Instance.GetComponent<CameraShake>();
        if (_camShake == null) {
            throw new MissingComponentException("Weapon.cs: No CameraShake found on game master");
        }

        ResetAttackDelay();

        DeathSpot.GetComponent<SimpleCollider>().enabled = false;
    }

    private void Apply(Vector3 v, Collider2D c) {
        StartCoroutine(PlayFinalDeathAnimation());
        DeathSpot.GetComponent<SimpleCollider>().enabled = false;
    }

    private void GenerateCameraShake() {
        _camShake.Shake(_camShakeAmount, _camShakeLength);
    }

    private void ApplyDamageModifier(int multiplier) {
        DamageMultiplier = multiplier;
    }

    private void ToggleFacingLock(bool allowFacing) {
        _allowPlayerfacing = allowFacing;
    }

    protected override void PreDestroy() {
        _deathInitiated = true;
        print("PreDestroy was called");
        // Ensures we cannot attack
        _attackTimeToWait = Time.time + int.MaxValue;
        Invoke("MoveToDeathSpot", 2f);
    }

    private void MoveToDeathSpot() {
        // Ensures we cannot move except to our death
        _moveTrigger = Time.time + int.MaxValue;
        _currentAttackPoint = DeathSpot.position;
        StartCoroutine(PlayFinalDeathAnimation());
    }

    private IEnumerator PlayFinalDeathAnimation() {
        yield return new WaitForSeconds(4f);
        Animator.SetBool("Death", true);
        yield return new WaitForSeconds(0.25f);
        Invoke("PlayDeathExplosion", 0f);
        Invoke("PlayDeathExplosion", 0.1f);
        Invoke("PlayDeathExplosion", 0.25f);
        Invoke("PlayDeathExplosion", 0.3f);
        Invoke("PlayDeathExplosion", 0.7f);
        _dead = true;
        GetComponent<SpriteRenderer>().sortingOrder = 0;
    }

    private void PlayDeathExplosion() {
        var clone = Instantiate(WeakspotExplosionPrefab, _weakspotExplosion.position, _weakspotExplosion.rotation);
        clone.GetComponent<SpriteRenderer>().sortingOrder = 10;
        clone.GetComponent<DeathTimer>().TimeToLive = 0.25f;
        clone.GetComponent<Animator>().SetBool("Invoke", true);
    }

    protected override void InvokeDestroy() {
        print("InvokeDestroy was called");
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
        if (_dead) {
            GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0f);
            return;
        }

        if (!CheckTargetsExist()) {
            return;
        }

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
        if (!_deathInitiated) {
            base.Update();
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

        CheckDamageTaken();
    }

    private void UpdateHeathbar() {
        if(_currentHealth.Health <= 0) {
            if(_healthChunks.Count == 0) {
                BaddieHudManager.Instance.HideHealthBar(_chunkIndex);
                Health = 0.0f;
                return;
            }else {
                print("adding new current health!");
                _currentHealth = _healthChunks.Pop();
                BaddieHudManager.Instance.HideHealthBar(_chunkIndex);
                ++_chunkIndex;
            }
        }
        print("Updating chunk: " + _chunkIndex + " with "+_currentHealth.Health + "/" + _currentHealth.MaxHealth);
        BaddieHudManager.Instance.SetHealthStatus(_chunkIndex, _currentHealth.Health, _currentHealth.MaxHealth);
    }

    private void CheckDamageTaken() {
        var currentDamageTaken = CalculateDamageTaken();
        if (_damageTaken != currentDamageTaken) {
            // update the damage to be the current and update layers
            UpdateAnimatorLayerByDamage(currentDamageTaken);
            _damageTaken = currentDamageTaken;
        }
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
            _defaultFireAngleAnimation = "Attack1-0";
            return FirePoint0;
        } else if (min == AttackHitDistances[1]) {
            _defaultFireAngleAnimation = "Attack1-45";
            return FirePoint45;
        } else {
            _defaultFireAngleAnimation = "Attack1-90";
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

        Animator.SetBool(_defaultFireAngleAnimation, true);
        for (var i = 0; i < Random.Range(10, 25); ++i) {
            Invoke("Fire", fireTime);
            fireTime += 0.075f;
        }

        Invoke("DeactivateDefaultAttackAnimation", fireTime - 0.075f);

        Dattack = false;
        _dAttackInitiated = false;
    }

    private void DeactivateDefaultAttackAnimation() {
        Animator.SetBool(_defaultFireAngleAnimation, false);
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
        _delayBetweenSpecialAttacks = Time.time + Random.Range(10, 15);
    }

    private DamageAmount CalculateDamageTaken() {
        if(_healthChunks.Count == 0) {
            return DamageAmount.ALL;
        } else if(_healthChunks.Count == 1) {
            return DamageAmount.SOME;
        } else {
            return DamageAmount.NONE;
        }
    }

    private void UpdateAnimatorLayerByDamage(DamageAmount newDamageLayer) {
        print("Updating layer " + _damageTaken + " to 0.0 and layer " + newDamageLayer + " to 1");
        if(_damageTaken != DamageAmount.NONE) {
            Animator.SetLayerWeight((int)_damageTaken, 0.0f);
        }
        Animator.SetLayerWeight((int)newDamageLayer, 1.0f);

    }

    private void DetermineNextAttackType() {
        _currentAttackType = AttackType.DEFAULT;

        if (Time.time <= _delayBetweenSpecialAttacks) {
            return;
        }

        var rand = Random.Range(0, 11);

        if(_healthChunks.Count == 2) {
            if(rand > 4) {
                _currentAttackType = GenerateAttackType();
            }
            // Normal Default speed
        } else if (_healthChunks.Count == 1) {
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
        Random.InitState((int)Time.time);
        var rand = Random.Range(0, 10);
        print("Random : " + rand);
        var newAttack = rand % 2 == 0 ? AttackType.VERTICAL : AttackType.HORIZONAL;
        if(_lastTwoAttacks[_lastAttackIndex] == newAttack) {
            if(_lastAttackIndex == _lastTwoAttacks.Length - 1) {
                _lastTwoAttacks[1] = AttackType.DEFAULT;
                _lastTwoAttacks[0] = GetOppositeAttackType(newAttack);
            }
            _lastTwoAttacks[++_lastAttackIndex] = newAttack;
        }
        _lastTwoAttacks[1] = AttackType.DEFAULT;
        _lastTwoAttacks[0] = newAttack;
        _lastAttackIndex = 0;
        return _lastTwoAttacks[_lastAttackIndex];
    }

    private AttackType GetOppositeAttackType(AttackType attack) {
        return attack == AttackType.VERTICAL ? AttackType.HORIZONAL : AttackType.VERTICAL;
    }

    public override bool Damage(float damage) {
        _currentHealth.Health -= (damage * DamageMultiplier);
        if(DamageMultiplier > 1) {
            print("Play massive weakspot explosion!");
            var clone = Instantiate(WeakspotExplosionPrefab, _weakspotExplosion.position, _weakspotExplosion.rotation);
            clone.GetComponent<SpriteRenderer>().sortingOrder = 10;
            clone.GetComponent<DeathTimer>().TimeToLive = 0.5f;
            clone.GetComponent<Animator>().SetBool("Invoke", true);
        } else {
            ActivateFlash();
        }
        UpdateHeathbar();
        return _currentHealth.Health <= 0 && _healthChunks.Count == 0;
    }
}
