using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour {

    private GameObject _player;

    private void Update() {
        foreach (var player in GameObject.FindGameObjectsWithTag(GameConstants.Tag_Player)) {
            player.GetComponent<PlayerInputController>().enabled = false;
        }
    }

    public void Quit() {
        Application.Quit();
    }

    public void Menu() {
        try {
            AudioManager.Instance.stopSound(GameConstants.Audio_MainMusic);
            AudioManager.Instance.playSound(GameConstants.Audio_MenuMusic);
        } catch (System.Exception e) {
            print("Don't care");
        }
        SceneManager.LoadScene(GameConstants.Scene_LevelName_Menu);
    }

}
