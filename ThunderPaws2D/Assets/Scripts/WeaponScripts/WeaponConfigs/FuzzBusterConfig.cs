using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuzzBusterConfig {
    /// <summary>
    /// How many seconds to wait between automatic firing
    /// If the player is holding down the button start firing automatically.
    /// Otherwise most weapons will be fired as fast as the player can pull the trigger.
    /// </summary>
	public static float AutoFireSpacing = 0.5f;
}
