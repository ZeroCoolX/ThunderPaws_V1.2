using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerInputController : MonoBehaviour {
    private Player _player;
     
    void Start() {
        _player = GetComponent<Player>();
    }

    private void Update() {
        _player.DirectionalInput = GetDirectionBasedOffInputType();
        
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp(_player.JoystickId + GameConstants.Input_Jump)) {
            _player.OnJumpInputUp();
        }
    }

    private Vector2 GetDirectionBasedOffInputType() {
        return (JoystickManagerController.Instance.ConnectedControllers() > 0)
            ? new Vector2(Input.GetAxisRaw(_player.JoystickId + GameConstants.Input_Horizontal), Input.GetAxisRaw(_player.JoystickId + GameConstants.Input_Vertical))
            : new Vector2(Input.GetAxisRaw(GameConstants.Input_Horizontal), Input.GetAxisRaw(GameConstants.Input_Vertical));
    }
}
