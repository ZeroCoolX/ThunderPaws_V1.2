﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCollider : MonoBehaviour {
    public delegate void InvokeCollisionDelegate(Vector3 pos, Collider2D collider);
    public InvokeCollisionDelegate InvokeCollision;

    public delegate void InvokeCollisionStoped();
    public InvokeCollisionStoped InvokeCollisionStopped;

    /// <summary>
    /// Indicates if we use base raycasts or Circle colliders
    /// </summary>
    private bool _useCircleCollider = false;
    /// <summary>
    /// If a circle collider is used, we want to raycast a circle the bounds of our circle plus a alittle more
    /// </summary>
    private float _radius;
    /// <summary>
    /// Use to indicate if we want to be notified once and only once for a collision or everytime there is a collision
    /// </summary>
    /// <returns></returns>
    private bool _continuousCollision;
    /// <summary>
    /// Optional list of objects NOT to collide with even if they're apart of the WhatToHit Layermask
    /// </summary>
    private List<string> _expemptFromCollision = new List<string>();

    private LayerMask _whatToHit;
    private Vector2 _targetDirection;
    private Vector3 _targetPos;
    private float _moveSpeed;
    private float _raycastLength;
    private bool _hit = false;
    private Transform _followCamera;// too much
    private Vector3 _positionLastFrame;//way too much
    private Vector3 _debugPosition;

    private Vector2 _areaSize;
    private bool _useAreaCollider = false;

    public void Initialize(LayerMask whatToHit, Vector2 areaSize, bool continuousCollision = false, Transform followCamera = null) { // too much
        _areaSize = areaSize;
        _whatToHit = whatToHit;
        _continuousCollision = continuousCollision;
        _useAreaCollider = true;
        _hit = false;
        _followCamera = followCamera;
        _positionLastFrame = transform.position;
    }

    public void Initialize(LayerMask whatToHit, float radius = 0f, bool continuousCollision = false) {
        _useCircleCollider = true;
        _radius = radius == 0 ? GetComponent<SpriteRenderer>().bounds.size.x : radius;
        _whatToHit = whatToHit;
        _continuousCollision = continuousCollision;
        _hit = false;
    }

    public void Initialize(LayerMask whatToHit, Vector2 targetDirection, Vector3 targetPos, float moveSpeed, string exemptions, float raycastLength = 0.2f) {
        _whatToHit = whatToHit;
        _targetDirection = targetDirection;
        _targetPos = targetPos;
        _moveSpeed = moveSpeed;
        _raycastLength = raycastLength;
        SetCollisionExemptions(exemptions);
    }

    public void SetCollisionExemptions(string exemptions) {
        if(string.IsNullOrEmpty(exemptions)) {
            return;
        }
        if(exemptions.IndexOf('|') < 0) {
            _expemptFromCollision.Add(exemptions);
            return;
        }
        foreach (var ex in exemptions.Split('|')) {
            _expemptFromCollision.Add(ex);
        }
    }
	
	void Update () {
        if (!_continuousCollision && _hit) {
            return;
        }
        if (_useCircleCollider) {
            CheckForMultiCircleCollisions();
        } else if(_useAreaCollider){
            CheckForMultiAreaCollisions();
        }else {
            CheckForRaycastCollisions();
        }
        if(_continuousCollision && !_hit && InvokeCollisionStopped != null) {
            InvokeCollisionStopped.Invoke();
        }
        if (_continuousCollision) {
            _hit = false;
        }
    }

    private void CheckForMultiCircleCollisions() {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _radius, _whatToHit);
        foreach (var collider in colliders) {
            if (collider != null && !_expemptFromCollision.Contains(collider.gameObject.tag)) {
                InvokeCollision.Invoke(transform.position, collider);
                _hit = true;
            }
        }
    }

    private void CheckForMultiAreaCollisions() {
        Vector3 position = transform.position;
        if(_followCamera != null) {
            float amountToShiftX = 0.5f * _areaSize.x;
            amountToShiftX *= (position.x < _positionLastFrame.x ? -1 : 1);
            position = new Vector3(position.x + amountToShiftX, position.y, position.z);
            _debugPosition = position;//debug purposes only!
        }
        _positionLastFrame = transform.position;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(position, _areaSize, 0, _whatToHit);
        foreach (var collider in colliders) {
            if (collider != null && !_expemptFromCollision.Contains(collider.gameObject.tag)) {
                InvokeCollision.Invoke(transform.position, collider);
                _hit = true;
            }
        }
    }

    void OnDrawGizmosSelected() {
        if(_followCamera) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_debugPosition, new Vector2(20, 20));
        }
    }

    private void CheckForRaycastCollisions() {
        // Raycast to check if we could potentially the target
        RaycastHit2D possibleHit = Physics2D.Raycast(transform.position, _targetDirection);
        if (possibleHit.collider != null) {
            RaycastHit2D distCheck = Physics2D.Raycast(transform.position, _targetDirection, _raycastLength, _whatToHit);

            if (ContactHasBeenMade(distCheck)) { 
                InvokeCollision.Invoke(transform.position, distCheck.collider);
                _hit = true;
            }

            // Last check is necessary as an extra mini raycast check to handle pinpoint precision collisions which feels better in game when dodging
            Vector3 dir = _targetPos - transform.position;
            float distanceThisFrame = _moveSpeed * Time.deltaTime;
            // Length of dir is distance to target. if thats less than distancethisframe we've already hit the target
            if (dir.magnitude <= distanceThisFrame) {
                // Make sure the target didn't dodge out of the way
                distCheck = Physics2D.Raycast(transform.position, _targetDirection, _raycastLength, _whatToHit);

                if (ContactHasBeenMade(distCheck)) {
                        InvokeCollision.Invoke(transform.position, distCheck.collider);
                    _hit = true;
                }
            }
        }
    }

    private bool ContactHasBeenMade(RaycastHit2D raycast) {
        return raycast.collider != null && !_expemptFromCollision.Contains(raycast.collider.gameObject.tag);
    }
}
