using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : PlayerLifeform {

    /// <summary>
    /// Indicates which player this is (either 1 or 2)
    /// </summary>
    public int PlayerNumber;
    /// <summary>
    /// How we can identify which controller we need to query for input
    /// </summary>
    public string JoystickId;

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
    /// Quantatative data of stats. Visually the GameMasterV2 handles that with the PlayerStatsUIController.
    /// </summary>
    public PlayerStats PlayerStats;

    /// <summary>
    /// Indicates we are melee'ing and we shouldn't be able to move left or right during melee animation
    /// </summary>
    private bool _meleeActive = false;
    /// <summary>
    /// How far should the melee hit
    /// </summary>
    private float _meleeRaycastLength = 2.5f;
    /// <summary>
    /// Specify which layers te melee should collide with
    /// </summary>
    private LayerMask _meleeLayerMask;
    /// <summary>
    /// How much damage to apply for each melee
    /// </summary>
    public float MeleeDamage = 10f;

    /// <summary>
    /// Indicates we are rolling
    /// </summary>
    private bool _rollActive = false;
    /// <summary>
    /// How fast the roll speed is
    /// </summary>
    private float _rollSpeed = 6f;

    /// <summary>
    /// Indicates we need to bounce back the player because they hit the baddie
    /// </summary>
    private bool _bounceBackActive;
    /// <summary>
    /// Indicates which direction we should bounce back
    /// - 1 indicates bounce left
    /// 1 indicates bounce right
    /// </summary>
    private float _bounceBackDirection;
    /// <summary>
    /// How hard of a bounce back occurrs when bounce back happens
    /// </summary>
    private float _bounceBackSpeed = 3f;
    /// <summary>
    /// A very small amount of damaage should be taken for colliding with a baddie
    /// </summary>
    private int _bounceBackDamage = 2;
    /// <summary>
    /// How long the player is allows to fall before they can no longer jump.
    /// This is in here since "platform mechanics" are not the core of the game.
    /// Shooting and chaos is - therefor I wanted to be more forgiving if the player misstimes jumps...but only up to 0.25s
    /// </summary>
    private float _allowedFallTime = 0.25f;
    /// <summary>
    /// Indicates that the forgivenness window has passed and the player is falling to his death;
    /// </summary>
    private float _fallDelay;
    /// <summary>
    /// Need this flag to indicate when the player has touched back down after jumping to ensure
    /// there is not shenannigans in jumping in the 0-0.25s forgivenness zones
    /// </summary>
    private bool _jumped;

    /// <summary>
    /// Setup Player object.
    /// Initialize physics values
    /// </summary>
    void Start() {

        if(PlayerNumber == 0 || PlayerNumber > 2) {
            PlayerNumber = 1;
            //throw new Exception("Somehow the player was either not set or too high with PlayerNumber of " + PlayerNumber);
        }

        if (string.IsNullOrEmpty(JoystickId)) {
            JoystickId = "J1-";
        }

        //Set all physics values  - originally 3 and 1
        InitializePhysicsValues(9f, 2.6f, 0.25f, 0.3f, 0.2f, 0.1f);

        _weaponAnchor = transform.Find(GameConstants.ObjectName_WeaponAnchor);
        _weaponAnchorAnimator = _weaponAnchor.GetComponent<Animator>();
        if(_weaponAnchorAnimator == null) {
            throw new MissingComponentException("The weapon anchor is missing an animator.");
        }
        CreateAndEquipWeapon(GameConstants.ObjectName_DefaultWeapon);

        if(_currentWeapon == null) {
            throw new MissingComponentException("There was no weapon attached to the Player");
        }
        //Add delegate for weapon switch notification from the GameMasterV2
        //GameMasterV2.Instance.OnWeaponSwitch += SwitchWeapon;

        //Setup stats
        PlayerStats = GetComponent<PlayerStats>();
        if(PlayerStats == null) {
            throw new MissingComponentException("No player stats found on the Player");
        }
        PlayerStats.MaxHealth = LivesManager.Health;
        PlayerStats.CurrentHealth = PlayerStats.MaxHealth;
        PlayerHudManager.Instance.UpdateHealthUI(PlayerNumber, PlayerStats.CurrentHealth, PlayerStats.MaxHealth);
        PlayerStats.CurrentUltimate = 0;
        PlayerHudManager.Instance.UpdateUltimateUI(PlayerNumber, PlayerStats.CurrentUltimate, PlayerStats.MaxUltimate);

        // Bitshift the DAMAGEABLE layermask because that is what we want to hit
        // 14 = DAMAGEABLE
        _meleeLayerMask = 1 << 14;

        Controller2d.NotifyCollision += BounceBack;
    }

    /// <summary>
    /// This is strictly used for testing
    /// </summary>
    public Transform[] ShowroomSpawns = new Transform[4];
    private int spawnIndex = -1;

    private void HackRollReset() {
        _rollActive = false;
    }

    void Update() {
        base.Update();

        //print("Querying for inputs with prefix : " + JoystickId);
        if (Input.GetKeyUp(KeyCode.R)) {
            HackRollReset();
        }

        FallCheck();

        //Do not accumulate gravity if colliding with anythig vertical
        if (Controller2d.Collisions.FromBelow || Controller2d.Collisions.FromAbove) {
            Velocity.y = 0;
        }
        if (_bounceBackActive) {
            print("BOUNCE BACK");
            Damage(_bounceBackDamage);
            Velocity.x = _bounceBackSpeed *_bounceBackDirection;
            Velocity.y = 0f;
            transform.Translate(Velocity);
            _bounceBackActive = false;
        }else {
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

        //Completely for testing
        if (Input.GetKeyDown(KeyCode.F)) {
            PlayerStats.MaxHealth = 500;
            PlayerStats.CurrentHealth = PlayerStats.MaxHealth;
            PlayerHudManager.Instance.UpdateHealthUI(PlayerNumber, PlayerStats.CurrentHealth, PlayerStats.MaxHealth);
        }

        //User is pressing the ultimate button - Inform the player
        if ((Input.GetButtonUp(JoystickId + GameConstants.Input_Ultimate) || Input.GetKeyUp(InputManager.Instance.Ultimate)) && PlayerStats.UltReady) {
            print("Pressing ult and we're ready!");
            ActivateUltimate();
            GameStatsManager.Instance.AddUlt(PlayerNumber);
        }

        if (Input.GetButtonUp(JoystickId + GameConstants.Input_LBumper) || Input.GetKeyUp(InputManager.Instance.ChangeWeapon)) {
            SwitchWeapon();
            AudioManager.Instance.playSound(GameConstants.Audio_WeaponSwitch);
        }
    }

    /// <summary>
    /// Delegate method fired from CollisionController2D which indicates
    /// the player collided with a baddie and should be damaged a little 
    /// and bounce back
    /// </summary>
    /// <param name="directionFrom"></param>
    public void BounceBack(float directionFrom) {
        // directionFrom == -1 ? from left
        // directionFrom != -1 ? from right
        _bounceBackDirection = directionFrom * -1;
        _bounceBackActive = true;
    }

    private void CalculateMovementAnimation() {
        // Allows us to set the running animation accurately
        var xVelocity = DirectionalInput.x * Convert.ToInt32(DirectionalInput.y <= 0.8 || (DirectionalInput == new Vector2(1f, 1f) || DirectionalInput == new Vector2(-1f, 1f)));
        // Store the Y value for multiple uses
        var yVelocity = Velocity.y;
        // Indicates we are crouching
        var crouch = (DirectionalInput.y < -0.25 || Input.GetKey(KeyCode.S));
        // Indication we are jumping up
        var jumping = yVelocity > 0 && !crouch;
        // Indicates we are on the descent
        var falling = !jumping && !Controller2d.Collisions.FromBelow;

        // Indicates we are rolling
        var rolling = ((Input.GetKeyDown(InputManager.Instance.Roll) || Input.GetButtonDown(JoystickId + GameConstants.Input_Roll)) && Controller2d.Collisions.FromBelow) || _rollActive;
        if(rolling && !_rollActive) {
            _rollActive = true;
            Invoke("DeactivateRollTrigger", 0.25f);
        }
        // Indicates we are melee'ing
        var melee = ((Input.GetKeyDown(InputManager.Instance.Melee) || Input.GetButtonDown(JoystickId + GameConstants.Input_Melee)) && Controller2d.Collisions.FromBelow) || _meleeActive;
        if(melee && !_meleeActive) {
            _meleeActive = true;
            AudioManager.Instance.playSound(GameConstants.Audio_Melee);
            // Wait for half the animation to play so it looks like the object takes damage as the fist hits them instead of instantly on button press
            Invoke("OnMeleeInputDown", 0.125f);
            // After 0.25 seconds deactivate melee
            Invoke("DeactivateMeleeTrigger", 0.25f);
        }
        // Play running animation if the Animator exists on the lifeform 
        if (Animator != null) {
            Animator.SetBool("Jumping", jumping);
            Animator.SetBool("Falling", falling);
            Animator.SetBool("Crouching", crouch);
            Animator.SetBool("Melee", melee);
            Animator.SetBool("Roll", rolling);
            // The only time we want to be playing the run animation is if we are grounded, not holding the left trigger (or left ctrl), and not crouching nor pointing exactly upwards
            var finalXVelocity = Math.Abs(xVelocity) * (Convert.ToInt32(!Input.GetKey(InputManager.Instance.LockMovement))) * (Convert.ToInt32(Input.GetAxis(JoystickId + GameConstants.Input_LTrigger) < 1 || (DirectionalInput == new Vector2(1f, 1f) || DirectionalInput == new Vector2(-1f, 1f)))) * Convert.ToInt32(!crouch) * Convert.ToInt32(!jumping) * Convert.ToInt32(!falling) * Convert.ToInt32(!_meleeActive) * Convert.ToInt32(!_rollActive);
            Animator.SetFloat("xVelocity", finalXVelocity);

            // Also inform the weapon animator that we are crouching
            _weaponAnchorAnimator.SetBool("Crouch", crouch);
            if (crouch || _rollActive) {
                Controller2d.BoxCollider.size = new Vector2(Controller2d.BoxCollider.size.x, GameConstants.Data_PlayerCrouchSize);
                Controller2d.BoxCollider.offset = new Vector2(Controller2d.BoxCollider.offset.x, GameConstants.Data_PlayerCrouchY);
            } else {
                Controller2d.BoxCollider.size = new Vector2(Controller2d.BoxCollider.size.x, GameConstants.Data_PlayerSize);
                Controller2d.BoxCollider.offset = new Vector2(Controller2d.BoxCollider.offset.x, GameConstants.Data_PlayerY);
            }

            // We want to hold still if any movement (even just pointing ad different angles) is happeneing
            var holdStill = (Input.GetKey(InputManager.Instance.LockMovement) || Input.GetAxis(JoystickId + GameConstants.Input_LTrigger) >= 1 || finalXVelocity > 0 || crouch || jumping || falling || _meleeActive);
            _weaponAnchorAnimator.SetBool("HoldStill", holdStill);
        }

        // Set the weapons inactive if either the roll or melee animation is playing
        _currentWeapon.gameObject.SetActive(!_rollActive && !_meleeActive);
    }

    /// <summary>
    /// Get the input from either the user 
    /// </summary>
    private void CalculateVelocityOffInput() {
        // This allows for a very small window of jumpability when falling
        if (!_jumped && !Controller2d.Collisions.FromBelow && Time.time >= _fallDelay) {
            _fallDelay = Time.time + _allowedFallTime;
        }

        //check if user - or NPC - is trying to jump and is standing on the ground
        if (!(DirectionalInput.y < -0.25 || Input.GetKey(KeyCode.S)) &&                          // We allow the player to jump if he's on the ground OR we're falling within 0.25 seconds
                (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown(JoystickId + GameConstants.Input_Jump)) && (Controller2d.Collisions.FromBelow || _fallDelay > Time.time)) {
            Velocity.y = MaxJumpVelocity;
            _fallDelay = 0f;
            _jumped = true;
        }

        var yAxis = DirectionalInput.y;
        float targetVelocityX = 0f;
        var leftTrigger = Input.GetAxis(JoystickId + GameConstants.Input_LTrigger);
        var leftCtrl = Input.GetKey(InputManager.Instance.LockMovement);
        // Only set the movement speed if we're not holding L trigger, not looking straight up, not crouching, and not melee'ing
        if (!leftCtrl && leftTrigger < 1  && !_meleeActive) {
            if(DirectionalInput == new Vector2(1f, 1f) || DirectionalInput == new Vector2(-1f, 1f)) {
                // We have to handle the case of rolling so that different amounts on the x axis dont effect the roll speed
                // The roll speed should be a constant instead of relative to how far the user if pushing the joystick
                if (DirectionalInput.x != 0f && _rollActive) {
                    targetVelocityX = (_rollSpeed + MoveSpeed) * (Mathf.Sign(DirectionalInput.x) > 0 ? 1 : -1);
                } else {
                    targetVelocityX = DirectionalInput.x * (MoveSpeed + (_rollActive ? _rollSpeed : 0f));
                }
                // Set the animator
                Animator.SetFloat("xVelocity", targetVelocityX);
            }
            else if ((yAxis <= 0.8 && yAxis > -0.25)) {
                // We have to handle the case of rolling so that different amounts on the x axis dont effect the roll speed
                // The roll speed should be a constant instead of relative to how far the user if pushing the joystick
                if (DirectionalInput.x != 0f && _rollActive) {
                    targetVelocityX = (_rollSpeed + MoveSpeed) * (Mathf.Sign(DirectionalInput.x) > 0 ? 1 : -1);
                } else {
                    targetVelocityX = DirectionalInput.x * (MoveSpeed + (_rollActive ? _rollSpeed : 0f));
                }
                // Set the animator
                Animator.SetFloat("xVelocity", targetVelocityX);
            }
        }
        // Get the leftmost edge of the viewport 
        var leftScreenEdge = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
        // Pad it to the right by 5
        leftScreenEdge.x += 2;
        // Ensure our players x value is to the right of that to stop backwards traveral
        if ( ( transform.position.x - 1 <= leftScreenEdge.x ) && DirectionalInput.x < 0) {
            Velocity.x = 0;
        }else {
            Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref VelocityXSmoothing, Controller2d.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        }
    }

    /// <summary>
    /// Helper method that handles variable jump height
    /// </summary>
    public void OnJumpInputUp() {
        if (!(DirectionalInput.y < -0.25 || Input.GetKey(KeyCode.S))) {
            if (Velocity.y > MinJumpVelocity) {
                Velocity.y = MinJumpVelocity;
            }
        }
    }

    /// <summary>
    /// Handles Melee logic
    /// Fire raycast out in forward facing direction
    /// If it hits anything, damage the thing - only lifeforms.
    /// </summary>
    public void OnMeleeInputDown() {
        _meleeActive = true;
        print("Melee'ing");
        //Mini raycast to check handle ellusive targets
        RaycastHit2D raycast = Physics2D.Raycast(transform.position, FacingRight ? Vector3.right : Vector3.left, _meleeRaycastLength, _meleeLayerMask);
        //We want to allow bullets to pass throught obstacles that the player can pass through
        if (raycast.collider != null) {
            //IF we hit a lifeform damage it - otherwise move on
            var lifeform = raycast.collider.transform.GetComponent<DamageableLifeform>();
            if (lifeform != null && lifeform.gameObject.tag != GameConstants.Tag_Tutorial) {
                print("hit lifeform: " + lifeform.gameObject.name + " and did " + MeleeDamage + " damage");
                if (lifeform.Damage(MeleeDamage)) {
                    // increment the stats for whoever shot the bullet
                    print("Adding baddie for player  : " + PlayerNumber);
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
        _weaponAnchor.rotation = Quaternion.Euler(0f, 0f, rotation);

        int degree = (int)Mathf.Abs(rotation);
        DisplayCorrectSprite(degree);
    }

    /// <summary>
    /// This sets the sprite to one of 3 representing shooting in various degrees: (0deg, 45deg, 90deg)
    /// </summary>
    /// <param name="degree"></param>
    private void DisplayCorrectSprite(int degree) {
        transform.GetComponent<SpriteRenderer>().sprite = GameMasterV2.Instance.GetSpriteFromMap(degree);
    }

    /// <summary>
    /// Creates a new instance of the weapon and equips it. This is used for picking up the weapons on map
    /// If a special weapon is already equipped this weapon should remove that weapon and take its place
    /// </summary>
    /// <param name="weaponKey"></param>
    public void CreateAndEquipWeapon(string weaponKey) {
        if (weaponKey != GameConstants.ObjectName_DefaultWeapon) {
            _currentWeapon.gameObject.SetActive(false);
        }
        _currentWeapon = Instantiate(GameMasterV2.Instance.GetWeaponFromMap(weaponKey), _weaponAnchor.position, _weaponAnchor.rotation, _weaponAnchor);
        _currentWeapon.gameObject.SetActive(true);
        if(_ownedWeapons.Count == 2) {
            var previousWeapon = _ownedWeapons[1];
            Destroy(previousWeapon.gameObject);
            _ownedWeapons[1] = _currentWeapon;
        }else {
            _ownedWeapons.Add(_currentWeapon);
        }
        print("Created weapon: " + _currentWeapon.gameObject.name);
        PlayerHudManager.Instance.UpdateWeaponPickup(PlayerNumber, weaponKey);
        PlayerHudManager.Instance.GetPlayerHud(PlayerNumber).SetAmmo(_currentWeapon.GetComponent<AbstractWeapon>().Ammo);
        try {
            AudioManager.Instance.playSound(GameConstants.Audio_WeaponPickup);
        }catch(System.Exception e) {
            print("Either the game master or the audiomanager doesn't exist yet");
        }
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
        PlayerHudManager.Instance.UpdateWeaponPickup(PlayerNumber, "fuzzbuster");
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
        if (!PlayerStats.UltEnabled) {
            PlayerStats.CurrentUltimate += 1;
            PlayerHudManager.Instance.UpdateUltimateUI(PlayerNumber, PlayerStats.CurrentUltimate, PlayerStats.MaxUltimate);//TODO: Hardcoded player number should be dynamic to whichever player this is
        }
        //Right now hardcoded for player 1 coins
        print("Adding coin!");
        GameStatsManager.Instance.AddCoin(PlayerNumber);
    }

    /// <summary>
    ///  Enable/disable UltMode on all weapons owned   
    /// </summary>
    private void ActivateUltimate() {
        //Player has activated the ultimatet! (Pressed Y)
        if (!PlayerStats.UltEnabled) {
            PlayerStats.UltEnabled = true;
            PlayerStats.UltReady = false;
            foreach(var weapon in _ownedWeapons) {
                weapon.GetComponent<AbstractWeapon>().UltMode = true;
            }
            InvokeRepeating("DepleteUltimate", 0, 0.07f);//100 max. 10 items a second = 1 item 1/10th of a second
            //After 10 seconds deactivate ultimate
            Invoke("DeactivateUltimate", 7f);
        }
    }

    private void DepleteUltimate() {
        --PlayerStats.CurrentUltimate;
        PlayerHudManager.Instance.UpdateUltimateUI(PlayerNumber, PlayerStats.CurrentUltimate, PlayerStats.MaxUltimate);
    }

    /// <summary>
    /// Set all weapon states to default mode.
    /// </summary>
    public void DeactivateUltimate() {
        // Since we know we're forcibly calling deactive - make sure the UI updates correctly
        PlayerStats.CurrentUltimate = 0;
        PlayerHudManager.Instance.UpdateUltimateUI(PlayerNumber, PlayerStats.CurrentUltimate, PlayerStats.MaxUltimate);

        // Stop the ultimate
        PlayerStats.UltEnabled = false;
        foreach (var weapon in _ownedWeapons) {
            weapon.GetComponent<AbstractWeapon>().UltMode = false;
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
    /// Helper method that ensures the player cannot walk during the melee animation
    /// </summary>
    private void DeactivateRollTrigger() {
        _rollActive = false;
    }

    /// <summary>
    /// Player takes damage and updates the status
    /// </summary>
    public override bool Damage(float dmg) {
        PlayerStats.CurrentHealth -= (int)dmg;
        PlayerHudManager.Instance.UpdateHealthUI(PlayerNumber, PlayerStats.CurrentHealth, PlayerStats.MaxHealth);//TODO: Don't hardcode this
        if(PlayerStats.CurrentHealth <= 0) {
            GameMasterV2.KillPlayer(this);
        }else {
            ActivateFlash();
        }
        return false;
    }

    /// <summary>
    /// Player gets all health back. This only occurs at beginning of spawn and checkpoints
    /// </summary>
    public void RegenerateAllHealth() {
        PlayerStats.CurrentHealth = PlayerStats.MaxHealth;
        PlayerHudManager.Instance.UpdateHealthUI(PlayerNumber, PlayerStats.CurrentHealth, PlayerStats.MaxHealth);
    }
}
