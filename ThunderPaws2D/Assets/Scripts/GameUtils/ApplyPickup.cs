using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyPickup : MonoBehaviour {
    public Transform PickupReward;

    private SimpleCollider Collider;

    private const int PLAYER_LAYER = 8;

	void Start () {
        // Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << PLAYER_LAYER);
    }
	
	private void Apply(Vector3 v, Collider2D c) {
        var player = c.transform.GetComponent<Player>();
        player.ApplyWeaponPickup(PickupReward.gameObject.name);
        Destroy(gameObject);
    }
}
