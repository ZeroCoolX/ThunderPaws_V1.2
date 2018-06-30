using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class handles all logic involving damaging and killing the lifeform. Also
/// generating the payload each lifeform has secretly within itself.
/// Baddies, Circuitboxes...etc
/// </summary>
public class DamageableLifeform : BaseLifeform {

    // Payload Properties
    /// <summary>
    /// What is generated upon the death of the lifeform.
    /// Could be either a Coin or a WeaponPickup prefab
    /// </summary>
    public Transform PayloadContent;
    /// <summary>
    /// Number of items in the payload.
    /// Deafult is 10 items.
    /// </summary>
    public int PayloadItemCount = 10;
    /// <summary>
    /// Min and Max values for the range to which the items in the payload can 
    /// explode.
    /// [0] = min
    /// [1] = max
    /// </summary>
    private Vector2[] _explodeRanges = new []{
        new Vector2(1f, 7f),
        new Vector2(2f, 15f)
    };

    // Lifeform Properties
    /// <summary>
    /// How much health this lifeform has.
    /// Default is 1f (dies in a single collision)
    /// </summary>
    public float Health = 1f;

    /// <summary>
    /// Call BaseLifeform.Update()
    /// Check health for if we need to die andapply gravity
    /// </summary>
    public new void Update() {
        base.Update();
        if (Health <= 0) {
            PreDestroy();
        }
        //Do not accumulate gravity if colliding with anythig vertical
        if (Controller2d.Collisions.FromBelow || Controller2d.Collisions.FromAbove) {
            Velocity.y = 0;
        }
        ApplyGravity();
    }

    /// <summary>
    /// Base implementation is just to invoke destruction immedaitely
    /// informing no one of the death
    /// </summary>
    protected virtual void PreDestroy() {
        InvokeDestroy();
    }

    /// <summary>
    /// Base implementation for destruction is simply to generate payload and 
    /// destroy the GameObject.
    /// Extending classes can override this for more functionality
    /// </summary>
    protected virtual void InvokeDestroy() {
        GeneratePayload();
        Destroy(gameObject);
    }

    /// <summary>
    /// Construct a list of all the transforms that are in the payload.
    /// Offload them by exploding.
    /// </summary>
    private void GeneratePayload() {
        List<Transform> payload = new List<Transform>();
        // Generate the items to be offloaded and store them in the list
        for (var i = 0; i < PayloadItemCount; ++i) {
            var pItem = Instantiate(PayloadContent, transform.position, Quaternion.identity) as Transform;
            pItem.gameObject.SetActive(false);
            // TODO: Allow for not just coins - currently CoinController is hardcoded
            var coinController = pItem.GetComponent<CoinController>();
            if(coinController != null) {
                coinController.Initialize(GenerateRandomExplosionDirection());
                coinController.enabled = false;
                // Add the item to the payload
            }
            payload.Add(pItem);
        }

        // Now offload in an explosive manor
        Explode(payload);
    }

    /// <summary>
    /// Generate a random Vector based off explodeRanges [0]Min and [1]Max values
    /// </summary>
    /// <returns></returns>
    private Vector2 GenerateRandomExplosionDirection() {
        var explodeX = UnityEngine.Random.Range(_explodeRanges[0].x, _explodeRanges[1].x);
        explodeX *= Mathf.Sign(UnityEngine.Random.Range(-1, 2));
        var explodeY = UnityEngine.Random.Range(_explodeRanges[0].y, _explodeRanges[1].y);
        return new Vector2(explodeX, explodeY);
    }

    /// <summary>
    /// Generate explosion effect by activating the script on each Transform
    /// in the payload.
    /// </summary>
    private void Explode(List<Transform> payload) {
        // TODO: Allow for not just coins - currently CoinController is hardcoded
        foreach(var pItem in payload) {
            if(pItem.GetComponent<CoinController>() != null) {
                pItem.GetComponent<CoinController>().enabled = true;
            }
            pItem.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Decrement the lifeforms health.
    /// Trigger flash damage animation
    /// </summary>
    /// <param name="damage"></param>
    public override bool Damage(float damage) {
        Health -= damage;
        ActivateFlash();
        return Health <= 0;
    }
}
