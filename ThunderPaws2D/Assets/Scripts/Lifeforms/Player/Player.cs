using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : AbstractLifeform {

    private Transform _currentWeapon;

    /// <summary>
    /// Reference to user input either from a keyboard or controller
    /// </summary>
    public Vector2 DirectionalInput { get; set; }

    /// <summary>
    /// Indicates player is facing right
    /// </summary>
    public bool FacingRight = true;

    /// <summary>
    /// Setup Player object.
    /// Initialize physics values
    /// </summary>
    void Start() {
        //Set all physics values 
        InitializePhysicsValues(7f, 3f, 1f, 0.3f, 0.2f, 0.1f);

        _currentWeapon = transform.Find("WeaponAnchor").Find("gun_1.2");//Name will not be harded in the future
        if(_currentWeapon == null) {
            throw new MissingComponentException("There was no weapon attached to the Player");
        }
    }

    void Update() {
        //Do not accumulate gravity if colliding with anythig vertical
        if (Controller.Collisions.FromBelow || Controller.Collisions.FromAbove) {
            Velocity.y = 0;
        }
        CalculateVelocityOffInput();
        ApplyGravity();
        //Animator.SetFloat("vSpeed", Velocity.y);
        Controller.Move(Velocity * Time.deltaTime, DirectionalInput);
        CalcualteFacingDirection();
        CalculateWeaponRotation();
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
        if (leftTrigger < 1 && yAxis <= 0.8) {
            targetVelocityX = DirectionalInput.x * MoveSpeed;
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
        _currentWeapon.rotation = Quaternion.Euler(0f, 0f, rotation);

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
    /// Overriden method to apply gravity ourselves
    /// </summary>
    protected override void ApplyGravity() {
        Velocity.y += Gravity * Time.deltaTime;
    }
}
