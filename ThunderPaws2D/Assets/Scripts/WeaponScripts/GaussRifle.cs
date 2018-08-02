using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaussRifle : ProjectileWeapon {
    /// <summary>
    /// List from least special to most special bullet prefabs
    /// Access the indicies by using the BulletType enum from ProjectileWeapon.cs
    /// </summary>
    public Transform[] BulletPrefabs;

    /// <summary>
    /// How long in seconds we want to delay trigger pulls from being registered.
    /// This stops from spamming when in Ult mode as that turned out to be incredibly
    /// overpowered
    /// Only allow 1 shot every 1/2 second.
    /// </summary>
    private float _ultShotDelay = 0.5f;
    private float _timeTillUltAllowed;

    /// <summary>
    /// Indicates the last shot was a charged shot so we should take 5 bullets away
    /// </summary>
    private bool _chargeShotFired;
    private float _shotDelay = 0.1f;
    private float _timeTillShotAllowed;

    /// <summary>
    /// All the necessary data used for manually calcualting if the player is holding the fire button.
    /// Cannot simply use .Key(up)(down) because I allow controller support  so I must detect it myself
    /// </summary>
    private struct HoldingFireData {
        public bool TriggerPressed;
        public bool HoldingDownTrigger;
        // Indicates we have not set the _holding variable and need to if we indicate a hold is coming
        public bool HoldingQueued;
    }
    private HoldingFireData _holdData;



    protected override void ApplyRecoil() {
        WeaponAnimator.SetBool("ApplyRecoil", true);
        StartCoroutine(ResetWeaponPosition());
    }

    protected override void UpdateAmmo() {
        if (HasAmmo) {
            Ammo -= (_chargeShotFired ? 5 : 1);
            PlayerHudManager.Instance.GetPlayerHud(Player.PlayerNumber).SetAmmo(Ammo);
        } else {
            PlayerHudManager.Instance.GetPlayerHud(Player.PlayerNumber).SetAmmo();
        }
    }


    private void Update() {
        WeaponAnimator.SetBool("FireUlt", UltMode);

        if (UltMode) {
            HandleUltShootingInput();
        } else {
            ResolveShootingInput();
        }
        if (HasAmmo) {
            CheckAmmo();
        }
    }

    private void HandleUltShootingInput() {
        if (Time.time > _timeTillUltAllowed) {
            if (Input.GetKeyDown(InputManager.Instance.Fire) || Input.GetAxis(Player.JoystickId + GameConstants.Input_RTrigger) > WeaponConfig.TriggerFireThreshold) {
                BulletPrefab = BulletPrefabs[(int)BulletType.ULT];
                AudioManager.Instance.PlaySound(GameConstants.Audio_GaussShotCharged);
                FireShot();
                _timeTillUltAllowed = Time.time + _ultShotDelay;
            }
        }
    }

    /// <summary>
    /// Helper method which allows other logic to use a delayed setting of the _holding variable which indicates the player 
    /// is holding the trigger.
    /// This method can also be cancelled before executing in case the player let go before we registered it as a "hold".
    /// </summary>
    private void InvokeHolding() {
        _holdData.HoldingDownTrigger = true;
    }

    /// <summary>
    /// Used to determine if the player is holding the trigger down so we should wait a small amount of time then fire a charged shot, or whether they're
    /// pressing the trigger rapid fire like and so we should fire every shot.
    /// </summary>
    private void ResolveShootingInput() {
        if (JoystickManagerController.Instance.ConnectedControllers() == 0) {
            FireUsingKeyboard();
        } else {
            FireUsingController();
        }
    }

    private void FireUsingKeyboard() {
        if (Input.GetKeyUp(InputManager.Instance.Fire)) {
            if (_holdData.HoldingDownTrigger) {
                PrepareHoldShot();
            } else if(Time.time > _timeTillShotAllowed) {
                PrepareSingleShot();
            }
            FireShot();
            ResetData();
        } else if (_holdData.HoldingDownTrigger) {
            WeaponAnimator.SetBool("HoldCharge", true);
        }

        // Determine if we should queue a hold fire shot
        if (Input.GetKeyDown(InputManager.Instance.Fire) && !_holdData.HoldingDownTrigger) {
            _holdData.HoldingQueued = true;
            // Wait 1/10th of a second to set the holding
            Invoke("InvokeHolding", 0.25f);
        }
    }

    /// <summary>
    /// Fire an awesome charged up bullet that explodes on impact or after it reaches its max lifetime
    /// </summary>
    private void PrepareHoldShot() {
        CancelInvoke("InvokeHolding");
        WeaponAnimator.SetBool("HoldCharge", false);
        _chargeShotFired = true;
        BulletPrefab = BulletPrefabs[(int)BulletType.CHARGED];
        AudioManager.Instance.PlaySound(GameConstants.Audio_GaussShotCharged);
    }

    private void PrepareSingleShot() {
        _timeTillShotAllowed = Time.time + _shotDelay;
        CancelInvoke("InvokeHolding");
        _chargeShotFired = false;
        BulletPrefab = BulletPrefabs[(int)BulletType.DEFAULT];
        AudioManager.Instance.PlaySound(GameConstants.Audio_GaussShot);
    }

    private void FireUsingController() {
        var rightTrigger = Input.GetAxis(Player.JoystickId + GameConstants.Input_RTrigger);

        if (_holdData.HoldingDownTrigger && (rightTrigger == 0)) {
            PrepareHoldShot();
            FireShot();
            ResetData();
        } else if (_holdData.HoldingDownTrigger) {
            WeaponAnimator.SetBool("HoldCharge", true);
        } else if (rightTrigger > WeaponConfig.TriggerFireThreshold) {
            _holdData.TriggerPressed = true;
        } else {
            if (_holdData.TriggerPressed && Time.time > _timeTillShotAllowed) {
                PrepareSingleShot();
                FireShot();
            }
            ResetData();
        }

        // Determine if we should queue a hold fire shot
        if (_holdData.TriggerPressed && !_holdData.HoldingDownTrigger && !_holdData.HoldingQueued) {
            _holdData.HoldingQueued = true;
            // Wait 1/10th of a second to set the holding
            Invoke("InvokeHolding", 0.25f);
        }
    }

    private void ResetData() {
        _holdData.TriggerPressed = _holdData.HoldingDownTrigger = _holdData.HoldingQueued = false;
    }
}
