using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSatellite : DamageableLifeform {
    /// <summary>
    /// List of possible pikcup options that drop when this baddie is destroyed
    /// </summary>
    public Transform[] PickupOptions;
    /// <summary>
    /// How fast the baddie moves
    /// </summary>
    private float _moveSpeed = 2f;
    /// <summary>
    /// Indicates which direction to move since these are stupid and only move in one direction
    /// </summary>
    public bool MoveLeft;

	// Use this for initialization
	void Start () {
        Gravity = 0;
        Health = 30;

        // Set the facing direction and movespeed
        CalculateFacingDirection();

        PayloadItemCount = 1;
        // Se the payload content to be a random weapon pickup
        PayloadContent = RandomlySelectPayload();
    }

    /// <summary>
    /// Just a fun little randomizer for which weapon pickup to spawn
    /// </summary>
    /// <returns></returns>
    private Transform RandomlySelectPayload() {
        int r1, r2, r3 = 0;
        r1 = Random.Range(0, PickupOptions.Length);
        r2 = Random.Range(0, PickupOptions.Length);
        r3 = Random.Range(0, PickupOptions.Length);

        var decider = Random.Range(1, 4);
        return decider == 1 ? PickupOptions[r1] : decider == 2 ? PickupOptions[r2] : PickupOptions[r3];
    }

    private void Update() {
        base.Update();

        float velOut = 0f;
        Velocity.y = 0f;
        Velocity.x = Mathf.SmoothDamp(Velocity.x, _moveSpeed, ref velOut, 0.2f);
        Controller2d.Move(Velocity * Time.deltaTime);
    }

    /// <summary>
    /// bBased off MoveLeft bool set at compile time make the baddie face correctly
    /// </summary>
    private void CalculateFacingDirection() {
        if (MoveLeft) {
            _moveSpeed *= -1;
            return;
        }
        
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
