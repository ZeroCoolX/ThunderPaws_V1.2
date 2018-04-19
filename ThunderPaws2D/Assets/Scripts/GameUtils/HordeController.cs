using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeController : MonoBehaviour {
    /// <summary>
    /// Necessary for collisions
    /// </summary>
    private SimpleCollider Collider;
    /// <summary>
    /// Reference to the main camera
    /// </summary>
    public Transform Camera;

    // Use this for initialization
    void Start() {
        //Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << 8, 12);

    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 12);
    }

    /// <summary>
    /// The Player has walked into the horde zone.
    /// Active the baddie spawners!
    /// </summary>
    /// <param name="v"></param>
    /// <param name="c"></param>
    private void Apply(Vector3 v, Collider2D c) {
        var camFollow = Camera.GetComponent<Camera2DFollow>();
        camFollow.Target = transform;
        //Camera.position = transform.position;
    }
}
