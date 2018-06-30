using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class hanadles projectile movement for "bullet" based projectiles.
/// </summary>
public class BulletProjectile : AbstractProjectile {
    public float OptionalGravity = 0f;
    private bool _applyGravity = false;
    private void Start() {
        //Call the base start
        base.Start();
        if(OptionalGravity != 0f) {
            // in a random time between 0.3 and 1 seconds apply gravity
            //Invoke("BeginGravity", (UnityEngine.Random.Range(0.3f, 1f)));
        }
    }

    private void BeginGravity() {
        _applyGravity = true;
    }

    private void Update() {
        //if (OptionalGravity != 0f) {
        //    ApplyGravity();
        //}
        Move();
    }

    private void ApplyGravity() {
        print("Applying gravity! - " + OptionalGravity);
        TargetDirection.y += OptionalGravity * Time.deltaTime;
    }

    protected override void Move() {
        //if (OptionalGravity != 0f) {
        //    var newVector = TargetDirection.normalized;
        //    transform.Translate(new Vector3(newVector.x, newVector.y, newVector.z) * MoveSpeed * Time.deltaTime, Space.World);
        //} else {
            transform.Translate(TargetDirection.normalized * MoveSpeed * Time.deltaTime, Space.World);
        //}
    }

    /// <summary>
    /// Destroy and generate effects
    /// </summary>
    /// <param name="hitPos"></param>
    /// <param name="hitObject"></param>
    protected override void HitTarget(Vector3 hitPos, Collider2D hitObject) {
        //Damage whoever we hit - or rocket jump
        Player player;
        print("Hit object: " + hitObject.gameObject.tag);
        if (hitObject.gameObject.tag == GameConstants.Tag_Baddie) {
            print(" PLAY CUZ " + hitObject.gameObject.tag);
            var rand = UnityEngine.Random.Range(0, 9);
            AudioManager.Instance.playSound(rand % 2 == 0 ? "BulletImpact1" : "BulletImpact2");
        }
        else{
            print("DONT PLAY CUZ " + hitObject.gameObject.tag);
        }
        //IF we hit a lifeform damage it - otherwise move on
        var lifeform = hitObject.transform.GetComponent<BaseLifeform>();
        if(lifeform != null) {
            print("hit lifeform: " + lifeform.gameObject.name + " and did " + Damage + " damage");
            if (lifeform.Damage(Damage)) {
                // increment the stats for whoever shot the bullet
                print("Adding baddie for player  : " + FromPlayerNumber);
                GameStatsManager.Instance.AddBaddie(FromPlayerNumber);
            }
        }
        GenerateEffect();
        Destroy(gameObject);
    }
}
