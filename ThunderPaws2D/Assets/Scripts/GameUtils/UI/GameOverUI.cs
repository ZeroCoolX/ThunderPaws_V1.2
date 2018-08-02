using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour {

    public Transform Player1CoinText;
    public Transform Player1BaddieText;
    public Transform Player1UltsText;
    public Transform Player1DiedText;
    public Transform Player1TimeText;

    public Transform Player2CoinText;
    public Transform Player2BaddieText;
    public Transform Player2UltsText;
    public Transform Player2DiedText;
    public Transform Player2TimeText;

    private GameObject _player;

    private void OnEnable() {
        Player1CoinText.GetComponent<TextMeshProUGUI>().text = ""+GameStatsManager.Instance.CoinsCollected(1);
        Player1BaddieText.GetComponent<TextMeshProUGUI>().text = ""+ GameStatsManager.Instance.BaddiesKilled(1);
        Player1UltsText.GetComponent<TextMeshProUGUI>().text = "" + GameStatsManager.Instance.UltimatesUsed(1);
        Player1DiedText.GetComponent<TextMeshProUGUI>().text = "" + GameStatsManager.Instance.TimesDied(1);
        Player1TimeText.GetComponent<TextMeshProUGUI>().text = "" + GameStatsManager.Instance.LevelTime(1);

        Player2CoinText.GetComponent<TextMeshProUGUI>().text = "" + GameStatsManager.Instance.CoinsCollected(2);
        Player2BaddieText.GetComponent<TextMeshProUGUI>().text = "" + GameStatsManager.Instance.BaddiesKilled(2);
        Player2UltsText.GetComponent<TextMeshProUGUI>().text = "" + GameStatsManager.Instance.UltimatesUsed(2);
        Player2DiedText.GetComponent<TextMeshProUGUI>().text = "" + GameStatsManager.Instance.TimesDied(2);
        Player2TimeText.GetComponent<TextMeshProUGUI>().text = "" + GameStatsManager.Instance.LevelTime(2);
    }

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
            AudioManager.Instance.StopSound(GameConstants.Audio_MainMusic);
            AudioManager.Instance.PlaySound(GameConstants.Audio_MenuMusic);
        } catch (System.Exception e) {
            print("AudoManager failed attempting to switch music. Just continue on");
        }
        SceneManager.LoadScene(GameConstants.Scene_LevelName_Menu);
    }

}
