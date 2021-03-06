﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class JoystickManagerController : MonoBehaviour{
    public static JoystickManagerController Instance;

    private void Awake() {
        if (Instance != null) {
            if (Instance != this) {
                Destroy(this.gameObject);
            }
        } else {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    public static bool ControllersConnected = false;

    // Mapping from <Player, Controller input identifier prefix>
    public Dictionary<int, string> ControllerMap = new Dictionary<int, string>();

    public void CollectControllers(bool singlePlayer) {
        var connectedControllers = Input.GetJoystickNames();
        for (var i = 0; i < connectedControllers.Length; ++i) {
            if (string.IsNullOrEmpty(connectedControllers[i])) {
                continue;
            }
            string outVal = null;
            var player = 1;
            if (ControllerMap.TryGetValue(1, out outVal)) {
                // This is a failsafe in case they user has multiple controllers conntect but is playing single player
                if (singlePlayer) {
                    return;
                }
                player = 2;
            }
            print("Mapping joystick " + "J" + (i + 1) + "- to Player " + player);
            ControllerMap.Add(player, "J" + (i + 1) + "-");
        }
    }

    public int ConnectedControllers() {
        var controllerNum = 0;
        var connectedControllers = Input.GetJoystickNames();
        for (var i = 0; i < connectedControllers.Length; ++i) {
            if (string.IsNullOrEmpty(connectedControllers[i])) {
                continue;
            }
            ++controllerNum;
        }
        return controllerNum;
    }
}
