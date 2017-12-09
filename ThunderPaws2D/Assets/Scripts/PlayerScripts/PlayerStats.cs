using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour {
    public static PlayerStats Instance;

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
        set { _currentUltimate = Mathf.Clamp(value, 0, MaxUltimate); }
    }

    /// <summary>
    /// Create singleton
    /// </summary>
    private void Awake() {
        if(Instance == null) {
            Instance = this;
        }
    }
}
