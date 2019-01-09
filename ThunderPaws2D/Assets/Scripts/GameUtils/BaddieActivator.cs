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
        Collider.Initialize(1 << 18, new Vector2(30, 20), true);
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector2(30, 20));
    }

    private void Apply(Vector3 v, Collider2D c) {
        c.transform.GetComponent<BaddieSpawn>().enabled = true;
    }
}
