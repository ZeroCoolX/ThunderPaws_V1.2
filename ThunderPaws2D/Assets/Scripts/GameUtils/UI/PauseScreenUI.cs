﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseScreenUI : MonoBehaviour {

    private bool _gameIsPaused = false;
    public Transform PauseMenuUI;
	
	void Update () {
        if (Input.GetButtonDown("Universal_Escape")) {
            if (!_gameIsPaused) {
                TogglePause(true);
            }else {
                TogglePause(false);
            }
        }
	}

    private void TogglePause(bool pauseStatus) {
        foreach (var player in GameObject.FindGameObjectsWithTag(GameConstants.Tag_Player)) {
            player.GetComponent<PlayerInputController>().enabled = !pauseStatus;
        }
        _gameIsPaused = pauseStatus;
        PauseMenuUI.gameObject.SetActive(_gameIsPaused);
        Time.timeScale = _gameIsPaused ? 0f : 1f;
    }

    public void Menu() {
        AudioManager.Instance.StopSound(GameConstants.GetLevel(DifficultyManager.Instance.LevelToPlay));
        AudioManager.Instance.PlaySound(GameConstants.Audio_MenuMusic);
        SceneManager.LoadScene(GameConstants.Scene_LevelName_Menu);
        Time.timeScale = 1f;
    }
}
