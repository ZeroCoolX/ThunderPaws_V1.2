using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHudManager : MonoBehaviour {
    public static PlayerHudManager Instance; 
    /// <summary>
    /// List containing all the HUDs of the players. Min 1, max 2
    /// </summary>
    private PlayerStatsUIController[] _playerHuds = new PlayerStatsUIController[2];

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }

        // Populate our HUD list
        try {
            _playerHuds[0] = transform.Find("Player1Stats").GetComponent<PlayerStatsUIController>();
            _playerHuds[1] = transform.Find("Player2Stats").GetComponent<PlayerStatsUIController>();
        } catch (System.Exception e) {
            print("Excetion generated attempting to find the player stats: " + e.Message);
        }
    }

    public PlayerStatsUIController GetPlayerHud(int player) {
        return _playerHuds[player - 1];
    }

    /// <summary>
    /// Activates a players HUD without exposing the array to the public
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public void ActivateStatsHud(int player) {
        if (!_playerHuds[player-1].gameObject.activeSelf) {
            _playerHuds[player-1].gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Updates the Health HUD bar for the player
    /// </summary>
    /// <param name="player"></param>
    /// <param name="current"></param>
    /// <param name="max"></param>
    public void UpdateHealthUI(int player, int current, int max) {
        print("Updating health : "+current+ "/" + max);
        var playerHud = _playerHuds[player-1];
        playerHud.SetHealthStatus(current, max);
    }

    /// <summary>
    /// Updates the Ultimate HUD bar for the player
    /// </summary>
    /// <param name="player"></param>
    /// <param name="current"></param>
    /// <param name="max"></param>
    public void UpdateUltimateUI(int player, int current, int max) {
        print("Updating ultimate : " + current + "/" + max);
        var playerHud = _playerHuds[player - 1];
        playerHud.SetUltimateStatus(current, max);
    }
}
