using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class hanadles projectile movement for "bullet" based projectiles.
/// </summary>
public class BulletProjectile : AbstractProjectile {

    private void Start() {
        //Call the base start
        base.Start();
    }

    private void Update() {
        Move();
    }

    protected override void Move() {
        //Move as a constant speed
        transform.Translate(TargetDirection.normalized * MoveSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// Destroy and generate effects
    /// </summary>
    /// <param name="hitPos"></param>
    /// <param name="hitObject"></param>
    protected override void HitTarget(Vector3 hitPos, Collider2D hitObject) {
        //Damage whoever we hit - or rocket jump
        Player player;
        switch (hitObject.gameObject.tag) {
            case "Player":
                Debug.Log("We hit " + hitObject.name + " and did " + Damage + " damage");
                player = hitObject.GetComponent<Player>();
                if (player != null) {
                    //player.DamageHealth(Damage);
                }
                break;
        }
        print("Hit object: " + hitObject.gameObject.tag);
        //IF we hit a lifeform damage it - otherwise move on
        var lifeform = hitObject.transform.GetComponent<BaseLifeform>();
        if(lifeform != null) {
            print("calling damage on lifeform!");
            lifeform.Damage(10f);
        }
        GenerateEffect();
        Destroy(gameObject);
    }

    private void GenerateEffect() {
        var clone = Instantiate(ImpactEffect, transform.position, transform.rotation);
        clone.GetComponent<DeathTimer>().TimeToLive = 0.25f;
        clone.GetComponent<Animator>().SetBool("Invoke", true);
    }
}
