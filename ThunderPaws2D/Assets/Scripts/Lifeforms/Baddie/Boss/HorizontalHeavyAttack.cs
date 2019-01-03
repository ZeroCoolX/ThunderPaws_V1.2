using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalHeavyAttack : MonoBehaviour {
    public Animator Animator;
    public Transform[] AttackPoints;

    public delegate void AttackComplete();
    public AttackComplete OnComplete;

    public delegate void ApplySpecialDamageModifier(int multiplier);
    public ApplySpecialDamageModifier ApplyDamageModifierForWeakSpot;

    public delegate void LockFacingDirectionDelegate(bool locked);
    public LockFacingDirectionDelegate ToggleFacingLock;

    public delegate void CameraShakeDelegate();
    public CameraShakeDelegate ShakeCamera;

    private float smoothTime = 1F;
    private float yVelocity = 0.3F;
    private float xVelocity = 0.3F;
    private Vector3 _currentAttackPoint;
    private Vector3 _chargeAttackPoint;
    private Vector2 _attackDirection;
    private bool _attack;
    private bool _flashOn;
    private bool _standupCalled = false;
    private bool _stateChangeInitiated = false;

    private bool _contactedPlayer = false;

    private Transform _player;

    private enum AttackState { DEFAULT, TRAVEL, POWERUP, CHARGE, END, WALLSMASH, WEAK, STAND, PIN }
    private AttackState _attackState = AttackState.DEFAULT;

    private void Start() {
        var bossBaddieScript = transform.GetComponent<BaddieBoss>();
        bossBaddieScript.PlayHorizontalHeavyAttack += InitiateAttack;
        _attackState = AttackState.DEFAULT;
    }


    public void CheckForPlayerContact() {
        if (!_contactedPlayer && _attackState != AttackState.END && _attackState != AttackState.DEFAULT && _attackState != AttackState.TRAVEL) {
            print("Checking collision");
            var leftCorner = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
            var rightCorner = new Vector3(transform.position.x, transform.position.y - 1.5f, transform.position.z);
            RaycastHit2D hitLeft = Physics2D.Raycast(leftCorner, _attackDirection, 2, 1 << 8);
            RaycastHit2D hitRight = Physics2D.Raycast(rightCorner, _attackDirection, 2, 1 << 8);
            Debug.DrawRay(leftCorner, _attackDirection * 2, Color.red);
            Debug.DrawRay(rightCorner, _attackDirection * 2, Color.red);

            _contactedPlayer = (hitLeft.collider != null || hitRight.collider != null);
            if (_contactedPlayer) {
                var hit = hitLeft.collider != null ? hitLeft : hitRight;
                // We know its the player because the player is the only thing on layer we're checking
                _player = hit.collider.transform;
                hit.collider.transform.GetComponent<Player>().Damage(100);
                // play explosion;
            }
            print("contacted player is : " + _contactedPlayer);
        }
    }

    private bool ShouldDragPlayer() {
        var drag = false;
        switch (_attackState) {
            case AttackState.CHARGE:
            case AttackState.WALLSMASH:
            case AttackState.WEAK:
                drag = true;
                break;
        }
        return drag && _contactedPlayer;
    }

    private void Update() {
        if (transform.GetComponent<BaddieBoss>().GetTarget() != null) {
            CheckForPlayerContact();
            if (ShouldDragPlayer()) {
                _player.transform.position = transform.position;
            }
        }
        switch (_attackState) {
            case AttackState.TRAVEL:
                smoothTime = 0.5f;
                if (!_stateChangeInitiated) {
                    _currentAttackPoint = GetClosestAttackPoint();
                    StartCoroutine(ChangeStateAfterSeconds(AttackState.POWERUP, 2f));
                    _stateChangeInitiated = true;
                }
                break;
            case AttackState.POWERUP:
                if (!_stateChangeInitiated) {
                    AudioManager.Instance.PlaySound("PowerUp");
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
                    AudioManager.Instance.StopSound("PowerUp");
                    AudioManager.Instance.PlaySound("Charge");
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
                    _stateChangeInitiated = true;
                    ShakeCamera.Invoke();
                    PlaySmashSound();
                    if (_contactedPlayer) {
                        Animator.SetBool("Attack3_WALLSMASH", true);
                        StartCoroutine(ChangeStateAfterSeconds(AttackState.PIN, 0.3f));
                    } else {
                        Animator.SetBool("Attack3_WALLSMASH", true);
                        Animator.SetBool("Attack3_BOUNCEBACK", true);
                        StartCoroutine(ChangeStateAfterSeconds(AttackState.WEAK, 0.5f));
                    }
                }
                break;
            case AttackState.PIN:
                smoothTime = 1f;
                if (!_stateChangeInitiated) {
                    Animator.SetBool("Attack3_WALLSMASH", false);
                    Animator.SetBool("Attack3_PIN", true);
                    StartCoroutine(ChangeStateAfterSeconds(AttackState.STAND, 3f));
                    _stateChangeInitiated = true;
                }
                break;
            case AttackState.WEAK:
                smoothTime = 1f;
                if (!_stateChangeInitiated) {
                    ApplyDamageModifierForWeakSpot.Invoke(8);
                    Animator.SetBool("Attack3_BOUNCEBACK", false);
                    Animator.SetBool("Attack3_WALLSMASH", false);
                    Invoke("ToggleLockFacingBackOn", 0.1f);
                    Animator.SetBool("Attack3_WEAK", true);
                    StartCoroutine(ChangeStateAfterSeconds(AttackState.STAND, 3f));
                    _stateChangeInitiated = true;
                }
                break;
            case AttackState.STAND:
                Animator.SetBool("Attack3_WEAK", false);
                Animator.SetBool("Attack3_PIN", false);
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

    private void PlaySmashSound() {
        AudioManager.Instance.PlaySound("CrashBase");
        var randCrash = Random.Range(0, 10) % 2 == 0 ? 1 : 2;
        AudioManager.Instance.PlaySound(("Crash" + randCrash));
    }


    private Vector3 GetClosestAttackPoint() {
        var attackStart = Random.Range(0, 10) % 2 == 0? 0 : 1;
        var attackEnd = attackStart > 0 ? 0 : 1;
        if(attackStart == 0) {
            _attackDirection = Vector2.left;
        }else {
            _attackDirection = Vector2.right;
        }
        _chargeAttackPoint = AttackPoints[attackEnd].position;
        return AttackPoints[attackStart].position;
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
        if(AttackState.WEAK == state && _contactedPlayer) {
            ApplyDamageModifierForWeakSpot.Invoke(1);
            ResetAllAnimations();
            // Instead we want to immediately get back up!
            state = AttackState.END;
        }
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
        _contactedPlayer = false;
    }

    private void ResetAllAnimations() {
        Animator.SetBool("Attack3_CROUCH", false);
        Animator.SetBool("Attack3_CHARGEUP", false);
        Animator.SetBool("Attack3_DASH", false);
        Animator.SetBool("Attack3_WALLSMASH", false);
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
