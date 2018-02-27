using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : AbstractWeapon {
    /// <summary>
    /// Indicates if this weapon has ammo. All weapons have a finite amount of ammo except the default weapon
    /// </summary>
    private bool _hasAmmo = true;
    /// <summary>
    /// Indicates the weapon is in Ultimate Mode!
    /// </summary>
    public bool UltMode {get;set;}

    /// <summary>
    /// Amount to shake camera by
    /// </summary>
    [Header("Camera Attributes")]
    public float CamShakeAmount = 0.025f;
    /// <summary>
    /// Length of time to shake camera
    /// </summary>
    public float CamShakeLength = 0.1f;
    /// <summary>
    /// Camera shake reference
    /// </summary>
    private CameraShake _camShake;
    /// <summary>
    /// LayuerMask indicating what to hit
    /// </summary>
    public LayerMask WhatToHit;

    /// <summary>
    /// Every weapon aside from the default one has ammo that runs out eventually
    /// </summary>
    public int Ammo;

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

    protected void Start() {
        base.Start();
        _player = transform.parent.parent.GetComponent<Player>();
        if(_player == null) {
            throw new MissingComponentException("This is massively bad... No Player.cs found on the Player");
        }
        _camShake = GameMaster.Instance.GetComponent<CameraShake>();
        if (_camShake == null) {
            Debug.LogError("Weapon.cs: No CameraShake found on game master");
            throw new MissingComponentException();
        }
        _hasAmmo = !gameObject.name.Equals(_player.DEFAULT_WEAPON_NAME);
    }

    private void Update() {
        HandleShootingInput();
        if (_hasAmmo) {
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

    /// <summary>
    /// Used to determine if the player is holding the trigger down so we shuold shoot at some fixed interval, or whether they're
    /// pressing the trigger rapid fire like and so we should fire every shot.
    /// If we are in ultimate mode - right now just shoot three bullets for every trigger pull.
    /// </summary>
    protected override void HandleShootingInput() {
        if (FireRate == 0) {//Single fire
            var rightTrigger = Input.GetAxis("X360_Trigger_R");
            if (Input.GetButtonUp("Fire1") || rightTrigger == 0) {
                _fireButtonPressed = false;
                _holdingFireDown = false;
            }
            if((Input.GetButton("Fire1") || rightTrigger > 0)) {
                if (!_fireButtonPressed) {
                    _fireButtonPressed = true;
                    _initialFirePressTime = Time.time + _fireHoldthreshold;
                }
                //They're been holding for longer than 0.5s so auto fire
                _holdingFireDown = _fireButtonPressed && Time.time > _initialFirePressTime;

            }
            if (_holdingFireDown) {
                if (Time.time > _timeToFire) {
                    _timeToFire = Time.time + 0.25f;
                    Shoot();
                }
            } else {
                if (_fireButtonPressed) {
                    Shoot();
                }
            }
        }
    }

    /// <summary>
    /// Fire projectile from origin to mouse position
    /// </summary>
    private void Shoot() {
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
            }else {
                directionInput = _player.FacingRight ? Vector2.right : Vector2.left;
            }
            //Actually instantiate the effect
            GenerateEffect(directionInput, hitNormal, WhatToHit, "PLAYER_PROJECTILE", UltMode);
            GenerateCamShake();
            ApplyRecoil();
            TimeToSpawnEffect = Time.time + 1 / EffectSpawnRate;
            if (_hasAmmo) {
                Ammo -= 1;
            }
        }
    }

    /// <summary>
    /// Generate camera shake
    /// </summary>
    private void GenerateCamShake() {
        //Generate camera shake
        _camShake.Shake(CamShakeAmount, CamShakeLength);
    }

    protected override void ApplyRecoil() {
        WeaponAnimator.SetBool("ApplyRecoil", true);
        StartCoroutine(ResetWeaponPosition());
    }
}