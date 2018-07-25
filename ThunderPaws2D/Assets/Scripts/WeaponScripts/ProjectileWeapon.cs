using System;
using UnityEngine;

public class ProjectileWeapon : AbstractWeapon {
    /// <summary>
    /// Int value on the type indicates the index within an array to where that particular prefab lives
    /// </summary>
    protected enum BulletType { DEFAULT = 0, CHARGED = 1, ULT = 2 };

    /// <summary>
    /// Calculate all the necessary pieces of a gunshot.
    /// Direction going, normal, what layer its in, who it should hit.
    /// </summary>
    protected override void FireShot(int bulletCount = 1) {
        Vector2 directionInput = Player.DirectionalInput;

        // Store bullet origin spawn popint (A)
        Vector2 firePointPosition = new Vector2(FirePoint.position.x, FirePoint.position.y);
        // Collect the hit data - distance and direction from A -> B
        RaycastHit2D shot = Physics2D.Raycast(firePointPosition, directionInput, 100, WhatToHit);
        // Generate bullet effect
        if (Time.time >= ShotEffectDelay) {
            // Bullet effect position data
            Vector3 hitPosition;
            Vector3 hitNormal;

            // Precalculate so if we aren't shooting at anything at least the normal is correct
            // Arbitrarily large number so the bullet trail flys off the camera
            hitPosition = directionInput * 50f;
            if (shot.collider != null) {
                // If we most likely hit something store the normal so the particles make sense when they shoot out
                hitNormal = shot.normal;
            } else {
                // Rediculously huge so we can use it as a sanity check for the effect
                hitNormal = new Vector3(999, 999, 999);
            }

            var yAxis = directionInput.y;
            if (((yAxis > 0.3 && yAxis < 0.8)) || (Player.DirectionalInput == new Vector2(1f, 1f) || Player.DirectionalInput == new Vector2(-1f, 1f))) {
                directionInput = (Vector2.up + (Player.FacingRight ? Vector2.right : Vector2.left)).normalized;
            } else if (yAxis > 0.8) {
                directionInput = Vector2.up;
            } else {
                directionInput = Player.FacingRight ? Vector2.right : Vector2.left;
            }
            //Actually instantiate the effect
            GenerateShot(directionInput, hitNormal, WhatToHit, GameConstants.Layer_PlayerProjectile, bulletCount);
            GenerateCameraShake();
            ApplyRecoil();
            ShotEffectDelay = Time.time + 1 / ShotEffectSpawnRate;
            UpdateAmmo();
        }
    }

    /// <summary>
    /// Generate particle effect, spawn bullet, then destroy after allotted time.
    /// For loop is specific to FuzzBuster but leaving it in here for now because we allow for loop index 
    /// passed in
    /// </summary>
    /// <param name="shotPos">Vector position of where the shot is coming from</param>
    /// <param name="shotNormal">If we were able to calculate it based off the "preshot" this is the normal of what we think the imact will be</param>
    /// <param name="whatToHit">Layermask describing what to hit</param>
    /// <param name="layer">Indicates what layer the bullet should be apart of</param>
    /// <param name="bulletCount">How many bullets we want to spawn</param>
    protected override void GenerateShot(Vector3 shotPos, Vector3 shotNormal, LayerMask whatToHit, string layer, int bulletCount = 1) {
        // Needed for special -45 degree angle shots
        var projRotation = CompensateQuaternion(FirePoint.rotation);
        // Optional offsets only used if we have a 3 round burst shot like that of the FuzzBuster ultimate
        var yOffset = 0.25f;
        var xOffset = 0.25f;

        for (var i = 0; i < bulletCount; ++i) {
            var firePosition = FirePoint.position;

            // This calculation is necessary so the bullets don't stack on top of eachother
            var yAxis = Player.DirectionalInput.y;
            print("yAxis = " + yAxis);
            if (((yAxis > 0.3 && yAxis < 0.8)) || (Player.DirectionalInput == new Vector2(1f, 1f) || Player.DirectionalInput == new Vector2(-1f, 1f))) {
                yOffset = 0.125f;
                // There is one single special case - when the player is facing right, and looking at 45 degrees.
                // Coorindates must then be +, - instead of all + or all -
                xOffset = 0.125f * (Player.FacingRight ? -1 : 1);
            } else if (yAxis > 0.8) {
                yOffset = 0f;
                xOffset = 0.25f;
            } else {
                yOffset = 0.25f;
                xOffset = 0f;
            }

            // Based off the offset place the bullet in a position so it doesn't overlap with another
            firePosition.y = FirePoint.position.y + (i > 0 ? (i % 2 == 0 ? yOffset : yOffset * -1) : 0);
            firePosition.x = FirePoint.position.x + (i > 0 ? (i % 2 == 0 ? xOffset : xOffset * -1) : 0);

            Transform bulletInstance = Instantiate(BulletPrefab, firePosition, projRotation) as Transform;
            //Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
            AbstractProjectile projectile = bulletInstance.GetComponent<BulletProjectile>();
            if (Mathf.Sign(shotPos.x) < 0) {
                Vector3 theScale = projectile.transform.localScale;
                theScale.x *= -1;
                projectile.transform.localScale = theScale;
            }

            //Set layermask of parent (either player or baddie)
            projectile.FromPlayerNumber = Player.PlayerNumber;
            projectile.SetLayerMask(whatToHit);
            projectile.gameObject.layer = LayerMask.NameToLayer(layer);
            projectile.Damage = Damage;
            projectile.MoveSpeed = BulletSpeed;
            projectile.MaxLifetime = MaxLifetime;
            projectile.Fire(shotPos, shotNormal);
        }
    }

    protected override void ApplyRecoil() {
        throw new NotImplementedException();
    }

    protected virtual void UpdateAmmo() {
        if (HasAmmo) {
            Ammo -= 1;
            PlayerHudManager.Instance.GetPlayerHud(Player.PlayerNumber).SetAmmo(Ammo);
        } else {
            PlayerHudManager.Instance.GetPlayerHud(Player.PlayerNumber).SetAmmo();
        }
    }
}
