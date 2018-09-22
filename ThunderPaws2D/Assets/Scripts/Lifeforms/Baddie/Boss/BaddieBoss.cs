using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieBoss : BaddieLifeform {
    public Transform[] Attack1Points;

    [Header("Play Vertical Attack")]
    public bool Vattack = false;
    private bool _vAttackInitiated = false;
    [Header("Play Horizontal Attack")]
    public bool Hattack = false;
    private bool _hAttackInitiated = false;

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
        if (_hAttackInitiated) {
            return;
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
}
