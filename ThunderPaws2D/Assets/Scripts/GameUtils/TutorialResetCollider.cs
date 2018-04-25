using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialResetCollider : MonoBehaviour {

    /// <summary>
    /// Necessary for collisions
    /// </summary>
    private SimpleCollider Collider;

    public int WhatToCollideWith = 8;
    public float SizeOfCollider = 10f;

    // Use this for initialization
    void Start() {
        //Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << WhatToCollideWith, SizeOfCollider, false);
    }

    //void OnDrawGizmosSelected() {
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawSphere(transform.position, 0.5f);
    //}


    private void Apply(Vector3 v, Collider2D c) {
        TutorialManager.Instance.TutorialFailedReset();
    }
}