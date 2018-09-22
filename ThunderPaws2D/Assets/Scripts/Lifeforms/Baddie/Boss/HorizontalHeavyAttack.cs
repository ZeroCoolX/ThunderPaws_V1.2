﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalHeavyAttack : MonoBehaviour {
    public Animator Animator;
    public Transform[] AttackPoints;

    public delegate void AttackComplete();
    public AttackComplete OnComplete;

    public delegate void LockFacingDirectionDelegate(bool locked);
    public LockFacingDirectionDelegate ToggleFacingLock;

    private float smoothTime = 1F;
    private float yVelocity = 0.3F;
    private float xVelocity = 0.3F;
    private Vector3 _currentAttackPoint;
    private Vector3 _chargeAttackPoint;
    private bool _attack;
    private bool _flashOn;
    private bool _standupCalled = false;
    private bool _stateChangeInitiated = false;

    private enum AttackState { DEFAULT, TRAVEL, POWERUP, CHARGE, END, WALLSMASH, WEAK, STAND }
    private AttackState _attackState = AttackState.DEFAULT;

    private void Start() {
        var bossBaddieScript = transform.GetComponent<BaddieBoss>();
        bossBaddieScript.PlayHorizontalHeavyAttack += InitiateAttack;
        _attackState = AttackState.DEFAULT;
    }

    private void Update() {
        switch (_attackState) {
            case AttackState.TRAVEL:
                _currentAttackPoint = GetClosestAttackPoint();
                smoothTime = 0.5f;
                if (!_stateChangeInitiated) {
                    StartCoroutine(ChangeStateAfterSeconds(AttackState.POWERUP, 2f));
                    _stateChangeInitiated = true;
                }
                break;
            case AttackState.POWERUP:
                if (!_stateChangeInitiated) {
                    ToggleFacingLock.Invoke(false);
                    Animator.SetBool("Attack3_CROUCH", true);
                    Animator.SetBool("Attack3_CHARGEUP", true);
                    StartCoroutine(ChangeStateAfterSeconds(AttackState.CHARGE, 3f));
                    _stateChangeInitiated = true;
                }
                break;
            case AttackState.CHARGE:
                Animator.SetBool("Attack3_CROUCH", false);
                Animator.SetBool("Attack3_CHARGEUP", false);
                _currentAttackPoint = _chargeAttackPoint;
                smoothTime = 0.2f;
                if (!_stateChangeInitiated) {
                    Animator.SetBool("Attack3_DASH", true);
                    StartCoroutine(ChangeStateAfterSeconds(AttackState.WALLSMASH, 0.3f));
                    _stateChangeInitiated = true;
                }
                break;
            case AttackState.END:
                ResetState();
                OnComplete.Invoke();
                break;
            case AttackState.WALLSMASH:
                smoothTime = 0.3f;
                if (!_stateChangeInitiated) {
                    Animator.SetBool("Attack3_DASH", false);
                    Animator.SetBool("Attack3_WALLSMASH", true);
                    Animator.SetBool("Attack3_BOUNCEBACK", true);
                    StartCoroutine(ChangeStateAfterSeconds(AttackState.WEAK, 0.5f));
                    _stateChangeInitiated = true;
                }
                break;
            case AttackState.WEAK:
                smoothTime = 1f;
                if (!_stateChangeInitiated) {
                    Animator.SetBool("Attack3_WALLSMASH", false);
                    Animator.SetBool("Attack3_BOUNCEBACK", false);
                    Invoke("ToggleLockFacingBackOn", 0.1f);
                    Animator.SetBool("Attack3_WEAK", true);
                    StartCoroutine(ChangeStateAfterSeconds(AttackState.STAND, 3f));
                    _stateChangeInitiated = true;
                }
                break;
            case AttackState.STAND:
                Animator.SetBool("Attack3_WEAK", false);
                if (!_standupCalled) {
                    Animator.SetBool("Attack3_STANDUP", true);
                    _standupCalled = true;
                } else {
                    Animator.SetBool("Attack3_STANDUP", false);
                }
                if (!_stateChangeInitiated) {
                    StartCoroutine(ChangeStateAfterSeconds(AttackState.END, 1f));
                    _stateChangeInitiated = true;
                }
                break;
            case AttackState.DEFAULT:
            default:
                return;
        }
        CalculateVelocity();
    }

    private void ToggleLockFacingBackOn() {
        ToggleFacingLock.Invoke(true);
    }


    private Vector3 GetClosestAttackPoint() {
        //float point1Distance;
        //float point2Distance;

        //if (Mathf.Sign(transform.position.x) != Mathf.Sign(AttackPoints[0].position.x)) {
        //    point1Distance = transform.position.x + AttackPoints[0].position.x;
        //}else {
        //    point1Distance = Mathf.Abs(transform.position.x - AttackPoints[0].position.x);
        //}

        //if (Mathf.Sign(transform.position.x) != Mathf.Sign(AttackPoints[1].position.x)) {
        //    point2Distance = transform.position.x + AttackPoints[1].position.x;
        //} else {
        //    point2Distance = Mathf.Abs(transform.position.x - AttackPoints[1].position.x);
        //}

        //if (point1Distance < point2Distance) {
        //    _chargeAttackPoint = AttackPoints[1].position;
        //    return AttackPoints[0].position;
        //} else {
        //    _chargeAttackPoint = AttackPoints[0].position;
        //    return AttackPoints[1].position;
        //}
        _chargeAttackPoint = AttackPoints[1].position;
        return AttackPoints[0].position;
    }

    private void Flash() {
        if (_flashOn) {
            GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0.8f);
            _flashOn = false;
        } else {
            GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0f);
            _flashOn = true;
        }
    }

    private IEnumerator ChangeStateAfterSeconds(AttackState state, float afterSeconds) {
        yield return new WaitForSeconds(afterSeconds);
        _stateChangeInitiated = false;
        _attackState = state;
    }

    private void InitiateAttack() {
        if (!_attack) {
            _attack = true;
            _attackState = AttackState.TRAVEL;
        }
    }

    private void ResetState() {
        _attack = false;
        _standupCalled = false;
        smoothTime = 1f;
        ResetAllAnimations();
        GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0f);
        _attackState = AttackState.DEFAULT;
    }

    private void ResetAllAnimations() {
        Animator.SetBool("Attack3_CROUCH", false);
        Animator.SetBool("Attack3_CHARGEUP", false);
        Animator.SetBool("Attack3_DASH", false);
        Animator.SetBool("Attack3_WALLSMASH", false);
        Animator.SetBool("Attack3_RECOVER", false);
        Animator.SetBool("Attack3_BOUNCEBACK", false);
        Animator.SetBool("Attack3_WEAK", false);
        Animator.SetBool("Attack3_STANDUP", false);
    }

    private void CalculateVelocity() {
        float newX = Mathf.SmoothDamp(transform.position.x, _currentAttackPoint.x, ref yVelocity, smoothTime);
        float newY = Mathf.SmoothDamp(transform.position.y, _currentAttackPoint.y, ref xVelocity, smoothTime);
        transform.position = new Vector3(newX, newY, transform.position.z);
    }

}
