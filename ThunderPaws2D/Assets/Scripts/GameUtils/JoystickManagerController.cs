using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class JoystickManagerController : MonoBehaviour{

    public static bool ControllersConnected = false;

    // Mapping from <Player, Controller input identifier prefix>
    public static Dictionary<int, string> ControllerMap = new Dictionary<int, string>();

    public static void CollectControllers() {
        var connectedControllers = Input.GetJoystickNames();
        for (var i = 0; i < connectedControllers.Length; ++i) {
            if (string.IsNullOrEmpty(connectedControllers[i])) {
                continue;
            }
            string outVal = null;
            var player = 1;
            if (ControllerMap.TryGetValue(1, out outVal)) {
                print("player 2 exists");
                player = 2;
            }
            print("Mapping joystick " + "J" + (i + 1) + "- to Player " + player);
            ControllerMap.Add(player, "J" + (i + 1) + "-");
        }
    }

    public static int ConnectedControllers() {
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
            // There is at least one controller connected so use that for input
            ControllersConnected = true;
            print("Assigning joystick " + i + " named: "+ connectedControllers[i]);
            // Just assign player1 to the first controller we find
            player1.JoystickNumberPrefix = "J"+(i + 1)+"-";
        }
    }
}
