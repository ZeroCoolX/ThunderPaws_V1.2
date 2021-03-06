﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantParallax : Parallaxing {
    private float _moveSpeed = 0.5f;
    private float _scaleFactor = 0.125f;
    private Transform _mainCamera;
    private Vector3 _previousCamPosition;

    public Transform ParentContainer;

    protected new void Awake() {
        base.Awake();
        _mainCamera = Camera.main.transform;
    }

    // Use this for initialization
    void Start() {
        // The previous frame had the current frames camera position
        _previousCamPosition = _mainCamera.position;
        for (int i = 0; i < Backgrounds.Length; ++i) {
            ParallaxScales[i] = _moveSpeed * -1f;
            _moveSpeed += _scaleFactor;
        }
    }

    // This should only be done for the relative backgrounds
    void Update() {
        ApplyParallax();
    }
    private bool ProgressingForward() {
        return (_previousCamPosition.x - _mainCamera.position.x) >= 0;
    }

    protected override void ApplyParallax() {
        for (int i = 0; i < Backgrounds.Length; ++i) {

            // Set a targert X position which is the current position plus the parallax
            float parallax = (_previousCamPosition.x - _mainCamera.position.x) * -1;
            float backgroundTargetPosX = Backgrounds[i].position.x + parallax;
            if (Mathf.Abs(parallax) <= 0.001f) {
                backgroundTargetPosX += ParallaxScales[i];
                // Create a target position which is the background's current position with it's target x position
                Vector3 backgroundTargetPosition = new Vector3(backgroundTargetPosX, Backgrounds[i].position.y, Backgrounds[i].position.z);
                Backgrounds[i].position = Vector3.Lerp(Backgrounds[i].position, backgroundTargetPosition, Smoothing * Time.deltaTime);
            } else {
                backgroundTargetPosX += ParallaxScales[i] + parallax;
                // Create a target position which is the background's current position with it's target x position
                Vector3 backgroundTargetPosition = new Vector3(backgroundTargetPosX, Backgrounds[i].position.y, Backgrounds[i].position.z);
                // Also add for the Y because I WILL be moving up and down a lot

                // Fade between the backgrounds current position and the target position
                Backgrounds[i].position = Vector3.Lerp(Backgrounds[i].position, backgroundTargetPosition, (1f - (Mathf.Abs(parallax) + ParallaxScales[i])) * Time.deltaTime);
            }
        }
        // Set the previous cam position to the cams position
        _previousCamPosition = _mainCamera.position;
        ParentContainer.position = new Vector3(_mainCamera.position.x, ParentContainer.position.y, ParentContainer.position.z);
    }
}
