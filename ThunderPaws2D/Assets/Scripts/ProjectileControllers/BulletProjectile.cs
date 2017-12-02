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
        CheckForCollisions();
        Move();
    }

    /// <summary>
    /// Check for collisions
    /// </summary>
    private void CheckForCollisions() {
        //Raycast to check if we could potentially the target
        RaycastHit2D possibleHit = Physics2D.Raycast(transform.position, TargetDirection);
        if (possibleHit.collider != null) {
            //Mini raycast to check handle ellusive targets
            RaycastHit2D distCheck = Physics2D.Raycast(transform.position, TargetDirection, 0.2f, WhatToHit);
            //We want to allow bullets to pass throught obstacles that the player can pass through
            if (distCheck.collider != null && distCheck.collider.gameObject.tag != "OBSTACLE-THROUGH") {
                HitTarget(transform.position, distCheck.collider);
            }

            //Last check is simplest check
            Vector3 dir = TargetPos - transform.position;
            float distanceThisFrame = MoveSpeed * Time.deltaTime;
            //Length of dir is distance to target. if thats less than distancethisframe we've already hit the target
            if (dir.magnitude <= distanceThisFrame) {
                //Make sure the player didn't dodge out of the way
                distCheck = Physics2D.Raycast(transform.position, TargetDirection, 0.2f, WhatToHit);
                //We want to allow bullets to pass throught obstacles that the player can pass through
                if (distCheck.collider != null && distCheck.collider.gameObject.tag != "OBSTACLE-THROUGH") {
                    HitTarget(transform.position, distCheck.collider);
                }
            }
        }
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
            //case "BADDIE":
            //    Debug.Log("We hit " + hitObject.name + " and did " + Damage + " damage");
            //    if (hitObject.GetComponent<Baddie>() != null) {
            //        Baddie baddie = hitObject.GetComponent<Baddie>();
            //        if (baddie != null) {
            //            //Naturally someone would realize they're being attacked if they were shot so retaliate
            //            if (baddie.State != MentalStateEnum.ATTACK) {
            //                baddie.State = MentalStateEnum.ATTACK;
            //            }
            //            baddie.DamageHealth(Damage);
            //        }
            //    } else if (hitObject.GetComponent<BaddieBoss>() != null) {
            //        BaddieBoss boss = hitObject.GetComponent<BaddieBoss>();
            //        boss.DamageHealth(Damage);
            //    }
            //    break;
        }
        print("Hit object: " + hitObject.gameObject.tag);
        Destroy(gameObject);
    }
}
