using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISoundController : MonoBehaviour {

    public string SoundName;
    private Button _button { get { return GetComponent<Button>(); } }
    private bool _joystickReset = true;

	// Use this for initialization
	void Start () {
        _button.onClick.AddListener(() => PlaySound());
	}

    private void Update() {
        if(JoystickManagerController.Instance.ConnectedControllers() == 0) {
            if (Input.GetButtonDown("Universal_Vertical") || Input.GetButtonDown("Universal_Horizontal")) {
                AudioManager.Instance.PlaySound("MenuButton_Navigate");
            }
        }else {
            if (!_joystickReset) {
                _joystickReset = Input.GetAxisRaw("Universal_Horizontal") == 0 && Input.GetAxisRaw("Universal_Vertical") == 0;
            }
            if (Input.GetAxisRaw("Universal_Vertical") > 0.8 || Input.GetAxisRaw("Universal_Vertical") < -0.8
                || Input.GetAxisRaw("Universal_Horizontal") > 0.8 || Input.GetAxisRaw("Universal_Horizontal") < -0.8) {
                if (_joystickReset) {
                    _joystickReset = false;
                    print("playing sound");
                    AudioManager.Instance.PlaySound("MenuButton_Navigate");
                }
            }
        }
    }

    void PlaySound() {
        AudioManager.Instance.PlaySound(SoundName);
    }
}
