using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour {

    private GameObject _player;

    private void Update() {
        if(_player == null) {
            _player = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);
            if (_player != null) {
                _player.transform.GetComponent<PlayerInputController>().enabled = false;
            }
        }
        if (Input.GetButtonUp(GameConstants.Input_Back)) {
            Menu();
        }
        if (Input.GetButtonUp(GameConstants.Input_Start)) {
            Quit();
        }
    }

    public void Quit() {
        Application.Quit();
    }

    public void Menu() {
        try {
            GameMaster.Instance.AudioManager.stopSound("Music_Main");
        }catch(System.Exception e) {
            print("Don't care");
        }
        SceneManager.LoadScene(GameConstants.Scene_LevelName_Menu);
    }

}
