using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControllerAssignmentUI : MonoBehaviour {

    private Button _player1Button;
    private bool player1Assigned = false;
    private Button _player2Button;
    private bool player2Assigned = false;

    private Transform ControllerConnectionWarning;

    private void Awake() {
        var buttons = transform.Find("Canvas").Find("Panel");
        _player1Button = buttons.Find("1PlayerButton").GetComponent<Button>();
        _player2Button = buttons.Find("2PlayerButton").GetComponent<Button>();
        ControllerConnectionWarning = buttons.Find("ControllerConnectionWarning");
    }

    private void Update() {
        if(player1Assigned && player2Assigned) {
            SceneManager.LoadScene("PreAlphaDemoMainMenu");
        }

        if (JoystickManagerController.Instance.ConnectedControllers() < 2) {
            if (!ControllerConnectionWarning.gameObject.activeSelf) {
                ControllerConnectionWarning.gameObject.SetActive(true);
                return;
            }
        }else {
            ControllerConnectionWarning.gameObject.SetActive(false);
            if (JoystickManagerController.Instance.ControllerMap.Count < 2) {
                JoystickManagerController.Instance.CollectControllers(false);
            }
        }

            string prefix = "";
            // Check if player 1 has pressed the button yet
            if (JoystickManagerController.Instance.ControllerMap.TryGetValue(1, out prefix)) {
                print("Player 1 looking for joystick : " + prefix);
                if (Input.GetButtonUp(prefix + GameConstants.Input_Jump)) {
                    print("player 1 pressed a!!!!");
                    AssignController(1);
                }
            }

            // Check if player 2 has pressed the button yet
            if (JoystickManagerController.Instance.ControllerMap.TryGetValue(2, out prefix)) {
                print("Player 2 looking for joystick : " + prefix);
                if (Input.GetButtonUp(prefix + GameConstants.Input_Jump)) {
                    print("player 2 pressed a!!!!");
                    AssignController(2);
                }
            }
    }

    public void AssignController(int player) {
        if(player == 1) {
            _player1Button.GetComponent<Image>().color = Color.green;
            player1Assigned = true;
        } else {
            _player2Button.GetComponent<Image>().color = Color.green;
            player2Assigned = true;
        }
    }

}
