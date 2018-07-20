using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaussRifle : ProjectileWeapon {
    /// <summary>
    /// List from least special to most special bullet prefabs
    /// Access the indicies by using the BulletType enum
    /// </summary>
    public Transform[] BulletPrefabs;
    /// <summary>
    /// How long in seconds we want to delay trigger pulls from being registered.
    /// This stops from spamming when in Ult mode as that turned out to be incredibly
    /// overpowered
    /// Only allow 1 shot every 1/2 second.
    /// </summary>
    private float _ultShotDelay = 0.5f;
    /// <summary>
    /// Timer which keeps track of the our delays to space out shooting by the _ultShotDelay
    /// </summary>
    private float _timeTillUltAllowed;
    /// <summary>
    /// Struct containing manual hold calculation data
    /// </summary>
    private HoldingFireData _holdData;
    /// <summary>
    /// Indicates the last shot was a charged shot so we should take 5 bullets away
    /// </summary>
    private bool _chargeShotFired;
    private float _maxShotDelay;
    private float _maxTimeBetweenShots = 0.1f;

    /// <summary>
    /// Check if we should fire any of the 3 types of bullets we can shoot
    /// </summary>
    private void Update() {
        WeaponAnimator.SetBool("FireUlt", UltMode);
        if (UltMode) {
            if(Time.time > _timeTillUltAllowed) {
                if (Input.GetKeyDown(InputManager.Instance.Fire) || Input.GetAxis(Player.JoystickId + GameConstants.Input_RTrigger) > WeaponConfig.TriggerFireThreshold) {
                    // Fire normal shot
                    print("SHOOT");
                    BulletPrefab = BulletPrefabs[(int)BulletType.ULT];
                    AudioManager.Instance.playSound(GameConstants.Audio_GaussShotCharged);
                    CalculateShot();
                    _timeTillUltAllowed = Time.time + _ultShotDelay;
                }
            }
        } else {
            HandleShootingInput();
        }
        if (HasAmmo) {
            AmmoCheck();
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
    /// Helper method which allows other logic to use a delayed setting of the _holding variable which indicates the player 
    /// is holding the trigger.
    /// This method can also be cancelled before executing in case the player let go before we registered it as a "hold"
    /// </summary>
    private void IndicateHolding() {
        _holdData.Holding = true;
    }

    /// <summary>
    /// Overriding the base ProjectileWeapons ammo update because we want to allow for charged shot to cost extra shots
    /// </summary>
    protected override void UpdateAmmo() {
        if (HasAmmo) {
            Ammo -= (_chargeShotFired ? 5 : 1);
            PlayerHudManager.Instance.GetPlayerHud(Player.PlayerNumber).SetAmmo(Ammo);
        } else {
            PlayerHudManager.Instance.GetPlayerHud(Player.PlayerNumber).SetAmmo();
        }
    }

    /// <summary>
    /// Used to determine if the player is holding the trigger down so we shuold wait a small amount of time then fire a charged shot, or whether they're
    /// pressing the trigger rapid fire like and so we should fire every shot.
    /// </summary>
    private void HandleShootingInput() {
        // Its pretty damn different logic for KB/M vs controllers
        print("JoystickManagerController.Instance.ConnectedControllers() = " + JoystickManagerController.Instance.ConnectedControllers());
        if (JoystickManagerController.Instance.ConnectedControllers() == 0) {
            if (Input.GetKeyUp(InputManager.Instance.Fire)) {
                if (_holdData.Holding) {
                    CancelInvoke("IndicateHolding");
                    // Fire awesome charge shot!
                    WeaponAnimator.SetBool("HoldCharge", false);
                    _chargeShotFired = true;
                    // Set the bullet to the charge shot
                    BulletPrefab = BulletPrefabs[(int)BulletType.CHARGED];
                    AudioManager.Instance.playSound(GameConstants.Audio_GaussShotCharged);
                    CalculateShot();
                } else {
                    if (Time.time > _maxShotDelay) {
                        _maxShotDelay = Time.time + _maxTimeBetweenShots;
                        // Fire normal shot
                        print("SHOOT");
                        CancelInvoke("IndicateHolding");
                        _chargeShotFired = false;
                        BulletPrefab = BulletPrefabs[(int)BulletType.DEFAULT];
                        AudioManager.Instance.playSound(GameConstants.Audio_GaussShot);
                        CalculateShot();
                    }
                }
                _holdData.TriggerPressed = false;
                _holdData.Holding = false;
            } else if (_holdData.Holding) {
                WeaponAnimator.SetBool("HoldCharge", true);
            }
            if (Input.GetKeyDown(InputManager.Instance.Fire) && !_holdData.Holding) {
                _holdData.HoldingQueued = true;
                // Wait 1/10th of a second to set the holding
                Invoke("IndicateHolding", 0.25f);
            }
        }else { 
            var rightTrigger = Input.GetAxis(Player.JoystickId + GameConstants.Input_RTrigger);
            // This indicates the user is pressing the trigger - must wait to determine if we are holding it 
            // or going to let it go
            if (_holdData.Holding && (rightTrigger == 0)) {
                CancelInvoke("IndicateHolding");
                // Fire awesome charge shot!
                WeaponAnimator.SetBool("HoldCharge", false);
                _chargeShotFired = true;
                // Set the bullet to the charge shot
                BulletPrefab = BulletPrefabs[(int)BulletType.CHARGED];
                AudioManager.Instance.playSound(GameConstants.Audio_GaussShotCharged);
                CalculateShot();
                _holdData.HoldingQueued = false;
                _holdData.TriggerPressed = false;
                _holdData.Holding = false;
            } else if (_holdData.Holding) {
                print("HOLDING");
                WeaponAnimator.SetBool("HoldCharge", true);
            } else if (rightTrigger > WeaponConfig.TriggerFireThreshold) {
                _holdData.TriggerPressed = true;
            } else {
                if (_holdData.TriggerPressed) {
                    if (Time.time > _maxShotDelay) {
                        _maxShotDelay = Time.time + _maxTimeBetweenShots;
                        // Fire normal shot
                        print("SHOOT");
                        CancelInvoke("IndicateHolding");
                        _chargeShotFired = false;
                        // Set the bullet to the DEFAULT shot
                        BulletPrefab = BulletPrefabs[(int)BulletType.DEFAULT];
                        AudioManager.Instance.playSound(GameConstants.Audio_GaussShot);
                        CalculateShot();
                    }
                }
                _holdData.HoldingQueued = false;
                _holdData.TriggerPressed = false;
                _holdData.Holding = false;
            }
            if (_holdData.TriggerPressed && !_holdData.Holding && !_holdData.HoldingQueued) {
                _holdData.HoldingQueued = true;
                // Wait 1/10th of a second to set the holding
                Invoke("IndicateHolding", 0.25f);
            }
        }
    }

    /// <summary>
    /// All the necessary data used for manually calcualting if the player is holding the fire button.
    /// Cannot simply use .Key(up)(down) because I allow controller support  so I must detect it myself
    /// </summary>
    private struct HoldingFireData {
        /// <summary>
        /// Indicates the trigger was depressed
        /// </summary>
        public bool TriggerPressed;
        /// <summary>
        /// Indicates the trigger is currently being held down
        /// </summary>
        public bool Holding;
        /// <summary>
        /// INdicates we have not set the _holding variable and need to is we indicate a hold is
        /// happeneing
        /// </summary>
        public bool HoldingQueued;
    }
}
