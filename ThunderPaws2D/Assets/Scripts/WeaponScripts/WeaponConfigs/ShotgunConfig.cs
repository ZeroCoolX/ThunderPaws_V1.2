using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunConfig {
    /// <summary>
    /// How long should the shotgun blast extend
    /// </summary>
    public static int RayLength = 5;
    /// <summary>
    /// Rotations per blast raycast so we create a "fan" of raycasts
    /// </summary>
    public static float[] BlastRotations = new float[] { 11.25f, 5.625f, 0f, 348.75f, 354.375f };
}
