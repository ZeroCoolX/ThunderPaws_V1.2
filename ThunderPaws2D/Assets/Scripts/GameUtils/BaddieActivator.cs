using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieActivator : MonoBehaviour {
    private SimpleCollider Collider;

    void Start() {
        // Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << 18, new Vector2(20, 20), true, GameObject.Find("Camera").transform);
    }

    private void Apply(Vector3 v, Collider2D c) {
        c.transform.GetComponent<BaddieSpawn>().enabled = true;
    }
}
