
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LighteningClaw : Ultimate {
    private bool _activated = false;
    private Vector2 _pauseDamageTime = new Vector2(0, 0.1f);

    private Stack<GameObject> _flyingBaddies;
    private List<GameObject> _baddies;
    private GameObject _currentTarget;

    private float _xVelocity;
    private float _yVelocity;
    private float _smoothTime = 0.15F;

    private Vector2 _spritePlayerSize;
    private Vector3 _originalPlayerPosition;
    private BetterCameraFollow _cameraScript;

    private void Start() {
        _cameraScript = Camera.main.GetComponentInParent<BetterCameraFollow>();
    }

    public override void Activate() {
        print("LighteningClaw activated!");
        PlayerStats.UltEnabled = true;
        PlayerStats.UltReady = false;

        _originalPlayerPosition = transform.position;
        print("_originalPlayerPosition = " + _originalPlayerPosition);

        ResetCollections();
        StopAllMovement();
        CalculateColliderSize();
        CollectAllBaddies();

        _activated = true;
    }

    private void ResetCollections() {
        _flyingBaddies = new Stack<GameObject>();
        _baddies = new List<GameObject>();
    }

    private void StopAllMovement() {
        GetComponent<Player>().enabled = false;
        _cameraScript.enabled = false;
    }

    private void CalculateColliderSize() {
        var spriteSize = GetComponent<SpriteRenderer>().size;
        _spritePlayerSize = new Vector2(spriteSize.x / 4, spriteSize.y / 4);
        print("SpriteSize : " + spriteSize);
        print("_spritePlayerSize : " + _spritePlayerSize);
    }

    private void CollectAllBaddies() {
        var baddies = GameObject.FindGameObjectsWithTag(GameConstants.Tag_Baddie).Union(GameObject.FindGameObjectsWithTag(GameConstants.Tag_HordeBaddie));
        if (baddies == null || baddies.Count() == 0) {
            print("There were no baddies on screen");
            return;
        }
        _baddies = baddies.ToList();
        foreach (var baddie in _baddies) {
            if (baddie.gameObject.name.IndexOf("FL") != -1) {
                print("Adding baddie " + baddie.gameObject.name + " to stack");
                _flyingBaddies.Push(baddie);
            }
            baddie.GetComponent<BaddieLifeform>().enabled = false;
        }
    }

    private void ResumeBaddieMovement() {
        foreach (var baddie in _baddies) {
            var baddieLifeform = baddie.GetComponent<BaddieLifeform>();
            baddieLifeform.enabled = true;

            // Damage if it was a flying baddie
            if (baddie.gameObject.name.IndexOf("FL") != -1) {
                baddieLifeform.Damage(100);
            }
        }
    }

    void LateUpdate() {
        if (!_activated) {
            return;
        }

        if(Time.time < _pauseDamageTime.x) {
            return;
        }

        if (_currentTarget == null) {
            if (_flyingBaddies.Count() == 0) {
                ReturnToOriginAndTurnOff();
                return;
            }
            _currentTarget = _flyingBaddies.Pop();
        } else {
            CalculateVelocity(_currentTarget.transform.position);
            CheckForCollision();
        }
    }

    private void ReturnToOriginAndTurnOff() {
        CalculateVelocity(_originalPlayerPosition);

        if (ToIntVector(transform.position) == ToIntVector(_originalPlayerPosition)) {
            GetComponent<Player>().enabled = true;
            _cameraScript.enabled = true;
            ResumeBaddieMovement();
            ResetCollections();
            DeactivateDelegate.Invoke();
            _activated = false;
        }else {
            print("transform.position " + ToIntVector(transform.position)+ "!= " + ToIntVector(_originalPlayerPosition));
        }
    }

    private Vector3 ToIntVector(Vector3 ogVector) {
        return new Vector3((int)ogVector.x, (int)ogVector.y, (int)ogVector.z);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, _spritePlayerSize);
    }

    private void CheckForCollision() {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, _spritePlayerSize, 0, 1<<14);
        foreach (var collider in colliders) {
            if (collider != null && _currentTarget.gameObject.GetInstanceID() == collider.gameObject.GetInstanceID()) {
                print("Damage this baddie and move to the next!");
                _currentTarget = null;
                _pauseDamageTime.x = Time.time + _pauseDamageTime.y;
                return;
            }
        }
    }

    private void CalculateVelocity(Vector3 targetPos) {
        float newX = Mathf.SmoothDamp(transform.position.x, targetPos.x, ref _xVelocity, _smoothTime);
        float newY = Mathf.SmoothDamp(transform.position.y, targetPos.y, ref _yVelocity, _smoothTime);
        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}
