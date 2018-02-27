using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : AbstractLifeform {
    /// <summary>
    /// Constant for the default, always owned weapon
    /// </summary>
    public readonly string DEFAULT_WEAPON_NAME = "DefaultWeapon_jva";

    /// <summary>
    /// Currently equipped weapon
    /// </summary>
    private Transform _currentWeapon;
    
    /// <summary>
    /// List of weapons currently owned by the player.
    /// There will always be either 1 or 2. No more, no less
    /// </summary>
    private List<Transform> _ownedWeapons = new List<Transform>();

    /// <summary>
    /// This is where we create our weapons
    /// </summary>
    private Transform _weaponAnchor;

    /// <summary>
    /// Reference to the weapon anchor animator to play idle, crouch, and melee animations
    /// </summary>
    private Animator _weaponAnchorAnimator;

    /// <summary>
    /// Reference to user input either from a keyboard or controller
    /// </summary>
    public Vector2 DirectionalInput { get; set; }

    /// <summary>
    /// Indicates player is facing right
    /// </summary>
    public bool FacingRight = true;

    /// <summary>
    /// Quantatative data of stats. Visually the GameMaster handles that with the PlayerStatsUIController.
    /// </summary>
    private PlayerStats _playerStats;

    /// <summary>
    /// Indicates we are melee'ing and we shouldn't be able to move left or right during melee animation
    /// </summary>
    private bool _meleeActive = false;


    /// <summary>
    /// Setup Player object.
    /// Initialize physics values
    /// </summary>
    void Start() {
        //Set all physics values 
        InitializePhysicsValues(7f, 3f, 1f, 0.3f, 0.2f, 0.1f);

        _weaponAnchor = transform.Find("WeaponAnchor");
        _weaponAnchorAnimator = _weaponAnchor.GetComponent<Animator>();
        if(_weaponAnchorAnimator == null) {
            throw new MissingComponentException("The weapon anchor is missing an animator.");
        }
        CreateAndEquipWeapon(DEFAULT_WEAPON_NAME);

        if(_currentWeapon == null) {
            throw new MissingComponentException("There was no weapon attached to the Player");
        }
        //Add delegate for weapon switch notification from the GameMaster
        GameMaster.Instance.OnWeaponSwitch += SwitchWeapon;

        //Setup stats
        _playerStats = GetComponent<PlayerStats>();
        if(_playerStats == null) {
            throw new MissingComponentException("No player stats found on the Player");
        }
        _playerStats.CurrentHealth = _playerStats.MaxHealth;
        GameMaster.Instance.UpdateHealthUI(1, _playerStats.CurrentHealth, _playerStats.MaxHealth);//TODO: Hardcoded player number should be dynamic to whichever player this is
        _playerStats.CurrentUltimate = 0;
        GameMaster.Instance.UpdateUltimateUI(1, _playerStats.CurrentUltimate, _playerStats.MaxUltimate);//TODO: Hardcoded player number should be dynamic to whichever player this is
    }

    void Update() {
        //Do not accumulate gravity if colliding with anythig vertical
        if (Controller.Collisions.FromBelow || Controller.Collisions.FromAbove) {
            Velocity.y = 0;
        }
        CalculateVelocityOffInput();
        ApplyGravity();
        Controller.Move(Velocity * Time.deltaTime, DirectionalInput);
        CalculateMovementAnimation();
        CalcualteFacingDirection();
        CalculateWeaponRotation();

        //Completely for testing
        if (Input.GetKeyDown(KeyCode.Q)) {
            DamagePlayer();
        }

        //Completely for testing
        if (Input.GetKeyDown(KeyCode.F)) {
            RegenerateAllHealth();
        }

        //User is pressing the ultimate button - Inform the player
        if (Input.GetKeyUp(KeyCode.Joystick1Button3) && _playerStats.UltReady) {
            print("Pressing ult and we're ready!");
            ActivateUltimate();
        }
    }

    private void CalculateMovementAnimation() {
        // Allows us to set the running animation accurately
        var xVelocity = DirectionalInput.x * Convert.ToInt32(DirectionalInput.y <= 0.8);
        // Store the Y value for multiple uses
        var yVelocity = Velocity.y;
        // Indication we are jumping up
        var jumping = yVelocity > 0;
        // Indicates we are on the descent
        var falling = !jumping && !Controller.Collisions.FromBelow;
        // Indicates we are crouching
        var crouch = DirectionalInput.y < -0.25;
        // Indicates we are tail whipping
        var melee = ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Joystick1Button1)) && Controller.Collisions.FromBelow) || _meleeActive;
        if(melee && !_meleeActive) {
            _meleeActive = true;
            //After 0.25 seconds deactivate melee
            Invoke("DeactivateMeleeTrigger", 0.25f);
        }
        // Play running animation if the Animator exists on the lifeform 
        if (Animator != null) {
            Animator.SetBool("Jumping", jumping);
            Animator.SetBool("Falling", falling);
            Animator.SetBool("Crouching", crouch);
            Animator.SetBool("Melee", melee);
            // The only time we want to be playing the run animation is if we are grounded, not holding the left trigger, and not crouching nor pointing exactly upwards
            var finalXVelocity = Math.Abs(xVelocity) * Convert.ToInt32(Input.GetAxis("X360_Trigger_L") < 1) * Convert.ToInt32(!crouch) * Convert.ToInt32(!jumping) * Convert.ToInt32(!falling) * Convert.ToInt32(!_meleeActive);
            Animator.SetFloat("xVelocity", finalXVelocity);

            // Also inform the weapon animator that we are crouching
            _weaponAnchorAnimator.SetBool("Crouch", crouch);

            // We want to hold still if any movement (even just pointing ad different angles) is happeneing
            var holdStill = (Input.GetAxis("X360_Trigger_L") >= 1 || finalXVelocity > 0 || crouch || jumping || falling || _meleeActive);
            _weaponAnchorAnimator.SetBool("HoldStill", holdStill);
        }
    }

    /// <summary>
    /// Get the input from either the user 
    /// </summary>
    private void CalculateVelocityOffInput() {
        //check if user - or NPC - is trying to jump and is standing on the ground
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0)) && Controller.Collisions.FromBelow) {
            Velocity.y = MaxJumpVelocity;
        }
        var yAxis = DirectionalInput.y;
        float targetVelocityX = 0f;
        var leftTrigger = Input.GetAxis("X360_Trigger_L");
        // Only set the movement speed if we're not holding L trigger, not looking straight up, not crouching, and not melee'ing
        if (leftTrigger < 1 && yAxis <= 0.8 && yAxis > -0.25 && !_meleeActive) {
            targetVelocityX = DirectionalInput.x * MoveSpeed;
            // Set the animator
            Animator.SetFloat("xVelocity", targetVelocityX);
        }
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref VelocityXSmoothing, Controller.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
    }

    /// <summary>
    /// Helper method that handles variable jump height
    /// </summary>
    public void OnJumpInputUp() {
        if (Velocity.y > MinJumpVelocity) {
            Velocity.y = MinJumpVelocity;
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
        if( ((yAxis > 0.3 && yAxis < 0.8))) {
            rotation = 45 * (FacingRight ? 1 : -1);
        }else if (yAxis > 0.8) {
            rotation = 90 * (FacingRight ? 1 : -1);
        }
        _weaponAnchor.rotation = Quaternion.Euler(0f, 0f, rotation);

        int degree = (int)Mathf.Abs(rotation);
        DisplayCorrectSprite(degree);
    }

    /// <summary>
    /// This sets the sprite to one of 3 representing shooting in various degrees: (0deg, 45deg, 90deg)
    /// </summary>
    /// <param name="degree"></param>
    private void DisplayCorrectSprite(int degree) {
        transform.GetComponent<SpriteRenderer>().sprite = GameMaster.Instance.GetSpriteFromMap(degree);
    }

    /// <summary>
    /// Creates a new instance of the weapon and equips it. This is used for picking up the weapons on map
    /// </summary>
    /// <param name="weaponKey"></param>
    public void CreateAndEquipWeapon(string weaponKey) {
        if (weaponKey != DEFAULT_WEAPON_NAME) {
            _currentWeapon.gameObject.SetActive(false);
        }
        _currentWeapon = Instantiate(GameMaster.Instance.GetWeaponFromMap(weaponKey), _weaponAnchor.position, _weaponAnchor.rotation, _weaponAnchor);
        _currentWeapon.gameObject.SetActive(true);
        _ownedWeapons.Add(_currentWeapon);
        print("Created weapon: " + _currentWeapon.gameObject.name);
    }

    /// <summary>
    /// Indicates that the currently equipped weapon is out of ammo, should be removed from the players weapon list, and the defaault weapon ceaated if it doesn't exist and equipped
    /// </summary>
    /// <param name="weapon"></param>
    public void RemoveOtherWeapon(Transform weapon) {
        _ownedWeapons.Remove(weapon);
        Destroy(_currentWeapon.gameObject);
        _currentWeapon = _ownedWeapons[0];
        _currentWeapon.position = _weaponAnchor.position;
        _currentWeapon.gameObject.SetActive(true);

        if (_currentWeapon == null) {
            throw new KeyNotFoundException("ERROR: Default weapon was not found in weapon map");
        }
    }

    /// <summary>
    /// Switch current weapon to the other one if there is on.
    /// There is only 1 or 2 weapons so this is an easy calculation
    /// </summary>
    private void SwitchWeapon() {
        if(_ownedWeapons.Count > 1) {
            var rotation = _currentWeapon.rotation;
            _currentWeapon.gameObject.SetActive(false);
            //0 is default, 1 is other weapon
            var index = _ownedWeapons.IndexOf(_currentWeapon);
            _currentWeapon = _ownedWeapons[Mathf.Abs(-1 + index)];
            //Have to set the rotation of what the weapon was like before switching
            _currentWeapon.gameObject.SetActive(true);
        }
    }

    public void PickupCoin() {
        if (!_playerStats.UltEnabled) {
            _playerStats.CurrentUltimate += 5;
            GameMaster.Instance.UpdateUltimateUI(1, _playerStats.CurrentUltimate, _playerStats.MaxUltimate);//TODO: Hardcoded player number should be dynamic to whichever player this is
        }
        //Right now hardcoded for player 1 coins
        GameMaster.Instance.AddCoins(0);
    }

    /// <summary>
    ///  Enable/disable UltMode on all weapons owned   
    /// </summary>
    private void ActivateUltimate() {
        //Player has activated the ultimatet! (Pressed Y)
        if (!_playerStats.UltEnabled) {
            _playerStats.UltEnabled = true;
            _playerStats.UltReady = false;
            foreach(var weapon in _ownedWeapons) {
                weapon.GetComponent<PlayerWeapon>().UltMode = true;
            }
            InvokeRepeating("DepleteUltimate", 0, 0.07f);//100 max. 10 items a second = 1 item 1/10th of a second
            //After 10 seconds deactivate ultimate
            Invoke("DeactivateUltimate", 7f);
        }
    }

    private void DepleteUltimate() {
        --_playerStats.CurrentUltimate;
        GameMaster.Instance.UpdateUltimateUI(1, _playerStats.CurrentUltimate, _playerStats.MaxUltimate);
    }

    /// <summary>
    /// Set all weapon states to default mode.
    /// </summary>
    private void DeactivateUltimate() {
        _playerStats.UltEnabled = false;
        foreach (var weapon in _ownedWeapons) {
            weapon.GetComponent<PlayerWeapon>().UltMode = false;
        }
        CancelInvoke("DepleteUltimate");
    }

    /// <summary>
    /// Helper method that ensures the player cannot walk during the melee animation
    /// </summary>
    private void DeactivateMeleeTrigger() {
        _meleeActive = false;
    }

    /// <summary>
    /// Player takes damage and updates the status
    /// </summary>
    public void DamagePlayer() {
        _playerStats.CurrentHealth -= 5;
        GameMaster.Instance.UpdateHealthUI(1, _playerStats.CurrentHealth, _playerStats.MaxHealth);//TODO: Don't hardcode this
    }

    /// <summary>
    /// Player gets all health back. This only occurs at beginning of spawn and checkpoints
    /// </summary>
    public void RegenerateAllHealth() {
        _playerStats.CurrentHealth = _playerStats.MaxHealth;
        GameMaster.Instance.UpdateHealthUI(1, _playerStats.CurrentHealth, _playerStats.MaxHealth);//TODO: Don't hardcode this
    }

    /// <summary>
    /// Overriden method to apply gravity ourselves
    /// </summary>
    protected override void ApplyGravity() {
        Velocity.y += Gravity * Time.deltaTime;
    }

    public override void Damage(float dmg) {
        throw new NotImplementedException();
    }
}
