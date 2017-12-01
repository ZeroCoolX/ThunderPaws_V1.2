using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Lifeform {


    /// <summary>
    /// Store the player input 
    /// </summary>
    /// <param name="input"></param>
    public void SetDirectionalInput(Vector2 input) {
        //DirectionalInput = input;
    }

    /// <summary>
    /// Helper method that handles variable jump height
    /// </summary>
    public void OnJumpInputUp() {
        if (Velocity.y > MinJumpVelocity) {
            Velocity.y = MinJumpVelocity;
        }
    }

    protected override void ApplyGravity() {
        throw new NotImplementedException();
    }
}
