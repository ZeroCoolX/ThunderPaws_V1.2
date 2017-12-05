using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyPickup : MonoBehaviour {

    private SimpleCollider Collider;

	// Use this for initialization
	void Start () {
        //Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += WeHitShit;
        Collider.Initialize(1 << 8);
    }
	
	private void WeHitShit(Vector3 v, Collider2D c) {
        print("OMG ITS WORKING");
    }
}
