using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoopersLazyHack : MonoBehaviour {

    /// <summary>
    /// Necessary for collisions
    /// </summary>
    private SimpleCollider Collider;
    public Transform Camera;

    void Start() {
        //Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << 8, 25, true);
    }

    private void Apply(Vector3 v, Collider2D c) {
        //print("Collided with baddie: " + c.gameObject.name);
        var lifeformScript = c.transform.GetComponent<PlayerLifeform>();
        if (lifeformScript != null) {
            lifeformScript.NoFallCheck = true;
        }

        Camera.GetComponent<Camera2DFollow>().YPosClamp = -500f;
    }
}
