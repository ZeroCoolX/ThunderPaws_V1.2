using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: this should astoundingly be refactored because its almost 100% copied from the FuzzBuster script
public class GaussRifle : AbstractWeapon {
    /// <summary>
    /// Some weapons have specific prefabs for their ultimate bullets
    /// </summary>
    public Transform UltBulletPrefab;
    /// <summary>
    /// Indicates the ultimate animations have finished and we can shoot again.
    /// This inhibits users from spamming the fire key during the ult animation
    /// </summary>
    public bool _allowshooting = true;
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

    /// <summary>
    /// Necessaary indicator for the ultMode.
    /// Indicates the trigger was let go telling the
    /// system we can shoot again
    /// </summary>
    private bool _triggerLetGo = true;

    /// <summary>
    /// Allows detection of ".KeyUp()" logic for controllers since its
    /// an axis instead of a button
    /// </summary>
    private float _triggerLastFrame;
    private bool _triggerPressed;
    private bool _holding;
    private bool _holdingQueued;


    private void Update() {
        if (UltMode) {
            HandleUltShooting();
        }else {
            HandleShootingInput();
        }
        if (HasAmmo) {
            AmmoCheck();
        }
        //WeaponAnimator.SetBool("UltModeActive", UltMode);
    }


    protected override void ApplyRecoil() {
        WeaponAnimator.SetBool("ApplyRecoil", true);
        StartCoroutine(ResetWeaponPosition());
    }

    /// <summary>
    /// Resets the animator
    /// </summary>
    /// <returns></returns>
    protected IEnumerator ResetUltFire() {
        yield return new WaitForSeconds(0.5f);
        WeaponAnimator.SetBool("FireUlt", false);
    }

    private void AllowUltShooting() {
        _allowshooting = true;
    }

