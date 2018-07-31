using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for weapons that emit extra Area Of Effect (AOE) damage like the Gauss Rifle
/// </summary>
public class AoeDamageController : MonoBehaviour {
    public float AoeDamageRadius;
    public float Damage;
    private SimpleCollider aoeDamageCollider;

	void Start () {
        // Add delegate for collision detection
        aoeDamageCollider = transform.GetComponent<SimpleCollider>();
        if (aoeDamageCollider == null) {
            throw new MissingComponentException("No collider for the aoe damage effect");
        }
        aoeDamageCollider.InvokeCollision += Apply;
        aoeDamageCollider.Initialize(1 << 14, AoeDamageRadius);
    }

    private void Apply(Vector3 v, Collider2D c) {
        //IF we hit a lifeform damage it - otherwise move on
        var lifeform = c.transform.GetComponent<BaseLifeform>();
        if (lifeform != null) {
            print("hit lifeform: " + lifeform.gameObject.name + " and did " + Damage + " damage");
            lifeform.Damage(Damage);
        }
    }
}
