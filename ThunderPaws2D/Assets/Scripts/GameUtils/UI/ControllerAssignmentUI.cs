using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControllerAssignmentUI : MonoBehaviour {

    private Image _player1Icon;
    private bool player1Assigned = false;
    private Image _player2Icon;
    private bool player2Assigned = false;

    private Transform ControllerConnectionWarning;

    private Transform DifficultyUI;



    public void AssignController(int player) {
        if (player == 1) {
            _player1Icon.color = Color.white;
            player1Assigned = true;
        } else {
            _player2Icon.color = Color.white;
            player2Assigned = true;
        }
    }

    private void Awake() {
        _player1Icon = transform.Find("Player1Icon").GetComponent<Image>();
        _player2Icon = transform.Find("Player2Icon").GetComponent<Image>();
        ControllerConnectionWarning = transform.Find("ControllerWarning");
        DifficultyUI = transform.parent.Find("DifficultySelectMenu");
    }

    private void Update() {
        if(player1Assigned && player2Assigned) {
            StartCoroutine(MoveToDifficulty());
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

        CheckForButtonPress(1);
        CheckForButtonPress(2);
    }

    private void CheckForButtonPress(int player) {
        string prefix = "";
        if (JoystickManagerController.Instance.ControllerMap.TryGetValue(player, out prefix)) {
            if (Input.GetButtonUp(prefix + GameConstants.Input_Jump)) {
                AssignController(player);
            }
        }
    }

    private IEnumerator MoveToDifficulty() {
        yield return new WaitForSeconds(1);
        gameObject.SetActive(false);
        DifficultyUI.gameObject.SetActive(true);
    }
}
