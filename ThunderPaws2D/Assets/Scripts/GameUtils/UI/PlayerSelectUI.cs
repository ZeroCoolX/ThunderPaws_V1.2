﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSelectUI : MonoBehaviour {

	public void Select1Player() {
        // Just start the game
        if(JoystickManagerController.Instance.ConnectedControllers() == 0) {
            // If the user is using KB - just fake controller 1
            string controller;
            if (!JoystickManagerController.Instance.ControllerMap.TryGetValue(1, out controller)) {
                JoystickManagerController.Instance.ControllerMap.Add(1, "J1-");
            }else {
                JoystickManagerController.Instance.ControllerMap[1] = "J1-";
            }
        } else {
            // If the user has > 0 controllers connected only get the first one. 
            // Its on them to figure out which controller I secretly assigned as player 1 xD
            JoystickManagerController.Instance.CollectControllers(true);
        }
        //SceneManager.LoadScene("PreAlphaDemoMainMenu");
    }

    public void Select2Player() {
        // Hide this UI and show the controller assignment screen
        SceneManager.LoadScene("PreAlphaDemoControllerAssignment");
    }
}
