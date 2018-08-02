using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputSelectScreenManager : MonoBehaviour {
    private Button _keyboardButton;

    private void Start() {
        _keyboardButton = transform.Find("KeyboardControls").GetComponent<Button>();
        if(_keyboardButton == null) {
            throw new MissingComponentException("No Keyboard control input button found on ControlsMenu");
        }
    }

    void Update () {
		if(_keyboardButton != null) {
            if(JoystickManagerController.Instance.ConnectedControllers() != 0) {
                if (_keyboardButton.interactable) {
                    _keyboardButton.interactable = false;
                }
            }else {
                if (!_keyboardButton.interactable) {
                    _keyboardButton.interactable = true;
                }
            }
        }
	}
}
