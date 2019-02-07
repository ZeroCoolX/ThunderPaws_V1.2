using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingBaddieLifeform : BaddieLifeform {
    // I don't like this but for right now its necessary
    public bool IsHorde2Hack = false;
    public int BeserkHealthTrigger;
    private bool _beserkAttempted = false;
    protected bool Beserk = false;

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

    public void Update() {
        base.Update();
        BeserkCheck();
    }

    protected void BeserkCheck() {
        if(Health <= BeserkHealthTrigger && !_beserkAttempted) {
            // Baddie gets 1 Chance to go beserk!
            _beserkAttempted = true;
            GoBeserk();
        }
    }

    private void GoBeserk() {
        Random.InitState((int)Time.time);
        // Only 10 items are NOT chosen from 1-31 that do not satisfy %2=0 or %3=0
        // 1/3 of 30 is 10 - 2/3 is 66.6666666%
        var beserk = Random.Range(1, 31);
        if(beserk % 2 == 0 || beserk % 3 == 0) {
            Beserk = true;
        }
    }

    protected void SuicideDiveTarget() {
        float newX = Mathf.SmoothDamp(transform.position.x, Target.position.x, ref FlyingPositionData.VelocityXSmoothing, 0.5f);
        float newY = Mathf.SmoothDamp(transform.position.y, Target.position.y, ref FlyingPositionData.VelocityYSmoothing, 0.5f);
        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}
