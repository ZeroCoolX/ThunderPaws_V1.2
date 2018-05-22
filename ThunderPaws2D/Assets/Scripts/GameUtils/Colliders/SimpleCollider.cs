﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCollider : MonoBehaviour {
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
    /// Optional list of object NOT to collide with even if they're apart of the WhatToHit Layermask
    /// </summary>
    private List<string> _expemptFromCollision = new List<string>();
    /// <summary>
    /// Indicates what to collide with.
    /// Defaults to the Player layer
    /// </summary>
    private LayerMask _whatToHit;
    /// <summary>
    /// In what direction should we raycast
    /// </summary>
    private Vector2 _targetDirection;
    /// <summary>
    /// If we know the targets position this is used for super small raycast double check
    /// </summary>
    private Vector3 _targetPos;
    /// <summary>
    /// If set this is how fasat we're moving
    /// </summary>
    private float _moveSpeed;
    /// <summary>
    /// If set this is how far our we raycast
    /// </summary>
    private float _raycastLength;
    /// <summary>
    /// Flag so it won't run any Update frames if we hit something because we're about to be destroyed
    /// </summary>
    private bool _hit = false;

    public delegate void InvokeCollisionDelegate(Vector3 pos, Collider2D collider);
    public InvokeCollisionDelegate InvokeCollision;

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
	
	// Update is called once per frame
	void Update () {
        if (!_continuousCollision && _hit) {
            return;
        }
        if (_useCircleCollider) {
            if (_continuousCollision) {
                CheckForMultiCircleCollisions();
            } else {
                CheckForCircleCollisions();
            }
        } else {
            CheckForRaycastCollisions();
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

    private void CheckForCircleCollisions() {
        Collider2D collider = Physics2D.OverlapCircle(transform.position, _radius, _whatToHit);
        if (collider != null && !_expemptFromCollision.Contains(collider.gameObject.tag)) {
            InvokeCollision.Invoke(transform.position, collider);
            _hit = true;
        }
    }

    /// <summary>
    /// Check for collisions
    /// </summary>
    private void CheckForRaycastCollisions() {
        //Raycast to check if we could potentially the target
        RaycastHit2D possibleHit = Physics2D.Raycast(transform.position, _targetDirection);
        if (possibleHit.collider != null) {
            //Mini raycast to check handle ellusive targets
            RaycastHit2D distCheck = Physics2D.Raycast(transform.position, _targetDirection, _raycastLength, _whatToHit);
            //We want to allow bullets to pass throught obstacles that the player can pass through
            if (distCheck.collider != null && !_expemptFromCollision.Contains(distCheck.collider.gameObject.tag)) { 
                InvokeCollision.Invoke(transform.position, distCheck.collider);
                _hit = true;
            }

            //Last check is simplest check
            Vector3 dir = _targetPos - transform.position;
            float distanceThisFrame = _moveSpeed * Time.deltaTime;
            //Length of dir is distance to target. if thats less than distancethisframe we've already hit the target
            if (dir.magnitude <= distanceThisFrame) {
                //Make sure the player didn't dodge out of the way
                distCheck = Physics2D.Raycast(transform.position, _targetDirection, _raycastLength, _whatToHit);
                //We want to allow bullets to pass throught obstacles that the player can pass through
                if (distCheck.collider != null  && !_expemptFromCollision.Contains(distCheck.collider.gameObject.tag)) {
                        InvokeCollision.Invoke(transform.position, distCheck.collider);
                    _hit = true;
                }
            }
        }
    }
}