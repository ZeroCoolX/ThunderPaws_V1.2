using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatCat : AbstractWeapon {
    /// <summary>
    /// This is another case where the weapon needs special ult bullets
    /// </summary>
    public Transform UltBulletPrefab;
    /// <summary>
    /// Indicates the ultimate animations have finished and we can shoot again.
    /// This inhibits users from spamming the fire key during the ult animation
    /// </summary>
    public bool _allowshooting = true;
    /// <summary>
    /// Necessaary indicator for the ultMode.
    /// Indicates the trigger was let go telling the
    /// system we can shoot again
    /// </summary>
    private bool _triggerLetGo = true;


    private void Update() {
        if (UltMode) {
            // Carpet Bomb
            MakeCarpetBomb();
            Player.DeactivateUltimate();
        } else {
            HandleShootingInput();
        }
        if (HasAmmo) {
            AmmoCheck();
        }
    }

    protected override void ApplyRecoil() {
        WeaponAnimator.SetBool("ApplyRecoil", true);
        StartCoroutine(ResetWeaponPosition());
    }


    private void AllowUltShooting() {
        _allowshooting = true;
    }

    private void MakeCarpetBomb() {
        Vector2 topRightCorner = new Vector2(1, 1);
        Vector2 edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);
        float width = edgeVector.x * 2;
        var interval = width / 15f;
        var xSpacing = 0f;
        var ySpacing = 0f;
        for (var i = 0; i < 20; ++i) {
            var pos = new Vector2(edgeVector.x - xSpacing, edgeVector.y + ySpacing);
            CreateBomb(pos);
            xSpacing += interval;
            ySpacing += 2;
        }
    }

    private void CreateBomb(Vector2 position) {
        Transform bulletInstance = Instantiate(UltBulletPrefab, position, Quaternion.identity) as Transform;
        //Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
        AbstractProjectile projectile = bulletInstance.GetComponent<BulletProjectile>();
        //Set layermask of parent (either player or baddie)
        projectile.SetLayerMask(WhatToHit);
        projectile.gameObject.layer = LayerMask.NameToLayer(GameConstants.Layer_PlayerProjectile);
        projectile.Damage = Damage;
        projectile.MoveSpeed = BulletSpeed;
        projectile.MaxLifetime = 10f;
        projectile.Fire(Vector2.down, Vector2.up);
    }

    /// <summary>
    /// Gauss Ultimate requires a special shooting mode.
    /// Instead of continuously shooting aws fast as the use pulls the trigger it 
    /// it charges a shot and shoots it as soon as they let go
    /// </summary>
    private void HandleShootingInput() {
        // Get the player fire input
        var rightTrigger = Input.GetAxis(Player.JoystickId + GameConstants.Input_RTrigger);
        // This checks if the player released the trigger in between shots - because this ultimate is not full auto
        if (!_triggerLetGo) {
            if (rightTrigger <= WeaponConfig.TriggerFireThreshold && !Input.GetKey(InputManager.Instance.Fire)) {
                _triggerLetGo = true;
            }
        }

        if (_triggerLetGo && (Input.GetKeyDown(InputManager.Instance.Fire) || rightTrigger > WeaponConfig.TriggerFireThreshold) && _allowshooting) {
            _triggerLetGo = false;
            _allowshooting = false;

            // Allow the user's fire pressing to be registered in 0.35seconds
            Invoke("AllowUltShooting", 0.35f);

            ApplyRecoil();
            CalculateCustomShot();
            //AudioManager.playSound(GameConstants.Audio_Shotgun);
        }
    }

    /// <summary>
    /// Gauss Ultimate has a special different kind of bullet
    /// that needs to be charged and then fired
    /// </summary>
    private void CalculateCustomShot() {
        Vector2 directionInput = Player.DirectionalInput;

        //Store bullet origin spawn popint (A)
        Vector2 firePointPosition = new Vector2(FirePoint.position.x, FirePoint.position.y);
        //Collect the hit data - distance and direction from A -> B
        RaycastHit2D shot = Physics2D.Raycast(firePointPosition, directionInput, 100, WhatToHit);
        //Generate bullet effect
        if (Time.time >= TimeToSpawnEffect) {
            //Bullet effect position data
            Vector3 hitPosition;
            Vector3 hitNormal;

            //Precalculate so if we aren't shooting at anything at least the normal is correct
            //Arbitrarily laarge number so the bullet trail flys off the camera
            hitPosition = directionInput * 50f;
            if (shot.collider != null) {
                //If we most likely hit something store the normal so the particles make sense when they shoot out
                hitNormal = shot.normal;
                //hitPosition = shot.point;
            } else {
                //Rediculously huge so we can use it as a sanity check for the effect
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

            GenerateShot(directionInput, hitNormal, WhatToHit, GameConstants.Layer_PlayerProjectile, UltMode);
            GenerateCameraShake();
            TimeToSpawnEffect = Time.time + 1 / EffectSpawnRate;
            if (HasAmmo) {
                Ammo -= 1;
            }
            GameMaster.Instance.GetPlayerStatsUi(1).SetAmmo();
        }
    }

    protected override void CalculateShot() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generate particle effect, spawn bullet, then destroy after allotted time
    /// </summary>
    /// <param name="shotPos"></param>
    /// <param name="shotNormal"></param>
    /// <param name="whatToHit"></param>
    protected override void GenerateShot(Vector3 shotPos, Vector3 shotNormal, LayerMask whatToHit, string layer, bool ultMode, float freeFlyDelay = 0.5f) {
        //Fire the projectile - this will travel either out of the frame or hit a target - below should instantiate and destroy immediately
        var projRotation = CompensateQuaternion(FirePoint.rotation);
        var yUltOffset = 0.25f;
        var xUltOffset = 0.25f;
        for (var i = 0; i < 1; ++i) {
            var firePosition = FirePoint.position;

            // This calculation is necessary so the bullets don't stack on top of eachother
            var yAxis = Player.DirectionalInput.y;
            print("yAxis = " + yAxis);
            if (((yAxis > 0.3 && yAxis < 0.8)) || (Player.DirectionalInput == new Vector2(1f, 1f) || Player.DirectionalInput == new Vector2(-1f, 1f))) {
                yUltOffset = 0.125f;
                // There is one single special case - when the player is facing right, and looking at 45 degrees.
                // Coorindates must then be +, - instead of all + or all -
                xUltOffset = 0.125f * (Player.FacingRight ? -1 : 1);
            } else if (yAxis > 0.8) {
                yUltOffset = 0f;
                xUltOffset = 0.25f;
            } else {
                yUltOffset = 0.25f;
                xUltOffset = 0f;
            }

            firePosition.y = FirePoint.position.y + (i > 0 ? (i % 2 == 0 ? yUltOffset : yUltOffset * -1) : 0);
            firePosition.x = FirePoint.position.x + (i > 0 ? (i % 2 == 0 ? xUltOffset : xUltOffset * -1) : 0);
            Transform bulletInstance = Instantiate(BulletPrefab, firePosition, projRotation) as Transform;
            //Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
            AbstractProjectile projectile = bulletInstance.GetComponent<BulletProjectile>();
            if (Mathf.Sign(shotPos.x) < 0) {
                Vector3 theScale = projectile.transform.localScale;
                theScale.x *= -1;
                projectile.transform.localScale = theScale;
            }

            //Set layermask of parent (either player or baddie)
            projectile.SetLayerMask(whatToHit);
            projectile.gameObject.layer = LayerMask.NameToLayer(layer);
            projectile.Damage = Damage;
            projectile.MoveSpeed = BulletSpeed;
            projectile.MaxLifetime = MaxLifetime;
            projectile.Fire(shotPos, shotNormal);
            if (!ultMode) return;
        }
    }
}
