using UnityEngine;

public class FuzzBuster : ProjectileWeapon {
    /// <summary>
    /// Must store the initial time the user pressed the button
    /// </summary>
    private float _initialFirePressTime;
    /// <summary>
    /// Struct containing manual hold calculation data
    /// </summary>
    private HoldingFireData _holdData;

    private void Update() {
        HandleShootingInput();

        if (HasAmmo) {
            AmmoCheck();
        }

        WeaponAnimator.SetBool("UltModeActive", UltMode);
    }

    /// <summary>
    /// Implementation specific override.
    /// Apply the recoil animation and reset the weapon position.
    /// Play audio
    /// </summary>
    protected override void ApplyRecoil() {
        WeaponAnimator.SetBool("ApplyRecoil", true);
        StartCoroutine(ResetWeaponPosition());
        AudioManager.playSound(GameConstants.Audio_FuzzBuster);
    }

    /// <summary>
    /// Used to determine if the player is holding the trigger down so we shuold shoot at some fixed interval, or whether they're
    /// pressing the trigger rapid fire like and so we should fire every shot.
    /// If we are in ultimate mode - right now just shoot three bullets for every trigger pull.
    /// </summary>
    private void HandleShootingInput() {
        var rightTrigger = Input.GetAxis(Player.JoystickId + GameConstants.Input_RTrigger);
        // Indicates the user his not pressing the trigger nor the fire key
        if (Input.GetKeyUp(InputManager.Instance.Fire)) {
            _holdData.TriggerPressed = false;
            _holdData.Holding = false;
        } else if (!Input.GetKey(InputManager.Instance.Fire) && rightTrigger == 0) {
            _holdData.TriggerPressed = false;
            _holdData.Holding = false;
        }
        // Indicates the user is trying to fire
        if ((Input.GetKey(InputManager.Instance.Fire) || rightTrigger > WeaponConfig.TriggerFireThreshold)) {
            if (!_holdData.TriggerPressed) {
                CalculateShot(UltMode ? 3 : 1);
                _holdData.TriggerPressed = true;
                _initialFirePressTime = Time.time + FuzzBusterConfig.AutoFireSpacing;
            }
            // They've been holding for longer than 0.5s so auto fire
            _holdData.Holding = _holdData.TriggerPressed && (Time.time > _initialFirePressTime);

        }
        if (_holdData.Holding) {
            if (Time.time > TimeToFire) {
                TimeToFire = Time.time + 0.25f;
                CalculateShot(UltMode ? 3 : 1);
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
    }
}
