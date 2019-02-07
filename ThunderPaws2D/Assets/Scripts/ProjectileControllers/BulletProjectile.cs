using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class hanadles projectile movement for "bullet" based projectiles.
/// </summary>
public class BulletProjectile : AbstractProjectile {
    private void Start() {
        base.Start();
    }

    private void Update() {
        Move();
    }

    protected override void Move() {
        transform.Translate(TargetDirection.normalized * MoveSpeed * Time.deltaTime, Space.World);
    }

    protected override void HitTarget(Vector3 hitPos, Collider2D hitObject) {
        //print("Hit object: " + hitObject.gameObject.tag);
        if (hitObject.gameObject.tag == GameConstants.Tag_Baddie) {
            var rand = UnityEngine.Random.Range(0, 9);
            AudioManager.Instance.PlaySound(rand % 2 == 0 ? "BulletImpact1" : "BulletImpact2");
        }
        // If we hit a lifeform damage it - otherwise move on
        var lifeform = hitObject.transform.GetComponent<BaseLifeform>();
        if(lifeform != null) {
            //print("hit lifeform: " + lifeform.gameObject.name + " and did " + Damage + " damage");
            if (lifeform.Damage(Damage)) {
                // Increment the stats for whoever shot the bullet
                GameStatsManager.Instance.AddBaddie(FromPlayerNumber);
            }
        }
        GenerateEffect();
        Destroy(gameObject);
    }
}
