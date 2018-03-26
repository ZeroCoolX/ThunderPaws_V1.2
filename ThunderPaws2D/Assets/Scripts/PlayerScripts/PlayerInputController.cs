using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerInputController : MonoBehaviour {
    /// <summary>
    /// Player reference
    /// </summary>
    Player Player;

    // Just a useful link to the xbox 360 controller map
    // http://wiki.unity3d.com/index.php?title=Xbox360Controller
     
    void Start() {
        Player = GetComponent<Player>();
    }
    /// <summary>
    /// Get the player input and store it on the Player object
    /// </summary>
    void Update() {
        
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw(GameConstants.Input_Horizontal), Input.GetAxisRaw(GameConstants.Input_Vertical));
        Player.DirectionalInput = directionalInput;
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp(GameConstants.Input_Jump)) {
            Player.OnJumpInputUp();
        }
    }
}
