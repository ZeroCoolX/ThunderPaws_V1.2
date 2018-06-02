﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class specifically for Player Lifeforms.
/// </summary>
public abstract class PlayerLifeform : BaseLifeform {
    // TODO: Take this out but im so lazy
    public bool NoFallCheck = false;
    /// <summary>
    /// How fast the Lifeform moves
    /// </summary>
    protected float MoveSpeed;
    /// <summary>
    /// Min height Lifeform can jump
    /// </summary>
    protected float MinJumpHeight;
    /// <summary>
    /// Max height Lifeform can jump
    /// </summary>
    protected float MaxJumpHeight;
    /// <summary>
    /// How long it takes to reach JumpHeight
    /// </summary>
    protected float TimeToJumpApex;
    /// <summary>
    /// Used for dampening movenents
    /// </summary>
    protected float AccelerationTimeAirborne;
    /// <summary>
    /// Used for dampening movement
    /// </summary>
    protected float AccelerationTimeGrounded;

    /// <summary>
    /// Indicates how far we can fall without dying
    /// </summary>
    protected float FallDeathHeight = -18;
    /// <summary>
    /// Calculated based off gravity and jump constraints and player input (max)
    /// </summary>
    protected float MaxJumpVelocity;
    /// <summary>
    /// Calculated based off gravity and jump constraints and player input (min)
    /// </summary>
    protected float MinJumpVelocity;

    /// <summary>
    /// Just used as a reference for the Mathf.SmoothDamp function
    /// </summary>
    protected float VelocityXSmoothing;


    /// <summary>
    /// Every lifeform has at lesat 1 animation thats needed
    /// </summary>
    protected Animator Animator;


    /// <summary>
    /// Set all constant physics values
    /// Calculate dynamic values like Gravity and JumpVelocity
    /// </summary>
    /// <param name="moveSpeed"></param>
    /// <param name="jumpHeight"></param>
    /// <param name="timeToJumpApex"></param>
    /// <param name="accelerationTimeAirborne"></param>
    /// <param name="accelerationTimeGrounded"></param>
    protected void InitializePhysicsValues(float moveSpeed, float maxJumpHeight, float minJumpHeight, float timeToJumpApex, float accelerationTimeAirborne, float accelerationTimeGrounded, float gravity = -1) {
        MoveSpeed = moveSpeed;
        MinJumpHeight = minJumpHeight;
        MaxJumpHeight = maxJumpHeight;
        TimeToJumpApex = timeToJumpApex;
        AccelerationTimeAirborne = accelerationTimeAirborne;
        AccelerationTimeGrounded = accelerationTimeGrounded;
        //Phsyics controller used for all collision detection
        Controller2d = GetComponent<CollisionController2D>();
        //Calculate gravity and jump velocity
        if (gravity == -1) {
            // originally 2
            Gravity = -(2.5f * MaxJumpHeight) / Mathf.Pow(TimeToJumpApex, 2);
        } else {
            Gravity = gravity;
        }
        MaxJumpVelocity = Mathf.Abs(Gravity) * TimeToJumpApex;
        MinJumpVelocity = (maxJumpHeight == minJumpHeight ? MaxJumpVelocity : Mathf.Sqrt(2 * Mathf.Abs(Gravity) * minJumpHeight));
        print("Gravity: " + Gravity + "\n Jump Velocity: " + MaxJumpVelocity);

        // Set the animator
        Animator = GetComponent<Animator>();
        if(Animator == null) {
            print("Animator was not set - however this is allowed for not : just logging for notice purposes");
        }
    }

    protected void FallCheck() {
        if (NoFallCheck) {
            return;
        }
        // This is a super dumb hack to allow baddies to spawn in the last room because im so worn out I want to finish this
        if(transform.position.y <= FallDeathHeight && transform.position.y > -40f) {
            // Ensure nothing can survive
            Damage(999);
        }
    }
}