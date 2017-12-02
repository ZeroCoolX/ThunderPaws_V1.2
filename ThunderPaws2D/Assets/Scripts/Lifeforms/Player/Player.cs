using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : AbstractLifeform {

    /// <summary>
    /// Reference to user input either from a keyboard or controller
    /// </summary>
    public Vector2 DirectionalInput;

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
    }

    /// <summary>
    /// Store the player input 
    /// </summary>
    /// <param name="input"></param>
    public void SetDirectionalInput(Vector2 input) {
        DirectionalInput = input;
    }

    public Vector2 GetDirectionalInput() {
        return DirectionalInput;
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
    }

    /// <summary>
    /// Get the input from either the user 
    /// </summary>
    private void CalculateVelocityOffInput() {
        //check if user - or NPC - is trying to jump and is standing on the ground
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0)) && Controller.Collisions.FromBelow) {
            Velocity.y = MaxJumpVelocity;
        }
        float targetVelocityX = DirectionalInput.x * MoveSpeed;
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
    /// Overriden method to apply gravity ourselves
    /// </summary>
    protected override void ApplyGravity() {
        Velocity.y += Gravity * Time.deltaTime;
    }
}
