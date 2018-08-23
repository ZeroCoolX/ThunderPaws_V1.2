using System;
using UnityEngine;
public class Camera2DFollow : FollowBase {
    public float VerticalOffset = 3f;
    public float HordeVerticalOffset;
    public bool FreeMovement = true;

    private float _currentXOffset;
    private const float _bottomYThreshold = -19;

    private float _originPoint;

    private void Start() {
        _currentXOffset = OffsetX;
        InitializeSearchName(GameConstants.Tag_Player);
        FindPlayer();
        base.Start();

        // After we find the player we need to store our origin point. 
        if(Target != null) {
            RetrieveOriginPoint();
        }
    }

    private void RetrieveOriginPoint() {
        _originPoint = Target.position.y;
    }

    private bool TooFarFromOrigin() {
        if(Mathf.Sign(_originPoint) == Mathf.Sign(Target.position.y)) {
            return Mathf.Abs(_originPoint - Target.position.y) > 5;
        } else {
            return (Mathf.Abs(_originPoint) + Mathf.Abs(Target.position.y)) > 5;
        }
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

       LookAheadPos = Vector3.MoveTowards(LookAheadPos, Vector3.zero, Time.deltaTime * LookAheadReturnSpeed);

        Vector3 tempPos = Target.position + LookAheadPos + Vector3.forward * OffsetZ;
        Vector3 aheadTargetPos = new Vector3(tempPos.x, 1, tempPos.z);
        if (TooFarFromOrigin()) {
            aheadTargetPos.y = tempPos.y;
            aheadTargetPos.y += HordeVerticalOffset;

        } else {
            aheadTargetPos.y = _originPoint;
            aheadTargetPos.y += VerticalOffset;
        }

        Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref CurrentVelocity, HorizontalDampening);
        newPos = new Vector3(newPos.x, Mathf.Clamp(newPos.y, YCameraPosClamp, Mathf.Infinity), -50f);
        transform.position = newPos;

        LastTargetPosition = Target.position;
    }
}
