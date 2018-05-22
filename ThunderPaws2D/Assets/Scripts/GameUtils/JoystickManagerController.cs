using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class JoystickManagerController : MonoBehaviour{

    public static void AssignControllers() {
        // Grab the players (1-2)
        var player1 = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player).GetComponent<Player>();
        //var player2 = null;

        // Get a string[] of all the connected controllers 
        print("Beginning controller assignment");
        var connectedControllers = Input.GetJoystickNames();
        for(var i = 0; i < connectedControllers.Length; ++i) {
            if (string.IsNullOrEmpty(connectedControllers[i])) {
                continue;
            }
            print("Assigning joystick " + i + " named: "+ connectedControllers[i]);
            // Just assign player1 to the first controller we find
            player1.JoystickNumberPrefix = "J"+(i + 1)+"-";
        }

    }
}
