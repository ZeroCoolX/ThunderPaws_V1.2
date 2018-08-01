using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyPickup : MonoBehaviour {
    /// <summary>
    /// This is what get spawned when the pickup collides with the player
    /// </summary>
    public Transform PickupReward;

    /// <summary>
    /// Necessary for collisions
    /// </summary>
    private SimpleCollider Collider;

	// Use this for initialization
	void Start () {
        //Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << 8);
    }
	
	private void Apply(Vector3 v, Collider2D c) {
        var player = c.transform.GetComponent<Player>();
        player.ApplyWeaponPickup(PickupReward.gameObject.name);
        Destroy(gameObject);
    }
}
