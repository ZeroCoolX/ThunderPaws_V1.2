using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndependentParallax : Parallaxing {
    private Transform _mainCamera;
    private Vector3 _previousCamPosition;
    public float MoveSpeed;// = 0.05f;

    protected new void Awake() {
        base.Awake();
        _mainCamera = Camera.main.transform;
    }

    // Use this for initialization
    void Start() {
        // The previous frame had the current frames camera position
        _previousCamPosition = _mainCamera.position;
        for (int i = 0; i < Backgrounds.Length; ++i) {
            ParallaxScales[i] = MoveSpeed;
        }
    }

    // This should only be done for the relative backgrounds
    void Update() {
        ApplyParallax();
    }

    protected override void ApplyParallax() {
        for (int i = 0; i < Backgrounds.Length; ++i) {
            // The parallax is the opposite of the camera movement because the previous frame multiplied by the scale
            float parallax = (_previousCamPosition.x - _mainCamera.position.x) * -1;

            // Set a targert X position which is the current position plus the parallax
            float backgroundTargetPosX = Backgrounds[i].position.x + parallax;

            // Create a target position which is the background's current position with it's target x position
            Vector3 backgroundTargetPosition = new Vector3(backgroundTargetPosX, Backgrounds[i].position.y, Backgrounds[i].position.z);
            // Also add for the Y because I WILL be moving up and down a lot

            // Fade between the backgrounds current position and the target position
            Backgrounds[i].position = Vector3.Lerp(Backgrounds[i].position, backgroundTargetPosition, 1f - Mathf.Abs(ParallaxScales[i]));
        }

        // Set the previous cam position to the cams position
        _previousCamPosition = _mainCamera.position;
    }
}
