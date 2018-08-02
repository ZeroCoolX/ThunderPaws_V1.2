using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHudManager : MonoBehaviour {
    public static PlayerHudManager Instance; 

    private PlayerStatsUIController[] _playerHuds = new PlayerStatsUIController[2];



    public PlayerStatsUIController GetPlayerHud(int player) {
        return _playerHuds[player - 1];
    }

    public void ActivateStatsHud(int player) {
        if (!_playerHuds[player-1].gameObject.activeSelf) {
            _playerHuds[player-1].gameObject.SetActive(true);
        }
    }

    public void UpdateHealthUI(int player, int current, int max) {
        var playerHud = _playerHuds[player-1];
        playerHud.SetHealthStatus(current, max);
    }

    public void UpdateUltimateUI(int player, int current, int max) {
        var playerHud = _playerHuds[player - 1];
        playerHud.SetUltimateStatus(current, max);
    }

    public void UpdateWeaponPickup(int player, string weaponName) {
        print("Updating weapon pickup UI to display pickedup weapon");
        var playerHud = _playerHuds[player - 1];
        playerHud.SetWeaponPickup(weaponName);
    }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }

        PopulateHuds();
    }

    private void PopulateHuds() {
        try {
            _playerHuds[0] = transform.Find("Player1Stats").GetComponent<PlayerStatsUIController>();
            _playerHuds[1] = transform.Find("Player2Stats").GetComponent<PlayerStatsUIController>();
        } catch (System.Exception e) {
            print("Excetion generated attempting to find the player stats: " + e.Message);
        }
    }
}
