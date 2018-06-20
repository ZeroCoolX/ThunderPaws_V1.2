using System;
using UnityEngine;
    public class Camera2DFollow : FollowBase {

        private float _currentXOffset;
        /// <summary>
        /// Flag to indicate whether the player can move freely around the area or they're forced to go forward
        /// </summary>
        public bool FreeMovement = true;
        public float YOffset = 5f;
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
            HandleFreePositionMovement();
        }

        private void HandleFreePositionMovement() {
        print("free");
        if(!FreeMovement && Target.position.x < LastTargetPosition.x) {
            return;
        }
            // only update lookahead pos if accelerating or changed direction
            float xMoveDelta = (Target.position - LastTargetPosition).x;
            bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > LookAheadMoveThreshold;

            if (updateLookAheadTarget) {
                LookAheadPos = LookAheadFactor * Vector3.right * Mathf.Sign(xMoveDelta);
            } else {
                LookAheadPos = Vector3.MoveTowards(LookAheadPos, Vector3.zero, Time.deltaTime * LookAheadReturnSpeed);
            }

            Vector3 aheadTargetPos = Target.position + LookAheadPos + Vector3.forward * OffsetZ;
            aheadTargetPos.y += YOffset;
            Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref CurrentVelocity, Dampening);
            newPos = new Vector3(newPos.x, Mathf.Clamp(newPos.y, YPosClamp, Mathf.Infinity), newPos.z);
            transform.position = newPos;

            LastTargetPosition = Target.position;
        }
    }
