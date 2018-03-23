using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour { 
    // Who we are tracking - in the future change this to be an array since there could be two players
    private Transform _target;

    // This allows us to only search for the player every so often instead of every frame
    private float _nextTimeToSearch = 0f;
    // Wait half a second to search for the player 
    private float _searchDelay = 0.5f;

    // Necessary to keep the camera zoomed out and not right on top of the target
    private float zOffset = -1;

    // 1 - FM (Free Movement)
    // 2 - FFM (Fixed Free Movement)
    // 3 - FFP (Forced Forward Progression)
    // Default = 1;
    public int MovementMode = 1;


    // Necessary for FFM or FFP
    // This is the origin of the fixewd camera for where all boundaries are calculated off
    private Vector3 _fixedOrigin;
    // How far right or left the camera can deviate from the origin point
    private float _allowedHorizontalMovement = 5f;
    // Need to store the last position we were in for boundary reasons
    private Vector3 _lastPosition;
    // Indicates 1/4 the camera width
    private float xOffset;


    // Use this for initialization
    void Start () {
		if(_target == null) {
            FindPlayer();
        }
        zOffset = (transform.position - _target.position).z;
        // Get 1/4th of the screen width
        float height = Camera.main.orthographicSize * 2.0f;
        float width = height * Screen.width / Screen.height;
        xOffset = (width / 2f) / 2f;
        print("xOffset = " + xOffset);
    }

    /// <summary>
    /// Find the player by Tag
    /// </summary>
    protected void FindPlayer() {
        if (_nextTimeToSearch <= Time.time) {
            GameObject searchResult = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);
            if (searchResult != null) {
                _target = searchResult.transform;
                _nextTimeToSearch = Time.time + _searchDelay;
            }
        }
    }

    // Update is called once per frame
    void Update () {
        // Make sure we have a player
        if (_target == null) {
            FindPlayer();
        }
        // Hotkeys must have ctrl being held down
        if (Input.GetKey(KeyCode.C)) {
            // FM
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                print("FM Activated");
                MovementMode = 1;
            }
            // FFM
            else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                print("FFM Activated");
                MovementMode = 2;
                _fixedOrigin = _target.position;
            }
            // FFP
            else if (Input.GetKeyDown(KeyCode.Alpha3)) {
                print("FFP Activated");
                MovementMode = 3;
                // The first time we enter FFP we zero out the last position so its not tainted
                _lastPosition = Vector3.zero;
            }
        }

        ApplyTracking();
    }

    private Vector3 PadPosition() {
        return new Vector3(_target.position.x, _target.position.y, zOffset);
    }

    private void ApplyTracking() {
        // FM
        if(MovementMode == 1) {
            transform.position = PadPosition();
        }
        // FFP
        else if(MovementMode == 3) {
            // Indicates they're moving right. We only want to move right in Forced Forward Progression
            if (_lastPosition == Vector3.zero || _target.position.x >= _lastPosition.x) {
                print("Moving!");
                _fixedOrigin = new Vector3(_target.position.x + xOffset, _target.position.y, zOffset);
                transform.position = _fixedOrigin;
                _lastPosition = _target.position;
            }
        }
        // FFM
        else {
            var targetX = _target.position.x;
            // > 0 indicates target is right of origin
            // <= 0 indicates target is left of origin
            var cameraTargetDifference = Mathf.Abs(targetX) - Mathf.Abs(transform.position.x);
            if(targetX > _fixedOrigin.x && targetX < (_fixedOrigin.x + _allowedHorizontalMovement)) {
                print("Moving right");
                transform.position = PadPosition();
            } else if(targetX <= _fixedOrigin.x && targetX > (_fixedOrigin.x - _allowedHorizontalMovement)) {
                transform.position = PadPosition();
                print("Moving right");
            } else {
                print("Stop moving camera!");
                transform.position = _lastPosition;
            }
            _lastPosition = transform.position;
        }
    }
}
