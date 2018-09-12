using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalHeavyAttack : MonoBehaviour {

    public Transform AttackPoint;
    public Transform MovementIndicator;

    public delegate void AttackComplete();
    public AttackComplete OnComplete;

    private Transform _target;
    private float vAttackTimeBeforeSmash = 5f;
    private float smoothTime = 1F;
    private float yVelocity = 0.3F;
    private float xVelocity = 0.3F;
    private Vector3 _currentAttackPoint;
    private float _moveSpeed = 1f;
    private float _moveTrigger;
    private float _delayBetweenMoves = 1f;

    private bool _attack;
    private bool _smashLocked = false;
    private bool _flashOn = false;

    private enum AttackState { DEFAULT, RISE, TRACK, SMASH, PAUSE, END }
    private AttackState _attackState = AttackState.DEFAULT;

    private void Start() {
        MovementIndicator.gameObject.SetActive(false);
        var bossBaddieScript = transform.GetComponent<BaddieBoss>();
        _target = bossBaddieScript.GetTarget();
        bossBaddieScript.PlayVerticalHeavyAttack += InitiateAttack;
    }

    private void InitiateAttack() {
        if (!_attack) {
            _attack = true;
            _attackState = AttackState.RISE;
        }
    }

    private void ResetState() {
        _attack = false;
        smoothTime = 1f;
        _smashLocked = false;
        MovementIndicator.GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0f);
        _attackState = AttackState.DEFAULT;
    }

    private void Update() {
        switch (_attackState) {
            case AttackState.RISE:
                _currentAttackPoint = AttackPoint.position;
                StartCoroutine(ChangeStateAfterSeconds(AttackState.TRACK, 2f));
                break;
            case AttackState.TRACK:
                if (!MovementIndicator.gameObject.activeSelf) {
                    MovementIndicator.gameObject.SetActive(true);
                }
                _currentAttackPoint.x = transform.GetComponent<BaddieBoss>().GetTarget().position.x;
                MovementIndicator.transform.position = new Vector3(_currentAttackPoint.x, MovementIndicator.transform.position.y, MovementIndicator.transform.position.z);
                StartCoroutine(ChangeStateAfterSeconds(AttackState.PAUSE, 5f));
                break;
            case AttackState.SMASH:
                if(!_smashLocked) {
                    MovementIndicator.gameObject.SetActive(false);
                    _currentAttackPoint = new Vector3(_currentAttackPoint.x, transform.GetComponent<BaddieBoss>().GetTarget().position.y + 1, _currentAttackPoint.z);
                    _smashLocked = true;
                }
                smoothTime = 0.3f;
                StartCoroutine(ChangeStateAfterSeconds(AttackState.END, 0.1f));
                break;
            case AttackState.PAUSE:
                Flash();
                StartCoroutine(ChangeStateAfterSeconds(AttackState.SMASH, 0.5f));
                break;
            case AttackState.END:
                ResetState();
                OnComplete.Invoke();
                break;
            case AttackState.DEFAULT:
            default:
                return;
        }
        CalculateVelocity();
    }

    private void Flash() {
        if (_flashOn) {
            MovementIndicator.GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0.8f);
            _flashOn = false;
        } else {
            MovementIndicator.GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0f);
            _flashOn = true;
        }
    }

    private IEnumerator ChangeStateAfterSeconds(AttackState state, float afterSeconds) {
        yield return new WaitForSeconds(afterSeconds);
        _attackState = state;
    }

    private void CalculateVelocity() {
        float newX = Mathf.SmoothDamp(transform.position.x, _currentAttackPoint.x, ref yVelocity, smoothTime);
        float newY = Mathf.SmoothDamp(transform.position.y, _currentAttackPoint.y, ref xVelocity, smoothTime);
        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}
