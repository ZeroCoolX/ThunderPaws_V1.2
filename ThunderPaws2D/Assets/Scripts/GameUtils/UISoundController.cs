using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISoundController : MonoBehaviour {

    public string SoundName;

    private Button _button { get { return GetComponent<Button>(); } }
    private bool _joystickReset = true;

    private const string VERTICAL = "Universal_Vertical";
    private const string HORIZONTAL = "Universal_Horizontal";

    void Start () {
        _button.onClick.AddListener(() => PlaySound());
	}

    private void Update() {
        if(JoystickManagerController.Instance.ConnectedControllers() == 0) {
            if (KeyboardNavigationActive()) {
                AudioManager.Instance.PlaySound("MenuButton_Navigate");
            }
        }else {
            if (!_joystickReset) {
                _joystickReset = Input.GetAxisRaw(HORIZONTAL) == 0 && Input.GetAxisRaw(VERTICAL) == 0;
            }
            if (JoystickNavicationActive()) {
                if (_joystickReset) {
                    _joystickReset = false;
                    AudioManager.Instance.PlaySound("MenuButton_Navigate");
                }
            }
        }
    }

    private bool KeyboardNavigationActive() {
        return Input.GetButtonDown(VERTICAL) || Input.GetButtonDown(HORIZONTAL);
    }

    private bool JoystickNavicationActive() {
        return Input.GetAxisRaw(VERTICAL) > 0.8 || Input.GetAxisRaw(VERTICAL) < -0.8
                || Input.GetAxisRaw(HORIZONTAL) > 0.8 || Input.GetAxisRaw(HORIZONTAL) < -0.8;
    }

    void PlaySound() {
        AudioManager.Instance.PlaySound(SoundName);
    }
}
