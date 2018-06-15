using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionIndexConfig {
    /// <summary>
    /// Maximum distance we allow the laser to be shot out
    /// </summary>
    public static float MaxLaserLength = 10f;
    /// <summary>
    /// Maximum distance we allow the laser to be shot out when in ult mode
    /// </summary>
    public static float MaxUltLaserLength = 15f;
    /// <summary>
    /// Time in seconds we should apply damage.
    /// 0.1 = damage every 1/10 of a second
    /// </summary>
    public static float DamageInterval = 0.1f;
}
