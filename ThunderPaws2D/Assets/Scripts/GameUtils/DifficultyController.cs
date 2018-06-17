using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyController : MonoBehaviour {
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
        Collider.Initialize(1 << 8, 2, true);
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 2);
    }


    private void Apply(Vector3 v, Collider2D c) {
        print("You are choosing Difficulty: " + gameObject.name);
        GameMasterV2.Instance.Difficulty = gameObject.name.ToLower();
    }
}
