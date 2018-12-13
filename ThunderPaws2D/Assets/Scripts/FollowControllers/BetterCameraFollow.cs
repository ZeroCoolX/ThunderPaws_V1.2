using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetterCameraFollow : MonoBehaviour {

    public Vector2 FocusAreaSize;

    private CollisionController2D _target;
    private FocusArea _focusArea;

    private float nextTimeToSearch = 0f;
    private float searchDelay = 0.25f;
    private string _searchName;

    private void Start() {
        if (_target == null) {
            FindPlayer();
            return;
        }
        _focusArea = new FocusArea(_target.BoxCollider.bounds, FocusAreaSize);
    }

    protected void FindPlayer() {
        if (nextTimeToSearch <= Time.time) {
            GameObject searchResult = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);
            if (searchResult != null) {
                if (!searchResult.GetComponent<BaddieActivator>().enabled) {
                    Invoke("DelayedActivate", 1);
                }
                _target = searchResult.transform.GetComponent<CollisionController2D>();
                _focusArea = new FocusArea(_target.BoxCollider.bounds, FocusAreaSize);
                nextTimeToSearch = Time.time + searchDelay;
            }
        }
    }

    private void LateUpdate() {
        if (_target == null) {
            FindPlayer();
            return;
        }
        _focusArea.Update(_target.BoxCollider.bounds);
    }

    private void OnDrawGizmos() {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(_focusArea.Center, FocusAreaSize);
    }

    struct FocusArea {
        public Vector2 Center;
        public Vector2 Velocity;
        float Left, Right;
        float Top, Bottom;

        public FocusArea(Bounds targetBounds, Vector2 size) {
            Left = targetBounds.center.x - (size.x / 2);
            Right = targetBounds.center.x + (size.x / 2);
            Bottom = targetBounds.min.y;
            Top = targetBounds.min.y + size.y;

            Velocity = Vector2.zero;
            Center = new Vector2((Left + Right) / 2, (Top + Bottom) / 2);
        }

        public void Update(Bounds targetBounds) {
            // check if the player is moving up against either the left or right edge
            float amountToShiftX = 0;
            if(targetBounds.min.x < Left) {
                amountToShiftX = targetBounds.min.x - Left;
            }else if(targetBounds.max.x > Right) {
                amountToShiftX = targetBounds.max.x - Right;
            }
            Left += amountToShiftX;
            Right += amountToShiftX;

            // check if the player is moving up against either the top or bottom edge
            float amountToShiftY = 0;
            if (targetBounds.min.y < Bottom) {
                amountToShiftY = targetBounds.min.y - Bottom;
            } else if (targetBounds.max.y > Top) {
                amountToShiftY = targetBounds.max.y - Top;
            }
            Top += amountToShiftY;
            Bottom += amountToShiftY;

            //copy center position
            Center = new Vector2((Left + Right) / 2, (Top + Bottom) / 2);
            Velocity = new Vector2(amountToShiftX, amountToShiftY);
        }
    }
}
