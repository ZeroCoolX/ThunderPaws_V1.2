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

    [Header("Life Properties")]
    public float Health = 1f;
    [Header("Payload Properties")]
    public Transform PayloadContent;
    public int PayloadItemCount = 10;

    /// <summary>
    /// Min and Max values for the range to which the items in the payload can explode.
    /// </summary>
    private Vector2[] _explodeRanges = new[]{
        new Vector2(1f, 7f),
        new Vector2(2f, 15f)
    };

    public new void Update() {
        base.Update();
        if (Health <= 0) {
            PreDestroy();
        }
        // Do not accumulate gravity if colliding with anythig vertical
        if (Controller2d.Collisions.FromBelow || Controller2d.Collisions.FromAbove) {
            Velocity.y = 0;
        }
        ApplyGravity();
    }

    /// <summary>
    /// Base implementation is just to invoke destruction immediately informing no one of the death
    /// </summary>
    protected virtual void PreDestroy() {
        InvokeDestroy();
    }

    /// <summary>
    /// Base implementation for destruction is simply to generate payload and destroy the GameObject.
    /// </summary>
    protected virtual void InvokeDestroy() {
        GeneratePayload();
        Destroy(gameObject);
    }

    private void GeneratePayload() {
        List<Transform> payload = new List<Transform>();
        // Generate the items to be offloaded and store them in the list
        for (var i = 0; i < PayloadItemCount; ++i) {
            var pItem = Instantiate(PayloadContent, transform.position, Quaternion.identity) as Transform;
            pItem.gameObject.SetActive(false);
            var coinController = pItem.GetComponent<CoinController>();
            if (coinController != null) {
                coinController.Initialize(GenerateRandomExplosionDirection());
                coinController.enabled = false;
            }
            payload.Add(pItem);
        }

        // Now offload in an explosive manor
        Explode(payload);
    }

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
        foreach (var pItem in payload) {
            if (pItem.GetComponent<CoinController>() != null) {
                pItem.GetComponent<CoinController>().enabled = true;
            }
            pItem.gameObject.SetActive(true);
        }
    }

    public override bool Damage(float damage) {
        Health -= damage;
        ActivateFlash();
        return Health <= 0;
    }
}
