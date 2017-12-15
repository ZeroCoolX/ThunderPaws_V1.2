using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
