using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the secret weapon that allows us to set the difficulty in one scene - load a scene - and retrieve the difficulty
/// Static classes persist and do not get recreated on load
/// </summary>
public static class LivesManager {

    public static int Lives;
    public static int Health;
}
