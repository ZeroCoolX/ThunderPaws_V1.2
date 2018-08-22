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

       LookAheadPos = Vector3.MoveTowards(LookAheadPos, Vector3.zero, Time.deltaTime * LookAheadReturnSpeed);

        // come back to when you want to have even BETTER camera functionality
        /*Vector3 aheadTargetPos = Target.position + LookAheadPos + Vector3.forward * OffsetZ;
        print("Mathf.Abs(transform.position.y - Target.position.y) = " + Mathf.Abs(transform.position.y - Target.position.y));
        aheadTargetPos.y += VerticalOffset;
        if (Mathf.Abs(transform.position.y - Target.position.y) < 3.5) {
            aheadTargetPos.y = transform.position.y;
        }

        Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref CurrentVelocity, HorizontalDampening);
        newPos = new Vector3(newPos.x, Mathf.Clamp(newPos.y, YCameraPosClamp, Mathf.Infinity), -50f);
        transform.position = newPos;*/

        Vector3 aheadTargetPos = Target.position + LookAheadPos + Vector3.forward * OffsetZ;
        aheadTargetPos.y += VerticalOffset;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref CurrentVelocity, HorizontalDampening);
        newPos = new Vector3(newPos.x, Mathf.Clamp(newPos.y, YCameraPosClamp, Mathf.Infinity), -50f);
        transform.position = newPos;

        LastTargetPosition = Target.position;
    }
}
