using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputBindingUI : MonoBehaviour {
    private Transform _menuPanel;

    private Event _keyEvent;

    public Text ButtonText;

    private KeyCode _newKey;

    private bool _shiftTrigger = false;


  private bool _waitingForKey;

    void Start() {
        _menuPanel = transform.Find("InputBindingPanel");
        _waitingForKey = false;

        for (var i = 0; i < _menuPanel.childCount; ++i) {
            print("_menuPanel thing = " + _menuPanel.GetChild(i).name);
            switch (_menuPanel.GetChild(i).name) {
                case "Melee":
                    _menuPanel.GetChild(i).GetChild(0).GetComponent<Text>().text = InputManager.Instance.Melee.ToString();
                    break;
                case "Fire":
                    _menuPanel.GetChild(i).GetChild(0).GetComponent<Text>().text = InputManager.Instance.Fire.ToString();
                    break;
                case "Roll":
                    _menuPanel.GetChild(i).GetChild(0).GetComponent<Text>().text = InputManager.Instance.Roll.ToString();
                    break;
                case "LockMovement":
                    _menuPanel.GetChild(i).GetChild(0).GetComponent<Text>().text = InputManager.Instance.LockMovement.ToString();
                    break;
                case "ChangeWeapon":
                    _menuPanel.GetChild(i).GetChild(0).GetComponent<Text>().text = InputManager.Instance.ChangeWeapon.ToString();
                    break;
                case "Ultimate":
                    _menuPanel.GetChild(i).GetChild(0).GetComponent<Text>().text = InputManager.Instance.Ultimate.ToString();
                    break;
            }
        }
    }

    void OnGUI() {
        // _keyEvent dictates what key our user is pressing by using Event.current to detect the current event
        _keyEvent = Event.current;
        if (_keyEvent.shift) {// we check with Event if shift is down
            if (Input.GetKey(KeyCode.LeftShift)) {   // we check with input witch shift is down
                // Execute if a button gets pressed
                if (_waitingForKey) {
                    _newKey = KeyCode.LeftShift; // Assigns _newKey to the key the user presses
                    _waitingForKey = false;
                    _shiftTrigger = true;
                }
            } else if (Input.GetKey(KeyCode.RightShift)) {// we check with input witch shift is down
                // Execute if a button gets pressed
                if (_waitingForKey) {
                    _newKey = KeyCode.RightShift; // Assigns _newKey to the key the user presses
                    _waitingForKey = false;
                    _shiftTrigger = true;
                }
            }
        }else {
            // Execute if a button gets pressed
            if (_keyEvent.isKey && _waitingForKey) {
                _newKey = _keyEvent.keyCode; // Assigns _newKey to the key the user presses
                _waitingForKey = false;
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
        while (!_keyEvent.isKey && !_shiftTrigger) {
            yield return null;
        }
    }


    public IEnumerator AssignKey(string keyName) {
        _waitingForKey = true;
        yield return WaitForKeys(); // Executes endlessly until the user presses a key
        _shiftTrigger = false;

        switch (keyName) {
            case "Melee":
                InputManager.Instance.Melee = _newKey;
                ButtonText.text = InputManager.Instance.Melee.ToString();
                PlayerPrefs.SetString("Melee", InputManager.Instance.Melee.ToString()); // Save the new key to player prefs
                break;
            case "Fire":
                InputManager.Instance.Fire = _newKey;
                ButtonText.text = InputManager.Instance.Fire.ToString();
                PlayerPrefs.SetString("Fire", InputManager.Instance.Fire.ToString()); // Save the new key to player prefs
                break;
            case "Roll":
                InputManager.Instance.Roll = _newKey;
                ButtonText.text = InputManager.Instance.Roll.ToString();
                PlayerPrefs.SetString("Roll", InputManager.Instance.Roll.ToString()); // Save the new key to player prefs
                break;
            case "LockMovement":
                InputManager.Instance.LockMovement = _newKey;
                ButtonText.text = InputManager.Instance.LockMovement.ToString();
                PlayerPrefs.SetString("LockMovement", InputManager.Instance.LockMovement.ToString()); // Save the new key to player prefs
                break;
            case "ChangeWeapon":
                InputManager.Instance.ChangeWeapon = _newKey;
                ButtonText.text = InputManager.Instance.ChangeWeapon.ToString();
                PlayerPrefs.SetString("ChangeWeapon", InputManager.Instance.ChangeWeapon.ToString()); // Save the new key to player prefs
                break;
            case "Ultimate":
                InputManager.Instance.Ultimate = _newKey;
                ButtonText.text = InputManager.Instance.Ultimate.ToString();
                PlayerPrefs.SetString("Ultimate", InputManager.Instance.Ultimate.ToString()); // Save the new key to player prefs
                break;
        }
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;

    }




}
