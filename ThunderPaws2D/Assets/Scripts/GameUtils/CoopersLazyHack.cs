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
        var lifeformScript = c.transform.GetComponent<AbstractLifeform>();
        if (lifeformScript != null) {
            lifeformScript.NoFallCheck = true;
        }
        //var playStats = c.transform.GetComponent<PlayerStats>();

        //playStats.MaxHealth = 1000;
        //playStats.CurrentHealth = playStats.MaxHealth;

        Camera.GetComponent<Camera2DFollow>().YPosClamp = -500f;
    }
}
