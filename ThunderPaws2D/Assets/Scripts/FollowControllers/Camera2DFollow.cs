using System;
using UnityEngine;

namespace UnityStandardAssets._2D {
    public class Camera2DFollow : FollowBase {

        private float _currentXOffset;
        /// <summary>
        /// Flag to indicate whether the player can move freely around the area or they're forced to go forward
        /// </summary>
        public bool LockedPositionMovement = true;

        private const float _bottomYThreshold = -19;

        private void Start() {
            _currentXOffset = OffsetX;
            InitializeSearchName(GameConstants.Tag_Player);
            FindPlayer();
            base.Start();
        }


        // Update is called once per frame
        private void Update() {
            //Dead player check
            if (Target == null) {
                FindPlayer();
                return;
            }
            if (LockedPositionMovement) {
                HandleLockedPositionMovement();
            } else {
                HandleFreePositionMovement();
            }
        }

        private void HandleLockedPositionMovement() {
            // only update lookahead pos if accelerating or changed direction
            float xMoveDelta = (Target.position - LastTargetPosition).x;
            bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > LookAheadMoveThreshold;

            if (updateLookAheadTarget) {
                LookAheadPos = LookAheadFactor * Vector3.right * Mathf.Sign(xMoveDelta);
            } else {
                LookAheadPos = Vector3.MoveTowards(LookAheadPos, Vector3.zero, Time.deltaTime * LookAheadReturnSpeed);
            }

            Vector3 aheadTargetPos = Target.position + LookAheadPos + Vector3.forward * OffsetZ;
            // TODO: add this offset as a configurable variable
            aheadTargetPos.y += 3;
            Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref CurrentVelocity, Dampening);
            newPos = new Vector3(newPos.x, Mathf.Clamp(newPos.y, YPosClamp, Mathf.Infinity), newPos.z);

            transform.position = newPos;
            LastTargetPosition = Target.position;
        }

        private void HandleFreePositionMovement() {
            // only update lookahead pos if accelerating or changed direction
            float xMoveDelta = (Target.position - LastTargetPosition).x;
            bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > LookAheadMoveThreshold;

            if (updateLookAheadTarget) {
                LookAheadPos = LookAheadFactor * Vector3.right * Mathf.Sign(xMoveDelta);
            } else {
                LookAheadPos = Vector3.MoveTowards(LookAheadPos, Vector3.zero, Time.deltaTime * LookAheadReturnSpeed);
            }

            Vector3 aheadTargetPos = Target.position + LookAheadPos + Vector3.forward * OffsetZ;
            Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref CurrentVelocity, Dampening);
            newPos = new Vector3(newPos.x, Mathf.Clamp(newPos.y, YPosClamp, Mathf.Infinity), newPos.z);
            transform.position = newPos;

            LastTargetPosition = Target.position;
        }
    }
}
