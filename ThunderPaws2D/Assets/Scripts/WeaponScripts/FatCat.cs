using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatCat : ProjectileWeapon {
    /// <summary>
    /// List from least special to most special bullet prefabs
    /// Access the indicies by using the BulletType enum
    /// </summary>
    public Transform[] BulletPrefabs;
    /// <summary>
    /// Indicates the ultimate animations have finished and we can shoot again.
    /// This inhibits users from spamming the fire key during the ult animation
    /// </summary>
    private bool _allowshooting = true;
    /// <summary>
    /// Necessaary indicator for the ultMode.
    /// Indicates the trigger was let go telling the
    /// system we can shoot again
    /// </summary>
    private bool _triggerLetGo = true;


    private void Update() {
        if (UltMode) {
            MakeCarpetBomb();
            Player.DeactivateUltimate();
        } else {
            HandleShootingInput();
        }
        if (HasAmmo) {
            CheckAmmo();
        }
    }

    /// <summary>
    /// Implementation specific override.
    /// Apply the recoil animation and reset the weapon position
    /// </summary>
    protected override void ApplyRecoil() {
        WeaponAnimator.SetBool("ApplyRecoil", true);
        StartCoroutine(ResetWeaponPosition());
    }

    /// <summary>
    /// Helper method that allows for delayed setting of the variable which
    /// indicates we can shoot. This creates a delay effect when shooting so you can't
    /// spawm 1000 rockets per second
    /// </summary>
    private void AllowUltShooting() {
        _allowshooting = true;
    }

    /// <summary>
    /// Ult logic for which a line of bombs are created outside the players field of view and dropped into the 
    /// scene making it look like a plane flew over the top and dropped a line of bombs
    /// </summary>
    private void MakeCarpetBomb() {
        // Use the top right corner to determine the screen width in world points 
        // so we know where to spawn the bombs
        Vector2 topRightCorner = new Vector2(1, 1);
        Vector2 edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);
        float width = edgeVector.x * 2;
        // Arbitrary value honestly - I'm not sold on this yet
        var interval = width / 15f;

        // These spacing variables increase as we move further from the left corner of the screen
        // so that the bombs don't all drop in an actual straight line, but instead a diagonal
        // So it's more realistic
        var xSpacing = 0f;
        var ySpacing = 0f;

        // Create and drop the bombs
        for (var i = 0; i < 20; ++i) {
            var pos = new Vector2(edgeVector.x - xSpacing, edgeVector.y + ySpacing);
            CreateBomb(pos);
            AudioManager.Instance.playSound(GameConstants.Audio_FatCatShot);
            xSpacing += interval;
            ySpacing += 2;
        }
    }

    /// <summary>
    /// Helper method to create an individual bomb which is apart of the carpet bombing run.
    /// Sets all the needed values
    /// </summary>
    /// <param name="position"></param>
    private void CreateBomb(Vector2 position) {
        Transform bulletInstance = Instantiate(BulletPrefabs[(int)BulletType.ULT], position, Quaternion.identity) as Transform;
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
            BulletPrefab = BulletPrefabs[(int)BulletType.DEFAULT];
            AudioManager.Instance.playSound(GameConstants.Audio_FatCatShot);
            CalculateShot();
        }
    }
}
