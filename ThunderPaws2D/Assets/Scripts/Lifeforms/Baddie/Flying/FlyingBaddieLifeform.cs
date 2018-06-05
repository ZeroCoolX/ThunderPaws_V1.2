using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class specifically for Flying Baddie types
/// </summary>
public class FlyingBaddieLifeform : BaddieLifeform {
    /// <summary>
    /// Struct holding flying data like where and how to fly
    /// </summary>
    protected FlyingPositionModel FlyingPositionData;

    // I don't like this but for right now its necessary
    public bool IsHorde2Hack = false;

    /// <summary>
    /// Determines the max and min Y values the baddie can go
    /// and stores them in the FlyingPositionData struct
    /// </summary>
    protected void CalculateBounds(float minAdd, float maxAdd) {
        if (IsHorde2Hack) {
            FlyingPositionData.MinY = -85.89f;
            FlyingPositionData.MaxY = -77.9f;
        } else {
            FlyingPositionData.MinY = Target.position.y + minAdd;//2f;
            FlyingPositionData.MaxY = Target.position.y + maxAdd;// 6f;
        }
    }

    /// <summary>
    /// Ensures that the baddie won't overshoot either the max or min 
    /// threshold positions.
    /// </summary>
    protected void MaxBoundsCheck() {
        if (transform.position.y >= FlyingPositionData.MaxY) {
            FlyingPositionData.TargetYDirection = -1;
        } else if (transform.position.y <= FlyingPositionData.MinY) {
            FlyingPositionData.TargetYDirection = 1;
        } else if (Mathf.Sign(transform.position.y - Target.position.y) < 0) {
            FlyingPositionData.TargetYDirection = 1;
        }
    }

    /// <summary>
    /// Centralized method for returning a random float between min and max Y
    /// </summary>
    /// <returns></returns>
    protected float ChooseRandomHeight() {
        return Random.Range(FlyingPositionData.MinY, FlyingPositionData.MaxY);
    }

    /// <summary>
    /// This is the same as MaxBoundsCheck
    /// </summary>
    // TODO : This somehow works for now but is screwy.
    // The line : targetY = Mathf.Sign(ChooseRandomHeight());
    // doesn't make any sense if TargetYDirection should be 1 or -1....
    protected void CalculateVerticalThreshold() {
        if (transform.position.y >= FlyingPositionData.MaxY) {
            //print("Send it to the min");
            FlyingPositionData.TargetYDirection = -1;
        } else if (transform.position.y <= FlyingPositionData.MinY) {
            //print("Send it to the max");
            FlyingPositionData.TargetYDirection = 1;
        } else {
            if (Mathf.Sign(transform.position.y - Target.position.y) < 0) {
                FlyingPositionData.TargetYDirection = 1;
            } else {
                FlyingPositionData.TargetYDirection = Mathf.Sign(ChooseRandomHeight());
            }
        }
    }

    /// <summary>
    /// Holds all necessary data for how to move around in the world
    /// </summary>
    protected struct FlyingPositionModel {
        // min[0] and max[1] Y values in the world where
        // this baddie can fly
        public float MinY, MaxY;
        // How fast baddie moveis
        public float MoveSpeed;
        // Needed for Mathf.SmoothDamp function when flying
        public float VelocityXSmoothing, VelocityYSmoothing;
        // The desired Y direction for movement
        public float TargetYDirection;
    }
}
