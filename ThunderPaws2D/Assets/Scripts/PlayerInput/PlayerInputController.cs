using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerInputController : MonoBehaviour {
    /// <summary>
    /// Player reference
    /// </summary>
    Player Player;


    void Start() {
        Player = GetComponent<Player>();
    }
    /// <summary>
    /// Get the player input and store it on the Player object
    /// </summary>
    void Update() {
        
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Player.SetDirectionalInput(directionalInput);
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Joystick1Button0)) {
            Player.OnJumpInputUp();
        }
    }
}
