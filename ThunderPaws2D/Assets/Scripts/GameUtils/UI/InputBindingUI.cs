using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputBindingUI : MonoBehaviour {
    public Text ButtonText;

    private Event _userPressedKey;
    private KeyCode _newKey;
    private bool _shiftTrigger = false;
    private bool _waitingForKey;

    void Start() {
        _waitingForKey = false;

        for (var i = 0; i < transform.childCount; ++i) {
            switch (transform.GetChild(i).name) {
                case "Melee":
                    transform.GetChild(i).GetChild(0).GetComponent<Text>().text = InputManager.Instance.Melee.ToString();
                    break;
                case "Fire":
                    transform.GetChild(i).GetChild(0).GetComponent<Text>().text = InputManager.Instance.Fire.ToString();
                    break;
                case "Roll":
                    transform.GetChild(i).GetChild(0).GetComponent<Text>().text = InputManager.Instance.Roll.ToString();
                    break;
                case "LockMovement":
                    transform.GetChild(i).GetChild(0).GetComponent<Text>().text = InputManager.Instance.LockMovement.ToString();
                    break;
                case "ChangeWeapon":
                    transform.GetChild(i).GetChild(0).GetComponent<Text>().text = InputManager.Instance.ChangeWeapon.ToString();
                    break;
                case "Ultimate":
                    transform.GetChild(i).GetChild(0).GetComponent<Text>().text = InputManager.Instance.Ultimate.ToString();
                    break;
            }
        }
    }

    void OnGUI() {
        _userPressedKey = Event.current;
        if (_userPressedKey.shift) {
            HandleShiftSpecialCase();
        } else {
            if (_userPressedKey.isKey && _waitingForKey) {
                _newKey = _userPressedKey.keyCode;
                _waitingForKey = false;
            }
        }
    }

    /// <summary>
    /// The Shift keys are special because they don't actually have a value for keycode on the keyEvent.
    /// </summary>
    private void HandleShiftSpecialCase() {
        if (Input.GetKey(KeyCode.LeftShift)) {
            if (_waitingForKey) {
                _newKey = KeyCode.LeftShift;
                _waitingForKey = false;
                _shiftTrigger = true;
            }
        } else if (Input.GetKey(KeyCode.RightShift)) {
            if (_waitingForKey) {
                _newKey = KeyCode.RightShift;
                _waitingForKey = false;
                _shiftTrigger = true;
            }
        }
    }

    public void StartAssignment(string keyName) {
        if (!_waitingForKey) {
            StartCoroutine(AssignKey(keyName));
        }
    }

    public void SendText(Text text) {
        ButtonText = text;
    }

    public IEnumerator WaitForKeys() {
        while (!_userPressedKey.isKey && !_shiftTrigger) {
            yield return null;
        }
    }

    public IEnumerator AssignKey(string keyName) {
        _waitingForKey = true;
        yield return WaitForKeys();
        _shiftTrigger = false;
        switch (keyName) {
            case "Melee":
                InputManager.Instance.Melee = _newKey;
                ButtonText.text = InputManager.Instance.Melee.ToString();
                PlayerPrefs.SetString("Melee", InputManager.Instance.Melee.ToString());
                break;
            case "Fire":
                InputManager.Instance.Fire = _newKey;
                ButtonText.text = InputManager.Instance.Fire.ToString();
                PlayerPrefs.SetString("Fire", InputManager.Instance.Fire.ToString());
                break;
            case "Roll":
                InputManager.Instance.Roll = _newKey;
                ButtonText.text = InputManager.Instance.Roll.ToString();
                PlayerPrefs.SetString("Roll", InputManager.Instance.Roll.ToString());
                break;
            case "LockMovement":
                InputManager.Instance.LockMovement = _newKey;
                ButtonText.text = InputManager.Instance.LockMovement.ToString();
                PlayerPrefs.SetString("LockMovement", InputManager.Instance.LockMovement.ToString());
                break;
            case "ChangeWeapon":
                InputManager.Instance.ChangeWeapon = _newKey;
                ButtonText.text = InputManager.Instance.ChangeWeapon.ToString();
                PlayerPrefs.SetString("ChangeWeapon", InputManager.Instance.ChangeWeapon.ToString());
                break;
            case "Ultimate":
                InputManager.Instance.Ultimate = _newKey;
                ButtonText.text = InputManager.Instance.Ultimate.ToString();
                PlayerPrefs.SetString("Ultimate", InputManager.Instance.Ultimate.ToString());
                break;
        }
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
    }
}
