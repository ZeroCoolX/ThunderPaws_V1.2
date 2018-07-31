using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the actual stats for a player (Represented on screen by the PlayerStatsUiController).
/// Each player has an instance of this class for the duration of their existence.
/// Since they live on the player if the player dies, so do these stats.
/// </summary>
public class PlayerStats : MonoBehaviour {
    public bool UltEnabled = false;
    public bool UltReady = false;
    public int MaxHealth = 100;
    public int CurrentHealth {
        get { return _currentHealth; }
        set { _currentHealth = Mathf.Clamp(value, 0, MaxHealth); }
    }
    public int CurrentUltimate {
        get { return _currentUltimate; }
        set {
            _currentUltimate = Mathf.Clamp(value, 0, MaxUltimate);
            if (!UltReady && !UltEnabled) {
                UltReady = _currentUltimate >= MaxUltimate;
            }
        }
    }
    public int MaxUltimate = 100;

    private int _currentHealth;
    private int _currentUltimate;
}
