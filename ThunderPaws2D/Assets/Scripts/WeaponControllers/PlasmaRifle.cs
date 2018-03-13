using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasmaRifle : Weapon {
    
    /// <summary>
    /// Amount to shake camera by
    /// </summary>
    public float CamShakeAmount = 0.025f;
    /// <summary>
    /// Length of time to shake camera
    /// </summary>
    public float CamShakeLength = 0.1f;

    /// <summary>
    /// Need a reference to the player this weapon is aattached to so we can get the direction input
    /// </summary>
    private Player _player;

    /// <summary>
    /// Indicates the player is holding down the fire button
    /// </summary>
    private bool _holdingFireDown = false;
    /// <summary>
    /// Allows me to track when the fire button is pressed to calculate if we should autofire or not
    /// </summary>
    private bool _fireButtonPressed = false;
    /// <summary>
    /// If the player is holding down the button for >= 0.5 seconds start firing automatically.
    /// Otherwise most weapons will be fired as fast as the player can pull the trigger.
    /// </summary>
    private float _fireHoldthreshold = 0.5f;
    /// <summary>
    /// Must store the initial time the user pressed the button
    /// </summary>
    private float _initialFirePressTime;



    protected new void Start() {
        base.Start();
        _player = transform.parent.parent.GetComponent<Player>();
        if (_player == null) {
            throw new MissingComponentException("This is massively bad... No Player.cs found on the Player");
        }
    }

    private void Update() {
        HandleShootingInput();
        if (HasAmmo) {
            AmmoCheck();
        }
        WeaponAnimator.SetBool("UltModeActive", UltMode);
    }

    /// <summary>
    /// Check if this weapon still has ammo
    /// </summary>
    private void AmmoCheck() {
        if (Ammo == 0) {
            _player.RemoveOtherWeapon(transform);
        }
    }

    protected override void ApplyRecoil() {
        WeaponAnimator.SetBool("ApplyRecoil", true);
        StartCoroutine(ResetWeaponPosition());
    }

    /// <summary>
    /// Used to determine if the player is holding the trigger down so we shuold shoot at some fixed interval, or whether they're
    /// pressing the trigger rapid fire like and so we should fire every shot.
    /// If we are in ultimate mode - right now just shoot three bullets for every trigger pull.
    /// </summary>
    private void HandleShootingInput() {
        if (FireRate == 0) {//Single fire
            var rightTrigger = Input.GetAxis(GameConstants.Input_Xbox_RTrigger);
            if (Input.GetButtonUp(GameConstants.Input_Fire) || rightTrigger == 0) {
                _fireButtonPressed = false;
                _holdingFireDown = false;
            }
            if ((Input.GetButton(GameConstants.Input_Fire) || rightTrigger > 0)) {
                if (!_fireButtonPressed) {
                    _fireButtonPressed = true;
                    _initialFirePressTime = Time.time + _fireHoldthreshold;
                }
                //They're been holding for longer than 0.5s so auto fire
                _holdingFireDown = _fireButtonPressed && Time.time > _initialFirePressTime;

            }
            if (_holdingFireDown) {
                if (Time.time > TimeToFire) {
                    TimeToFire = Time.time + 0.25f;
                    CalculateShot();
                }
            } else {
                if (_fireButtonPressed) {
                    CalculateShot();
                }
            }
        }
    }

    protected override void CalculateShot() {
        Vector2 directionInput = _player.DirectionalInput;

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
            if (((yAxis > 0.3 && yAxis < 0.8))) {
                directionInput = (Vector2.up + (_player.FacingRight ? Vector2.right : Vector2.left)).normalized;
            } else if (yAxis > 0.8) {
                directionInput = Vector2.up;
            } else {
                directionInput = _player.FacingRight ? Vector2.right : Vector2.left;
            }
            //Actually instantiate the effect
            GenerateShot(directionInput, hitNormal, WhatToHit, GameConstants.Layer_PlayerProjectile, UltMode);
            GenerateCameraShake();
            ApplyRecoil();
            TimeToSpawnEffect = Time.time + 1 / EffectSpawnRate;
            if (HasAmmo) {
                Ammo -= 1;
            }
        }
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
        var verticalUltOffset = 0.25f;
        for (var i = 0; i < 3; ++i) {
            var firePosition = FirePoint.position;
            firePosition.y = FirePoint.position.y + (i > 0 ? (i % 2 == 0 ? verticalUltOffset : verticalUltOffset * -1) : 0);
            Transform bulletInstance = Instantiate(BulletPrefab, firePosition, projRotation) as Transform;
            //Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
            AbstractProjectile projectile = bulletInstance.GetComponent<BulletProjectile>();
            //TODO will have to be changed when diagonal directional shooting comes into play - take out when we pass in the rotation of the bullet
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

    protected override void GenerateCameraShake() {
        CamShake.Shake(CamShakeAmount, CamShakeLength);
    }
}
