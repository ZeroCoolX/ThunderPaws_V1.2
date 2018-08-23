using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : PlayerLifeform {
    public PlayerStats PlayerStats;
    public int PlayerNumber;
    /// <summary>
    /// How we can identify which controller we need to query for input
    /// </summary>
    public string JoystickId;
    public Vector2 DirectionalInput { get; set; }
    public bool FacingRight = true;

    private float _meleeDamage = 10f;
    private bool _meleeActive = false;
    private float _meleeRaycastLength = 2.5f;
    private LayerMask _meleeLayerMask;
    private PlayerWeaponManager _weaponManager;
    /// <summary>
    /// How long the player is allowed to fall before they can no longer jump.
    /// This is in here since "platform mechanics" are not the core of the game.
    /// Shooting and chaos is - therefor I wanted to be more forgiving if the player misstimes jumps...but only up to 0.25s
    /// </summary>
    private float _allowedFallTime = 0.25f;
    /// <summary>
    /// Once the Time.time > this it indicates that the forgivenness window has passed and the player is falling to his death;
    /// </summary>
    private float _fallDelay;
    /// <summary>
    /// Need this flag to indicate when the player has touched back down after jumping to ensure
    /// there is not shenannigans in jumping in the 0-0.25s forgivenness zones
    /// </summary>
    private bool _jumped;

    private struct ActionMovementData {
        public float MaxTimeBetweenRoll;
        public float RollResetDelay;
        public bool RollActive;
        public float RollSpeed;
        public bool BounceBackActive;
        public float BounceBackDirection;
        public float BounceBackSpeed;
        public int BounceBackDamageTaken;
    }
    private ActionMovementData _actionMovement;



    public override bool Damage(float dmg) {
        PlayerStats.CurrentHealth -= (int)dmg;
        PlayerHudManager.Instance.UpdateHealthUI(PlayerNumber, PlayerStats.CurrentHealth, PlayerStats.MaxHealth);//TODO: Don't hardcode this
        if (PlayerStats.CurrentHealth <= 0) {
            GameMasterV2.KillPlayer(this);
        } else {
            ActivateFlash();
        }
        return false;
    }

    public void DeactivateUltimate() {
        // Since we know we're forcibly calling deactive - make sure the UI updates correctly
        PlayerStats.CurrentUltimate = 0;
        PlayerHudManager.Instance.UpdateUltimateUI(PlayerNumber, PlayerStats.CurrentUltimate, PlayerStats.MaxUltimate);

        // Stop the ultimate
        PlayerStats.UltEnabled = false;
        _weaponManager.ToggleUltimateForAllWeapons(false);
        CancelInvoke("DepleteUltimate");
    }

    public void ApplyWeaponPickup(string weaponkey) {
        _weaponManager.CreateAndEquipWeapon(weaponkey);
    }

    public void RegenerateAllHealth() {
        PlayerStats.CurrentHealth = PlayerStats.MaxHealth;
        PlayerHudManager.Instance.UpdateHealthUI(PlayerNumber, PlayerStats.CurrentHealth, PlayerStats.MaxHealth);
    }

    public void RemoveOtherWeapon(Transform weapon) {
        _weaponManager.RemoveOtherWeapon(weapon);
    }

    public void PickupCoin() {
        if (!PlayerStats.UltEnabled) {
            PlayerStats.CurrentUltimate += 1;
            PlayerHudManager.Instance.UpdateUltimateUI(PlayerNumber, PlayerStats.CurrentUltimate, PlayerStats.MaxUltimate);
        }
        GameStatsManager.Instance.AddCoin(PlayerNumber);
    }

    /// <summary>
    /// Delegate method fired from CollisionController2D which indicates
    /// the player collided with a baddie and should be damaged a little 
    /// and bounce back. -1(from left) - 1(from right)
    /// </summary>
    public void BounceBack(float directionFrom) {
        _actionMovement.BounceBackDirection = directionFrom * -1;
        _actionMovement.BounceBackActive = true;
    }

    public void OnJumpInputUp() {
        if (!(DirectionalInput.y < -0.25 || Input.GetKey(KeyCode.S))) {
            if (Velocity.y > MoveData.MinJumpVelocity) {
                Velocity.y = MoveData.MinJumpVelocity;
            }
        }
    }

    private void Start() {
        SetupActionMovement();
        SetupPlayerIdentification();
        InitializePhysicsValues(9f, 2.6f, 0.25f, 0.3f, 0.2f, 0.1f);
        SetupWeapons();
        SetupPlayerStats();

        // Bitshift the DAMAGEABLE layermask because that is what we want to hit
        // 14 = DAMAGEABLE
        _meleeLayerMask = 1 << 14;

        Controller2d.NotifyCollision += BounceBack;
    }

    private void SetupActionMovement() {
        _actionMovement.MaxTimeBetweenRoll = 0.3f;
        _actionMovement.RollResetDelay = 0f;
        _actionMovement.RollActive = false;
        _actionMovement.RollSpeed = 6f;
        _actionMovement.BounceBackActive = false;
        _actionMovement.BounceBackSpeed = 20;
        _actionMovement.BounceBackDamageTaken = 2;
    }

    private void SetupPlayerIdentification() {
        if (PlayerNumber == 0 || PlayerNumber > 2) {
            PlayerNumber = 1;
        }

        if (string.IsNullOrEmpty(JoystickId)) {
            JoystickId = "J1-";
        }
    }

    private void SetupWeapons() {
        _weaponManager = transform.GetComponent<PlayerWeaponManager>();
        if (_weaponManager == null) {
            throw new MissingComponentException("No PlayerWeaponManager found on Player object");
        }
        _weaponManager.InitializeWeapon(PlayerNumber, transform.Find(GameConstants.ObjectName_WeaponAnchor));
    }

    private void SetupPlayerStats() {
        PlayerStats = GetComponent<PlayerStats>();
        if (PlayerStats == null) {
            throw new MissingComponentException("No player stats found on the Player");
        }
        PlayerStats.MaxHealth = LivesManager.Health;
        PlayerStats.CurrentHealth = PlayerStats.MaxHealth;
        PlayerHudManager.Instance.UpdateHealthUI(PlayerNumber, PlayerStats.CurrentHealth, PlayerStats.MaxHealth);
        PlayerStats.CurrentUltimate = 0;
        PlayerHudManager.Instance.UpdateUltimateUI(PlayerNumber, PlayerStats.CurrentUltimate, PlayerStats.MaxUltimate);
    }

    private new void Update() {
        base.Update();

        FallCheck();

        // Do not accumulate gravity if colliding with anythig vertical
        if (Controller2d.Collisions.FromBelow || Controller2d.Collisions.FromAbove) {
            Velocity.y = 0;
        }
        if (_actionMovement.BounceBackActive) {
            ApplyBounceBack();
        } else {
            CalculateVelocityOffInput();
            ApplyGravity();
            Controller2d.Move(Velocity * Time.deltaTime, DirectionalInput, JoystickId);
            // Reset the jump indicator once we've made contact with the ground again
            if (_jumped && Controller2d.Collisions.FromBelow) {
                _jumped = false;
            }
        }

        CalculateMovementAnimation();
        CalcualteFacingDirection();
        CalculateWeaponRotation();
        DevelopmentHealthHack();
        CheckForUltimateUse();

        // Check for weapon switching
        if (Input.GetButtonUp(JoystickId + GameConstants.Input_LBumper) || Input.GetKeyUp(InputManager.Instance.ChangeWeapon)) {
            _weaponManager.SwitchWeapon();
        }
    }

    private void CalculateVelocityOffInput() {
        CalculateJumpVelocity();
        float targetVelocityX = CalculateHorizontalVelocity();
        if (BackwardsLevelProgressionAttempted()) {
            Velocity.x = 0;
        } else {
            Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref VelocityXSmoothing, Controller2d.Collisions.FromBelow ? MoveData.AccelerationTimeGrounded : MoveData.AccelerationTimeAirborne);
        }
    }

    private void CalculateJumpVelocity() {
        // This allows for a very small window of jumpability when falling
        if (!_jumped && !Controller2d.Collisions.FromBelow && Time.time >= _fallDelay) {
            _fallDelay = Time.time + _allowedFallTime;
        }

        // Check if user is trying to jump and is standing on the ground
        // We allow the player to jump if he's on the ground OR we're falling within 0.25 seconds
        if (!(DirectionalInput.y < -0.25 || Input.GetKey(KeyCode.S)) &&
            (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown(JoystickId + GameConstants.Input_Jump)) &&
            (Controller2d.Collisions.FromBelow || _fallDelay > Time.time)) {

            Velocity.y = MoveData.MaxJumpVelocity;
            _fallDelay = 0f;
            _jumped = true;
        }
    }

    private float CalculateHorizontalVelocity() {
        var yAxis = DirectionalInput.y;
        float targetVelocityX = 0f;
        var leftTrigger = Input.GetAxis(JoystickId + GameConstants.Input_LTrigger);
        var leftCtrl = Input.GetKey(InputManager.Instance.LockMovement);
        // Only set the movement speed if we're not holding L trigger, not looking straight up, not crouching, and not melee'ing
        if (!leftCtrl && leftTrigger < 1 && !_meleeActive) {
            if (DirectionalInput == new Vector2(1f, 1f) || DirectionalInput == new Vector2(-1f, 1f) || (yAxis <= 0.8 && yAxis > -0.25)) {
                // We have to handle the case of rolling so that different amounts on the x axis dont effect the roll speed
                // The roll speed should be a constant instead of relative to how far the user if pushing the joystick
                if (DirectionalInput.x != 0f && _actionMovement.RollActive) {
                    targetVelocityX = (_actionMovement.RollSpeed + MoveData.MoveSpeed) * (Mathf.Sign(DirectionalInput.x) > 0 ? 1 : -1);
                } else {
                    targetVelocityX = DirectionalInput.x * (MoveData.MoveSpeed + (_actionMovement.RollActive ? _actionMovement.RollSpeed : 0f));
                }
                // Set the animator
                Animator.SetFloat("xVelocity", targetVelocityX);
            }
        }
        return targetVelocityX;
    }

    private bool BackwardsLevelProgressionAttempted() {
        // Get the leftmost edge of the viewport and pad it
        var leftScreenEdge = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
        leftScreenEdge.x += 2;
        // Ensure our players x value is to the right of that to stop backwards traveral
        return ((transform.position.x - 1 <= leftScreenEdge.x) && DirectionalInput.x < 0);
    }

    private void ApplyBounceBack() {
        Damage(_actionMovement.BounceBackDamageTaken);
        Velocity.y = 0f;
        Velocity.x = Mathf.SmoothDamp(Velocity.x, (_actionMovement.BounceBackSpeed * _actionMovement.BounceBackDirection), ref VelocityXSmoothing, 0.1f);
        transform.Translate(Velocity * Time.deltaTime);
        Invoke("DeactivateBounceBackTrigger", 0.1f);
    }

    private void CheckForUltimateUse() {
        if ((Input.GetButtonUp(JoystickId + GameConstants.Input_Ultimate) || Input.GetKeyUp(InputManager.Instance.Ultimate)) && PlayerStats.UltReady) {
            if (!PlayerStats.UltEnabled) {
                ActivateUltimate();
            }
            GameStatsManager.Instance.AddUlt(PlayerNumber);
        }
    }

    private void CalculateMovementAnimation() {
        // Allows us to set the movement animation accurately
        var xVelocity = DirectionalInput.x * Convert.ToInt32(DirectionalInput.y <= 0.8 || (DirectionalInput == new Vector2(1f, 1f) || DirectionalInput == new Vector2(-1f, 1f)));
        var yVelocity = Velocity.y;

        var crouch = (DirectionalInput.y < -0.25 || Input.GetKey(KeyCode.S));
        var jumping = yVelocity > 0 && !crouch;
        var falling = !jumping && !Controller2d.Collisions.FromBelow;

        var rolling = (Time.time > _actionMovement.RollResetDelay) && (((Input.GetKeyDown(InputManager.Instance.Roll) || Input.GetButtonDown(JoystickId + GameConstants.Input_Roll)) && Controller2d.Collisions.FromBelow));
        if(rolling && !_actionMovement.RollActive) {
            _actionMovement.RollActive = true;
            Invoke("DeactivateRollTrigger", 0.25f);
        }

        var melee = ((Input.GetKeyDown(InputManager.Instance.Melee) || Input.GetButtonDown(JoystickId + GameConstants.Input_Melee)) && Controller2d.Collisions.FromBelow) || _meleeActive;
        if(melee && !_meleeActive) {
            _meleeActive = true;
            AudioManager.Instance.PlaySound(GameConstants.Audio_Melee);
            // Wait for half the animation to play so it looks like the object takes damage as the fist hits them instead of instantly on button press
            Invoke("OnMeleeInputDown", 0.125f);
            // After 0.25 seconds deactivate melee
            Invoke("DeactivateMeleeTrigger", 0.25f);
        }
      
        // Set the weapons inactive if either the roll or melee animation is playing
       // _weaponManager.ToggleWeaponActiveStatus(!_actionMovement.RollActive && !_meleeActive);

        PlayMovementAnimations(jumping, falling, crouch, melee, _actionMovement.RollActive, xVelocity);
    }

    private void PlayMovementAnimations(bool jumping, bool falling, bool crouch, bool melee, bool rolling, float xVelocity) {
        if (Animator != null) {
            Animator.SetBool("Jumping", jumping);
            Animator.SetBool("Falling", falling);
            Animator.SetBool("Crouching", crouch);
            Animator.SetBool("Melee", melee);
           //if(Animator.GetBool("Roll") == true ^ rolling) {
            Animator.SetBool("Roll", rolling);
            //}
            print("Rolling = " + rolling);
            _weaponManager.ToggleWeaponActiveStatus(!rolling && !melee);

            // The only time we want to be playing the run animation is if we are grounded, not holding the left trigger (or left ctrl), and not crouching nor pointing exactly upwards
            var finalXVelocity = Math.Abs(xVelocity) * VelocityBasedOffInput(crouch, jumping, falling);
            Animator.SetFloat("xVelocity", finalXVelocity);

            // Also inform the weapon animator that we are crouching
            _weaponManager.AnimateWeapon("Crouch", crouch);

            ChangeBoxColliderBasedOffActionMovement(crouch);

            // We want to hold still if any movement (even just pointing at different angles) is happeneing
            var holdStill = (Input.GetKey(InputManager.Instance.LockMovement) || Input.GetAxis(JoystickId + GameConstants.Input_LTrigger) >= 1 || finalXVelocity > 0 || crouch || jumping || falling || _meleeActive);
            _weaponManager.AnimateWeapon("HoldStill", holdStill);
        }
    }

    private int VelocityBasedOffInput(bool crouch, bool jumping, bool falling) {
        return (Convert.ToInt32(!Input.GetKey(InputManager.Instance.LockMovement))) * (Convert.ToInt32(Input.GetAxis(JoystickId + GameConstants.Input_LTrigger) < 1 || (DirectionalInput == new Vector2(1f, 1f) || DirectionalInput == new Vector2(-1f, 1f)))) * Convert.ToInt32(!crouch) * Convert.ToInt32(!jumping) * Convert.ToInt32(!falling) * Convert.ToInt32(!_meleeActive) * Convert.ToInt32(!_actionMovement.RollActive);
    }

    private void ChangeBoxColliderBasedOffActionMovement(bool crouch) {
        if (crouch || _actionMovement.RollActive) {
            Controller2d.BoxCollider.size = new Vector2(Controller2d.BoxCollider.size.x, GameConstants.Data_PlayerCrouchSize);
            Controller2d.BoxCollider.offset = new Vector2(Controller2d.BoxCollider.offset.x, GameConstants.Data_PlayerCrouchY);
        } else {
            Controller2d.BoxCollider.size = new Vector2(Controller2d.BoxCollider.size.x, GameConstants.Data_PlayerSize);
            Controller2d.BoxCollider.offset = new Vector2(Controller2d.BoxCollider.offset.x, GameConstants.Data_PlayerY);
        }
    }

    private void OnMeleeInputDown() {
        _meleeActive = true;
        RaycastHit2D raycast = Physics2D.Raycast(transform.position, FacingRight ? Vector3.right : Vector3.left, _meleeRaycastLength, _meleeLayerMask);
        if (raycast.collider != null) {
            var lifeform = raycast.collider.transform.GetComponent<DamageableLifeform>();
            if (lifeform != null && lifeform.gameObject.tag != GameConstants.Tag_Tutorial) {
                print("hit lifeform: " + lifeform.gameObject.name + " and did " + _meleeDamage + " damage");
                if (lifeform.Damage(_meleeDamage)) {
                    // Increment the stats for whoever shot the bullet
                    GameStatsManager.Instance.AddBaddie(PlayerNumber);
                }
            }
        }
    }

    /// <summary>
    /// Mirror the player graphics by inverting the .x local scale value
    /// </summary>
    private void CalcualteFacingDirection() {
        if(DirectionalInput.x == 0 || Mathf.Sign(transform.localScale.x) == Mathf.Sign(DirectionalInput.x)) {return;}

        // Switch the way the player is labelled as facing.
        FacingRight = Mathf.Sign(DirectionalInput.x) > 0;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// Rotates the weapon based off how the player is pushing the joystick
    /// </summary>
    private void CalculateWeaponRotation() {
        var yAxis = DirectionalInput.y;
        float rotation = 0f;
        if( ((yAxis > 0.3 && yAxis < 0.8)) || (DirectionalInput == new Vector2(1f, 1f) || DirectionalInput == new Vector2(-1f, 1f))) {
            rotation = 45 * (FacingRight ? 1 : -1);
        }else if (yAxis > 0.8) {
            rotation = 90 * (FacingRight ? 1 : -1);
        }
        _weaponManager.SetWeaponRotation(Quaternion.Euler(0f, 0f, rotation));

        int degree = (int)Mathf.Abs(rotation);
        DisplayCorrectSprite(degree);
    }

    /// <summary>
    /// This sets the sprite to one of 3 representing shooting in various degrees: (0deg, 45deg, 90deg)
    /// </summary>
    private void DisplayCorrectSprite(int degree) {
        transform.GetComponent<SpriteRenderer>().sprite = GameMasterV2.Instance.GetSpriteFromMap(degree);
    }

    private void ActivateUltimate() {
        _weaponManager.ToggleUltimateForAllWeapons(true);

        PlayerStats.UltEnabled = true;
        PlayerStats.UltReady = false;

        InvokeRepeating("DepleteUltimate", 0, 0.07f);
        // After 10 seconds deactivate ultimate
        Invoke("DeactivateUltimate", 7f);
    }

    private void DepleteUltimate() {
        --PlayerStats.CurrentUltimate;
        PlayerHudManager.Instance.UpdateUltimateUI(PlayerNumber, PlayerStats.CurrentUltimate, PlayerStats.MaxUltimate);
    }

    /// <summary>
    /// Helper method that ensures the player cannot walk during the melee animation
    /// </summary>
    private void DeactivateMeleeTrigger() {
        _meleeActive = false;
    }

    /// <summary>
    /// Helper method that ensures the player cannot walk during the roll animation
    /// </summary>
    private void DeactivateRollTrigger() {
        if(!(Input.GetKeyDown(InputManager.Instance.Roll) || Input.GetButtonDown(JoystickId + GameConstants.Input_Roll))) {
            _actionMovement.RollActive = false;
        }
        _actionMovement.RollResetDelay = Time.time + _actionMovement.MaxTimeBetweenRoll;
    }

    private void DeactivateBounceBackTrigger() {
        _actionMovement.BounceBackActive = false;
    }

    private void DevelopmentHealthHack() {
        if (Input.GetKeyDown(KeyCode.F)) {
            PlayerStats.MaxHealth = 500;
            PlayerStats.CurrentHealth = PlayerStats.MaxHealth;
            PlayerHudManager.Instance.UpdateHealthUI(PlayerNumber, PlayerStats.CurrentHealth, PlayerStats.MaxHealth);
        }
    }
}
