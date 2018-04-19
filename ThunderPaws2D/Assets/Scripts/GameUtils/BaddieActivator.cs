using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieActivator : MonoBehaviour {

    /// <summary>
    /// Necessary for collisions
    /// </summary>
    private SimpleCollider Collider;

    // Use this for initialization
    void Start() {
        //Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << 14, 25, true);
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 25);
    }


    private void Apply(Vector3 v, Collider2D c) {
        print("Collided with baddie: " + c.gameObject.name);
        var baddieScript = c.transform.GetComponent<DamageableLifeform>();
            if (!baddieScript.enabled) {
                print("Activated Baddie: " + c.gameObject.name);
                baddieScript.enabled = true;
            } else {
                print("Baddie: " + c.gameObject.name + " is already activated.");
            }
    }
}
