using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : AbstractWeapon {

    /// <summary>
    /// How long should the shotgun blast extend
    /// </summary>
    private float _rayLength = 8;

    private float[] _blastRotations = new float[] { 11.25f, 5.625f, 0f, 348.75f, 354.375f };

    private Vector2[] _debugBlastRotations = new Vector2[7];

    /// <summary>
    /// Sprite animatio to play when the bullet impacts
    /// </summary>
    public Transform ImpactEffect;

    private void Update() {

        for (var i = 1; i < _debugBlastRotations.Length; ++i) {
            Debug.DrawRay(_debugBlastRotations[0], _debugBlastRotations[i] * _rayLength, Color.green);
        }
        // Get the player fire input
        var rightTrigger = Input.GetAxis(GameConstants.Input_Xbox_RTrigger);
        if ((Input.GetButton(GameConstants.Input_Fire) || rightTrigger > WeaponConfig.TriggerFireThreshold)) {
            CalculateShot();
        }
        if (HasAmmo) {
            AmmoCheck();
        }
        WeaponAnimator.SetBool("UltModeActive", UltMode);
    }

    protected override void ApplyRecoil() {
        WeaponAnimator.SetBool("ApplyRecoil", true);
        StartCoroutine(ResetWeaponPosition());
    }

    protected override void CalculateShot() {
        if (Time.time >= TimeToSpawnEffect) {
            // Generate 5 raycasts 45, 22.5, 0, 337.5, 315 degrees
            // Rotation around the z axis
            var i = 1;
            var collisionMap = new Dictionary<string, Collider2D>();
            foreach(var rotation in _blastRotations) {
                var finalRotation = rotation * (Player.FacingRight ? 1 : -1);

                // Get the direction the player is pointing
                Vector2 directionInput = Player.DirectionalInput;
                // Concert the degrees to radians 
                var rads = Mathf.Deg2Rad * finalRotation;
                print(directionInput);
                var yAxis = directionInput.y;
                var xDir = 1.0f * (Player.FacingRight ? 1 : -1);
                var yDir = 0f;
                if (((yAxis > 0.3 && yAxis < 0.8))) {
                    yDir = 0.5f;
                    xDir = 0.5f * (Player.FacingRight ? 1 : -1);
                } else if (yAxis > 0.8) {
                    yDir = 1.0f;
                    xDir = 0.0f * (Player.FacingRight ? 1 : -1);
                }

                Vector2 rotatedDirection = new Vector2(
                    xDir*Mathf.Cos(rads) - yDir*Mathf.Sin(rads),
                    xDir*Mathf.Sin(rads) + yDir*Mathf.Cos(rads)
                    );

                // Store bullet origin spawn point (A)
                Vector2 firePointPosition = new Vector2(FirePoint.position.x, FirePoint.position.y);
                _debugBlastRotations[0] = firePointPosition;
                // Collect the hit data - distance and direction from A -> B
                RaycastHit2D shot = Physics2D.Raycast(firePointPosition, rotatedDirection, _rayLength, WhatToHit);

                // Check if it hit anything
                //We want to allow bullets to pass throught obstacles that the player can pass through
                if (shot.collider != null) {
                    print("We have collided with something i: " + i);
                    Collider2D outCollider;
                    if (!collisionMap.TryGetValue(shot.collider.gameObject.name, out outCollider)) {
                        collisionMap.Add(shot.collider.gameObject.name, shot.collider);
                    }
                }

                _debugBlastRotations[i] = rotatedDirection;
                ++i;
            }

            foreach(var collider in collisionMap) {
                HitTarget(collider.Value);
            }

            TimeToSpawnEffect = Time.time + 1 / EffectSpawnRate;
            if (HasAmmo) {
                Ammo -= 1;
            }
        }
    }

    /// <summary>
    /// Destroy and generate effects
    /// </summary>
    /// <param name="hitObject"></param>
    private void HitTarget(Collider2D hitObject) {
        //Damage whoever we hit - or rocket jump
        Player player;
        if (hitObject.gameObject.tag == GameConstants.Tag_Player) {
            Debug.Log("We hit " + hitObject.name + " and did " + Damage + " damage");
            player = hitObject.GetComponent<Player>();
            if (player != null) {
                //player.DamageHealth(Damage);
            }
        }
        print("Hit object: " + hitObject.gameObject.tag);
        //IF we hit a lifeform damage it - otherwise move on
        var lifeform = hitObject.transform.GetComponent<BaseLifeform>();
        if (lifeform != null) {
            print("hit lifeform: " + lifeform.gameObject.name + " and did " + Damage + " damage");
            lifeform.Damage(Damage);
        }
        GenerateEffect(hitObject.transform);
    }

    private void GenerateEffect(Transform impactTransform) {
        // Subtract 1 from whatever the Z position is to ensure the explosion animation is played over the top
        var fixedPosition = new Vector3(impactTransform.position.x, impactTransform.position.y, impactTransform.position.z - 1);
        var clone = Instantiate(ImpactEffect, fixedPosition, impactTransform.rotation);
        clone.GetComponent<DeathTimer>().TimeToLive = 0.25f;
        clone.GetComponent<Animator>().SetBool("Invoke", true);
    }

    protected override void GenerateShot(Vector3 shotPos, Vector3 shotNormal, LayerMask whatToHit, string layer, bool ultMode, float freeFlyDelay = 0.5F) {
        throw new NotImplementedException();
    }
}
