using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the actual stats for a player (Represented on screen by the PlayerStatsUiController).
/// Each player has an instance of this class for the duration of their existence.
/// Since they live on the player if the player dies, so do these stats.
/// </summary>
public class PlayerStats : MonoBehaviour {
    /// <summary>
    /// Indicates if we are int the ultimate or not
    /// </summary>
    public bool UltEnabled = false;
    /// <summary>
    /// Indicates if we are ready for ultimate status or not
    /// </summary>
    public bool UltReady = false;

    public int MaxHealth = 100;
    private int _currentHealth;
    public int CurrentHealth {
        get { return _currentHealth; }
        set { _currentHealth = Mathf.Clamp(value, 0, MaxHealth); }
    }

    public int MaxUltimate = 100;
    private int _currentUltimate;
    public int CurrentUltimate {
        get { return _currentUltimate; }
        set {
            _currentUltimate = Mathf.Clamp(value, 0, MaxUltimate);
            if(!UltReady && !UltEnabled) {
                UltReady = _currentUltimate >= MaxUltimate;
            }
        }
    }

    /// <summary>
    /// Create singleton
    /// </summary>
    private void Start() {

    }
}
