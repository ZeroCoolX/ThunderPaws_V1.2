﻿using UnityEngine;
using System.Collections;
public class InputManager : MonoBehaviour {
    public static InputManager Instance;

    // WASD is for movement and SPACE is jump. Deal with it
    public KeyCode Melee { get; set; }
    public KeyCode Fire { get; set; }
    public KeyCode Roll { get; set; }
    public KeyCode LockMovement { get; set; }
    public KeyCode ChangeWeapon { get; set; }
    public KeyCode Ultimate { get; set; }

    void Awake() {
        if (Instance == null) {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
        // Assign keycodes to default
        Melee = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Melee", "RightShift"));
        Fire = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Fire", "Return"));
        Roll = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Roll", "LeftShift"));
        LockMovement = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("LockMovement", "RightControl"));
        ChangeWeapon = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ChangeWeapon", "UpArrow"));
        Ultimate = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Ultimate", "Q"));
    }



}
