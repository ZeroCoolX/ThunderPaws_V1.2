using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalHeavyAttack : MonoBehaviour {

    public Animator Animator;
    public Transform AttackPoint;
    public Transform MovementIndicator;

    public delegate void AttackComplete();
    public AttackComplete OnComplete;

    private SimpleCollider SpecialDamageCollider;

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
    private bool _standupCalled = false;
    private bool _testIt = false;
    public Vector3 _startSmashPos;

    private bool _stateChangeInitiated = false;
    private bool _contactedPlayer = false;

    private enum AttackState { DEFAULT, RISE, TRACK, SMASH, PAUSE, END, WEAK, STAND }
    private AttackState _attackState = AttackState.DEFAULT;

    private void Start() {
        MovementIndicator.gameObject.SetActive(false);
        var bossBaddieScript = transform.GetComponent<BaddieBoss>();
        _target = bossBaddieScript.GetTarget();
        bossBaddieScript.PlayVerticalHeavyAttack += InitiateAttack;

        SpecialDamageCollider = transform.GetComponent<SimpleCollider>();
        if(SpecialDamageCollider == null) {
            throw new MissingComponentException("Simple Collider for special damage is missing!");
        }
        SpecialDamageCollider.InvokeCollision += Apply;
        SpecialDamageCollider.Initialize(1 << 11, 6, true);
        SpecialDamageCollider.enabled = false;


    }

    private void InitiateAttack() {
        if (!_attack) {
            _attack = true;
            Animator.SetBool("Attack2_UP", true);
            Animator.SetBool("Attack2_UP_FLY", true);
            StartCoroutine(ChangeStateAfterSeconds(AttackState.RISE, 0.25f));
        }
    }

    public void Apply(Vector3 v, Collider2D c) {
        print("Apply special damage!");
    }


    public void CheckForPlayerContact() {
        if (!_contactedPlayer && _attackState != AttackState.END && _attackState != AttackState.DEFAULT) {
            print("Checking collision");
            var leftCorner = new Vector3(transform.position.x - 1, transform.position.y, transform.position.z);
            var rightCorner = new Vector3(transform.position.x + 1, transform.position.y, transform.position.z);
            RaycastHit2D hitLeft = Physics2D.Raycast(leftCorner, Vector2.down, 2, 1 << 8);
            RaycastHit2D hitMiddle = Physics2D.Raycast(transform.position, Vector2.down, 2, 1 << 8);
            RaycastHit2D hitRight = Physics2D.Raycast(rightCorner, Vector2.down, 2, 1 << 8);
            Debug.DrawRay(leftCorner, Vector2.down * 2, Color.green);
            Debug.DrawRay(transform.position, Vector2.down * 2, Color.green);
            Debug.DrawRay(rightCorner, Vector2.down * 2, Color.green);

            _contactedPlayer = (hitLeft.collider != null || hitMiddle.collider != null || hitRight.collider != null);
            if (_contactedPlayer) {
                StopAllCoroutinesAndStand();
            }
            print("contacted player is : " + _contactedPlayer);
        }
    }

    private void StopAllCoroutinesAndStand() {
        SpecialDamageCollider.enabled = false;
        StopAllCoroutines();
        StartCoroutine(ChangeStateAfterSeconds(AttackState.STAND, 0));
    }

    private void ResetState() {
        _attack = false;
        smoothTime = 1f;
        _standupCalled = false;
        _smashLocked = false;
        ResetAllAnimations();
        MovementIndicator.GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0f);
        _attackState = AttackState.DEFAULT;
        _stateChangeInitiated = false;
        _contactedPlayer = false;
        SpecialDamageCollider.enabled = false;
    }

    private void ResetAllAnimations() {
        Animator.SetBool("Attack2_UP", false);
        Animator.SetBool("Attack2_DOWN", false);
        Animator.SetBool("Attack2_STANDUP", false);
        Animator.SetBool("Attack2_WEAK", false);
        Animator.SetBool("Attack2_SMASH", false);
        Animator.SetBool("Attack2_UP_FLY", false);
    }

    private void Update() {
        CheckForPlayerContact();
        switch (_attackState) {
            case AttackState.RISE:
                Animator.SetBool("Attack2_UP", true);
                Animator.SetBool("Attack2_UP_FLY", true);
                _currentAttackPoint = AttackPoint.position;
                if (!_stateChangeInitiated) {
                    _stateChangeInitiated = true;
                    StartCoroutine(ChangeStateAfterSeconds(AttackState.TRACK, 2f));
                }
                break;
            case AttackState.TRACK:
                if (!MovementIndicator.gameObject.activeSelf) {
                    MovementIndicator.gameObject.SetActive(true);
                }
                    _currentAttackPoint.x = transform.GetComponent<BaddieBoss>().GetTarget().position.x;
                    MovementIndicator.transform.position = new Vector3(_currentAttackPoint.x, MovementIndicator.transform.position.y, MovementIndicator.transform.position.z);
                if (!_stateChangeInitiated) {
                    _stateChangeInitiated = true;
                    StartCoroutine(ChangeStateAfterSeconds(AttackState.PAUSE, 5f));
                }
                break;
            case AttackState.SMASH:
                Animator.SetBool("Attack2_DOWN", true);
                Animator.SetBool("Attack2_UP_FLY", false);
                Animator.SetBool("Attack2_UP", false);
                if (!_smashLocked) {
                    MovementIndicator.gameObject.SetActive(false);
                    _currentAttackPoint = new Vector3(_currentAttackPoint.x, transform.GetComponent<BaddieBoss>().GetTarget().position.y + 1, _currentAttackPoint.z);
                    _smashLocked = true;
                    _startSmashPos = transform.position;
                    _testIt = true;
                }
                if (!_stateChangeInitiated) {
                    _stateChangeInitiated = true;
                    smoothTime = 0.1f;
                    StartCoroutine(ChangeStateAfterSeconds(AttackState.WEAK, 0.25f));
                }
                break;
            case AttackState.PAUSE:
                Flash();
                if (!_stateChangeInitiated) {
                    _stateChangeInitiated = true;
                    StartCoroutine(ChangeStateAfterSeconds(AttackState.SMASH, 0.25f));
                }
                break;
            case AttackState.WEAK:
                _testIt = false;
                Animator.SetBool("Attack2_DOWN", false);
                Animator.SetBool("Attack2_SMASH", true);
                Animator.SetBool("Attack2_WEAK", true);

                if (!_stateChangeInitiated) {
                    SpecialDamageCollider.enabled = true;
                    _stateChangeInitiated = true;
                    StartCoroutine(ChangeStateAfterSeconds(AttackState.STAND, 3f));
                }
                break;
            case AttackState.STAND:
                Animator.SetBool("Attack2_SMASH", false);
                Animator.SetBool("Attack2_WEAK", false);
                if (!_standupCalled) {
                    Animator.SetBool("Attack2_STANDUP", true);
                    _standupCalled = true;
                } else {
                    Animator.SetBool("Attack2_STANDUP", false);
                }
                if (!_stateChangeInitiated) {
                    SpecialDamageCollider.enabled = false;
                    _stateChangeInitiated = true;
                    StartCoroutine(ChangeStateAfterSeconds(AttackState.END, 0.5f));
                }
                break;
            case AttackState.END:
                Animator.SetBool("Attack2_STANDUP", false);
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
        // Extra check to see if we hit the player - if we did we want to move immediately to the End
        _stateChangeInitiated = false;
        print("State = " + state);
        _attackState = state;
    }

    private void CalculateVelocity() {
        float newX = Mathf.SmoothDamp(transform.position.x, _currentAttackPoint.x, ref yVelocity, smoothTime);
        float newY = Mathf.SmoothDamp(_testIt ? _startSmashPos.y : transform.position.y, _currentAttackPoint.y, ref xVelocity, smoothTime);
        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}
