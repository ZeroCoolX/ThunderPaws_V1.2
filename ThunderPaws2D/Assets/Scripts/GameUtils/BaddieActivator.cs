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

    //private void Apply(Vector3 v, Collider2D c) {
    //    var baddieScript = c.transform.GetComponent<DamageableLifeform>();
    //    var lifeformControllerScript = c.transform.GetComponent<LifetimeController>();
    //        if (!baddieScript.enabled) {
    //            baddieScript.enabled = true;
    //        }
    //    if (lifeformControllerScript!=null && !lifeformControllerScript.enabled) {
    //        lifeformControllerScript.enabled = true;
    //    }
    //}

    private void Apply(Vector3 v, Collider2D c) {
        c.transform.GetComponent<BaddieSpawn>().enabled = true;
    }
}
