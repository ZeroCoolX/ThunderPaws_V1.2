using UnityEngine;

public class FuzzBuster : ProjectileWeapon {

    private HoldingFireData _holdData;
    private float _initialFirePressTime = 0;
    private float _maxTimeBetweenShots = 0.1f;
    private float _maxShotDelay = 0;


    private void Update() {
        HandleShootingInput();
        if (HasAmmo) {
            CheckAmmo();
        }
        WeaponAnimator.SetBool("UltModeActive", UltMode);
    }

    protected override void ApplyRecoil() {
        WeaponAnimator.SetBool("ApplyRecoil", true);
        StartCoroutine(ResetWeaponPosition());
        AudioManager.playSound(GameConstants.Audio_FuzzBuster);
    }

    /// <summary>
    /// Used to determine if the player is holding the trigger down so we should shoot at some fixed interval, or whether they're
    /// pressing the trigger rapid fire like and so we should fire every shot.
    /// If we are in ultimate mode - right now just shoot three bullets for every trigger pull.
    /// </summary>
    private void HandleShootingInput() {
        var rightTrigger = Input.GetAxis(Player.JoystickId + GameConstants.Input_RTrigger);

        if (!isUserTryingToFire(rightTrigger)) {
            _holdData.TriggerPressed = false;
            _holdData.HoldingDownTrigger = false;
        }

        if (isUserTryingToFire(rightTrigger)) {
            if (!_holdData.TriggerPressed && Time.time > _maxShotDelay) {
                FireSingleShot();
            }
            // They've been holding for longer than 0.5s so auto fire
            _holdData.HoldingDownTrigger = _holdData.TriggerPressed && (Time.time > _initialFirePressTime);
        }

        if (_holdData.HoldingDownTrigger) {
            FireHoldShot();
        } 
    }

    private void FireSingleShot() {
        _maxShotDelay = Time.time + _maxTimeBetweenShots;
        FireShot(UltMode ? 3 : 1);
        _holdData.TriggerPressed = true;
        _initialFirePressTime = Time.time + FuzzBusterConfig.AutoFireSpacing;
    }

    private void FireHoldShot() {
        if (Time.time > TimeToFire) {
            TimeToFire = Time.time + 0.25f;
            FireShot(UltMode ? 3 : 1);
        }
    }

    private bool isUserTryingToFire(float triggerInput) {
        if((Input.GetKey(InputManager.Instance.Fire) || triggerInput > WeaponConfig.TriggerFireThreshold)) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// All the necessary data used for manually calculating if the player is holding the fire button.
    /// Cannot simply use .Key(up)(down) because I allow controller support  so I must detect it myself
    /// </summary>
    private struct HoldingFireData {
        public bool TriggerPressed;
        public bool HoldingDownTrigger;
    }
}
