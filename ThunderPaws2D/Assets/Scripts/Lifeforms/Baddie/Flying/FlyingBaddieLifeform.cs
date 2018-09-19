using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingBaddieLifeform : BaddieLifeform {
    // I don't like this but for right now its necessary
    public bool IsHorde2Hack = false;

    protected struct FlyingPositionModel {
        public float MinY, MaxY;
        public float MoveSpeed;
        public float VelocityXSmoothing, VelocityYSmoothing;
        public float TargetYDirection;
    }
    protected FlyingPositionModel FlyingPositionData;

    /// <summary>
    /// Determines the max and min Y values the baddie can go
    /// and stores them in the FlyingPositionData struct
    /// </summary>
    protected void CalculateBounds(float minAdd, float maxAdd) {
        if (IsHorde2Hack) {
            FlyingPositionData.MinY = -85.89f;
            FlyingPositionData.MaxY = -77.9f;
        } else {
            FlyingPositionData.MinY = Target.position.y + minAdd;
            FlyingPositionData.MaxY = Target.position.y + maxAdd;
        }
    }

    protected void MaxBoundsCheck() {
        if (transform.position.y >= FlyingPositionData.MaxY) {
            FlyingPositionData.TargetYDirection = -1;
        } else if (transform.position.y <= FlyingPositionData.MinY) {
            FlyingPositionData.TargetYDirection = 1;
        } else if (Mathf.Sign(transform.position.y - Target.position.y) < 0) {
            FlyingPositionData.TargetYDirection = 1;
        }
    }

    private void InitiateAttack() {
        Animator.SetBool("Attack", true);
    }

    private void ResetAttack() {
        Animator.SetBool("Attack", false);
    }

    protected float ChooseRandomHeight() {
        return Random.Range(FlyingPositionData.MinY, FlyingPositionData.MaxY);
    }

    protected void CalculateVerticalThreshold() {
        if (transform.position.y >= FlyingPositionData.MaxY) {
            FlyingPositionData.TargetYDirection = -1;
        } else if (transform.position.y <= FlyingPositionData.MinY) {
            FlyingPositionData.TargetYDirection = 1;
        } else {
            if (Mathf.Sign(transform.position.y - Target.position.y) < 0) {
                FlyingPositionData.TargetYDirection = 1;
            } else {
                FlyingPositionData.TargetYDirection = Mathf.Sign(ChooseRandomHeight());
            }
        }
    }
}
