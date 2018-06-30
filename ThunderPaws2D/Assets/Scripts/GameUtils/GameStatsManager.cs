using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Holds all the stats per level.
/// This object will only persist inside this level. 
/// Going back to the outside menu willdetroy this object similar to how the GameMaster is handled
/// </summary>
public class GameStatsManager : MonoBehaviour {

    public static GameStatsManager Instance;

    private Dictionary<int, GameStats>_playerGameStatsMap = new Dictionary<int, GameStats>();

    void Awake() {
        if (Instance != null) {
            if (Instance != this) {
                Destroy(this.gameObject);
            }
        } else {
            Instance = this;
        }
    }

    public void AddPlayerToMap(int player) {
        print("added player " + player + " to stats map");
        _playerGameStatsMap.Add(player, new GameStats());
    }

    public float GetValueFromMap(int player, string value) {
        GameStats stats;
        _playerGameStatsMap.TryGetValue(player, out stats);
        switch (value.ToUpper()) {
            case "COIN":
                return stats.CoinsCollected;
            case "BADDIE":
                return stats.BaddiesKilled;
            case "ULT":
                return stats.UltimatesUsed;
            case "DIED":
                return stats.TimesDied;
            default:
                return -117;
        }
    }
    public void IncrementValueInMap(int player, string value) {
        GameStats stats;
        _playerGameStatsMap.TryGetValue(player, out stats);
        switch (value.ToUpper()) {
            case "COIN":
                ++stats.CoinsCollected;
                break;
            case "BADDIE":
                ++stats.BaddiesKilled;
                break;
            case "ULT":
                ++stats.UltimatesUsed;
                break;
            case "DIED":
                ++stats.TimesDied;
                break;
            default:
                print("could not find a mapping for player " + player + " and value : " + value);
                break;
        }
        _playerGameStatsMap[player] = stats;
    }

    public void AddCoin(int player) {
        print("Adding coin for player " + player);
        IncrementValueInMap(player, "COIN");
    }

    public void AddBaddie(int player) {
        IncrementValueInMap(player, "BADDIE");
    }

    public void AddUlt(int player) {
        IncrementValueInMap(player, "ULT");
    }

    public void AddDeath(int player) {
        IncrementValueInMap(player, "DIED");
    }

    public void StartTimer(int player) {
        GameStats stats;
        _playerGameStatsMap.TryGetValue(player, out stats);
        if(stats.LevelTimer == null) {
            stats.LevelTimer = new Stopwatch();
        }
        stats.LevelTimer.Start();
        _playerGameStatsMap[player] = stats;
    }

    public void StopTimer(int player) {
        GameStats stats;
        _playerGameStatsMap.TryGetValue(player, out stats);
        stats.LevelTimer.Stop();
        _playerGameStatsMap[player] = stats;
    }

    public int CoinsCollected(int player) {
        print("Coins for player " + player + " = " + (int)GetValueFromMap(player, "COIN"));
        return (int)GetValueFromMap(player, "COIN");
    }

    public int BaddiesKilled(int player) {
        return (int)GetValueFromMap(player, "BADDIE");
    }

    public int UltimatesUsed(int player) {
        return (int)GetValueFromMap(player, "ULT");
    }

    public int TimesDied(int player) {
        return (int)GetValueFromMap(player, "DIED");
    }

    public string LevelTime(int player) {
        GameStats stats;
        _playerGameStatsMap.TryGetValue(player, out stats);
        if(stats.LevelTimer != null) {
            return string.Format("{0:00}:{1:00}", stats.LevelTimer.Elapsed.Minutes, stats.LevelTimer.Elapsed.Seconds);
        }
        return "...";
    }

    private struct GameStats {
        public int CoinsCollected;
        public int BaddiesKilled;
        public int UltimatesUsed;
        public int TimesDied;
        public Stopwatch LevelTimer;
    }

}