    /// <summary>
    /// Gauss Ultimate requires a special shooting mode.
    /// Instead of continuously shooting aws fast as the use pulls the trigger it 
    /// it charges a shot and shoots it as soon as they let go
    /// </summary>
    private void HandleUltShooting() {
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
            // Also indicate that we should fire (special to the ult gauss)
            WeaponAnimator.SetBool("FireUlt", true);
            StartCoroutine(ResetUltFire());
            CalculateUltShot();
            //AudioManager.playSound(GameConstants.Audio_Shotgun);
        }
        if (HasAmmo) {
            AmmoCheck();
        }
        WeaponAnimator.SetBool("UltModeActive", UltMode);
        //if (UltMode && Damage < 100) {
        //    Damage = 100;
        //} else {
        //    if (Damage != 25) {
        //        Damage = 25;
        //    }
        //}
    }

    /// <summary>
    /// Gauss Ultimate has a special different kind of bullet
    /// that needs to be charged and then fired
    /// </summary>
    private void CalculateUltShot() {
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

            //Wait for a quarter of a second for the animation to play then fire!
            GenerateShot(directionInput, hitNormal, WhatToHit, GameConstants.Layer_PlayerProjectile, true);
            GenerateCameraShake();
            TimeToSpawnEffect = Time.time + 1 / EffectSpawnRate;
            if (HasAmmo) {
                Ammo -= 1;
            }
            GameMaster.Instance.GetPlayerStatsUi(1).SetAmmo();
        }
    }

    private void IndicateHolding() {
        _holding = true;
    }

    private void ResetHoldCharge() {
        WeaponAnimator.SetBool("HoldCharge", false);
    } 

    /// <summary>
    /// Used to determine if the player is holding the trigger down so we shuold shoot at some fixed interval, or whether they're
    /// pressing the trigger rapid fire like and so we should fire every shot.
    /// If we are in ultimate mode - right now just shoot three bullets for every trigger pull.
    /// </summary>
    private void HandleShootingInput() {
        // Its pretty damn different logic for KB/M vs controllers
        print("JoystickManagerController.Instance.ConnectedControllers() = " + JoystickManagerController.Instance.ConnectedControllers());
        if (JoystickManagerController.Instance.ConnectedControllers() == 0) {
            if (Input.GetKeyUp(InputManager.Instance.Fire)) {
                if (_holding) {
                    CancelInvoke("IndicateHolding");
                    // Fire awesome charge shot!
                    WeaponAnimator.SetBool("HoldCharge", false);
                    ApplyRecoil();
                    CalculateUltShot();
                } else {
                    // Fire normal shot
                    print("SHOOT");
                    CancelInvoke("IndicateHolding");
                    CalculateShot();
                }
                _triggerPressed = false;
                _holding = false;
            } else if (_holding) {
                WeaponAnimator.SetBool("HoldCharge", true);
            }
            if (Input.GetKeyDown(InputManager.Instance.Fire) && !_holding) {
                _holdingQueued = true;
                // Wait 1/10th of a second to set the holding
                Invoke("IndicateHolding", 0.25f);
            }
        }else { 
            var rightTrigger = Input.GetAxis(Player.JoystickId + GameConstants.Input_RTrigger);
            // This indicates the user is pressing the trigger - must wait to determine if we are holding it 
            // or going to let it go
            if (_holding && (rightTrigger == 0)) {
                CancelInvoke("IndicateHolding");
                // Fire awesome charge shot!
                WeaponAnimator.SetBool("HoldCharge", false);
                ApplyRecoil();
                CalculateUltShot();
                _holdingQueued = false;
                _triggerPressed = false;
                _holding = false;
            } else if (_holding) {
                print("HOLDING");
                WeaponAnimator.SetBool("HoldCharge", true);
                //Invoke("ResetHoldCharge", 0.25f);
            } else if (rightTrigger > WeaponConfig.TriggerFireThreshold) {
                _triggerPressed = true;
            } else {
                if (_triggerPressed) {
                    // Fire normal shot
                    print("SHOOT");
                    CancelInvoke("IndicateHolding");
                    CalculateShot();
                }
                _holdingQueued = false;
                _triggerPressed = false;
                _holding = false;
            }
            if (_triggerPressed && !_holding && !_holdingQueued) {
                _holdingQueued = true;
                // Wait 1/10th of a second to set the holding
                Invoke("IndicateHolding", 0.25f);
            }
        }
        // This indicates the last frame we are holding the trigger
        //if (rightTrigger != 0 && _triggerLastFrame == rightTrigger) {
        //    print("HOLDING!");
        //    _triggerLastFrame = rightTrigger;
        //    return;
        //} else {
        //    if (rightTrigger > WeaponConfig.TriggerFireThreshold) {
        //        _triggerLastFrame = rightTrigger;
        //    }else if(_triggerLastFrame != rightTrigger) {
        //        // Fire?
        //        CalculateShot();
        //    }
        //}
        //// Indicates the user his not pressing the trigger nor the fire key
        //if (Input.GetKeyUp(InputManager.Instance.Fire) || rightTrigger == 0) {
        //    _fireButtonPressed = false;
        //    _holdingFireDown = false;
        //}
        //// Indicates the user is trying to fire
        //if ((Input.GetKeyUp(InputManager.Instance.Fire) || rightTrigger > WeaponConfig.TriggerFireThreshold)) {
        //    if (!_fireButtonPressed) {
        //        CalculateShot();
        //        _fireButtonPressed = true;
        //        _initialFirePressTime = Time.time + _fireHoldthreshold;
        //    }
        //    // They've been holding for longer than 0.5s so auto fire
        //    _holdingFireDown = _fireButtonPressed && (Time.time > _initialFirePressTime);

        //}
        //if (_holdingFireDown) {
        //    print("holding fire!");
        //    if (Time.time > TimeToFire) {
        //        TimeToFire = Time.time + 0.25f;
        //        CalculateShot();
        //    }
        //}
    }

    protected override void CalculateShot() {
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
            //Actually instantiate the effect
            GenerateShot(directionInput, hitNormal, WhatToHit, GameConstants.Layer_PlayerProjectile, UltMode);
            GenerateCameraShake();
            ApplyRecoil();
            TimeToSpawnEffect = Time.time + 1 / EffectSpawnRate;
            if (HasAmmo) {
                Ammo -= 1;
            }
            GameMaster.Instance.GetPlayerStatsUi(1).SetAmmo();
        }
    }

    /// <summary>
    /// Generate particle effect, spawn bullet, then destroy after allotted time
    /// </summary>
    /// <param name="shotPos"></param>
    /// <param name="shotNormal"></param>
    /// <param name="whatToHit"></param>
    protected override void GenerateShot(Vector3 shotPos, Vector3 shotNormal, LayerMask whatToHit, string layer, bool chargeShot, float freeFlyDelay = 0.5f) {
        //Fire the projectile - this will travel either out of the frame or hit a target - below should instantiate and destroy immediately
        var projRotation = CompensateQuaternion(FirePoint.rotation);
        var yUltOffset = 0.25f;
        var xUltOffset = 0.25f;
        // for (var i = 0; i < 1; ++i) {
        var i = 0;
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
            Transform bulletInstance = Instantiate(chargeShot ? UltBulletPrefab :  BulletPrefab, firePosition, projRotation) as Transform;
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
        //}
    }
}
