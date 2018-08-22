using System;
using UnityEngine;
public class Camera2DFollow : FollowBase {
    public float VerticalOffset = 5f;
    public bool FreeMovement = true;

    private float _currentXOffset;
    private const float _bottomYThreshold = -19;



    private void Start() {
        _currentXOffset = OffsetX;
        InitializeSearchName(GameConstants.Tag_Player);
        FindPlayer();
        base.Start();
    }

    private void Update() {
        if (Target == null) {
            FindPlayer();
            return;
        }
        HandleFreePositionMovement();
    }

    private void HandleFreePositionMovement() {
        if (!FreeMovement && Target.position.x < LastTargetPosition.x) {
            return;
        }
        // Only update lookahead pos if accelerating or changed direction
        float xMoveDelta = (Target.position - LastTargetPosition).x;
        bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > LookAheadMoveThreshold;

       // if (updateLookAheadTarget) {
       //     LookAheadPos = LookAheadFactor * Vector3.right * Mathf.Sign(xMoveDelta);
       // } else {
            LookAheadPos = Vector3.MoveTowards(LookAheadPos, Vector3.zero, Time.deltaTime * LookAheadReturnSpeed);
       // }

        Vector3 aheadTargetPos = Target.position + LookAheadPos + Vector3.forward * OffsetZ;
        aheadTargetPos.y += VerticalOffset;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref CurrentVelocity, Dampening);
        newPos = new Vector3(newPos.x, Mathf.Clamp(newPos.y, YCameraPosClamp, Mathf.Infinity), -50f);
        transform.position = newPos;

        LastTargetPosition = Target.position;
    }
}
