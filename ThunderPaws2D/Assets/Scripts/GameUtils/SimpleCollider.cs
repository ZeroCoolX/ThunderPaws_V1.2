using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCollider : MonoBehaviour {
    /// <summary>
    /// Optional list of object NOT to collide with even if they're apart of the WhatToHit Layermask
    /// </summary>
    private List<string> _expemptFromCollision = new List<string>();
    /// <summary>
    /// Indicates what to collide with
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
    /// Flag so it won't run any Update frames if we hit something because we're about to be destroyed
    /// </summary>
    private bool _hit = false;

    public delegate void InvokeCollisionDelegate(Vector3 pos, Collider2D collider);
    public InvokeCollisionDelegate InvokeCollision;

    public void Initialize(LayerMask whatToHit, Vector2 targetDirection, Vector3 targetPos, float moveSpeed, string exemptions) {
        _whatToHit = whatToHit;
        _targetDirection = targetDirection;
        _targetPos = targetPos;
        _moveSpeed = moveSpeed;
        SetCollisionExemptions(exemptions);
    }

    public void SetCollisionExemptions(string exemptions) {
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
        if (!_hit) {
            CheckForCollisions();
        }
    }

    /// <summary>
    /// Check for collisions
    /// </summary>
    private void CheckForCollisions() {
        //Raycast to check if we could potentially the target
        RaycastHit2D possibleHit = Physics2D.Raycast(transform.position, _targetDirection);
        if (possibleHit.collider != null) {
            //Mini raycast to check handle ellusive targets
            RaycastHit2D distCheck = Physics2D.Raycast(transform.position, _targetDirection, 0.2f, _whatToHit);
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
                distCheck = Physics2D.Raycast(transform.position, _targetDirection, 0.2f, _whatToHit);
                //We want to allow bullets to pass throught obstacles that the player can pass through
                if (distCheck.collider != null  && !_expemptFromCollision.Contains(distCheck.collider.gameObject.tag)) {
                        InvokeCollision.Invoke(transform.position, distCheck.collider);
                    _hit = true;
                }
            }
        }
    }
}
