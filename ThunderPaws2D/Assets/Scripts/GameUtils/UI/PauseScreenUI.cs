using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseScreenUI : MonoBehaviour {

    private bool _gameIsPaused = false;
    public Transform PauseMenuUI;
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Universal_Escape")) {
            if (!_gameIsPaused) {
                Pause();
            }/*else {
                Resume();
            }*/
        }
	}

    private void Pause() {
        foreach(var player in GameObject.FindGameObjectsWithTag(GameConstants.Tag_Player)) {
            player.GetComponent<PlayerInputController>().enabled = false;
        }
        PauseMenuUI.gameObject.SetActive(true);
        Time.timeScale = 0f;
        _gameIsPaused = true;
    }

    public void Resume() {
        foreach (var player in GameObject.FindGameObjectsWithTag(GameConstants.Tag_Player)) {
            player.GetComponent<PlayerInputController>().enabled = true;
        }
        PauseMenuUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
        _gameIsPaused = false;
    }

    public void Menu() {
        AudioManager.Instance.stopSound(GameConstants.Audio_MainMusic);
        AudioManager.Instance.playSound(GameConstants.Audio_MenuMusic);
        SceneManager.LoadScene("AlphaDemoMainMenu");
        Time.timeScale = 1f;
    }
}
