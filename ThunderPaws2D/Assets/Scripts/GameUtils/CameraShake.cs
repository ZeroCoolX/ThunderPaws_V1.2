﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour {

    private Camera _mainCam;
    private float _shakeAmount = 0f;


    private void Awake() {
        if (_mainCam == null) {
            _mainCam = Camera.main;
        }
    }

    public void Shake(float amt, float length) {
        _shakeAmount = amt;
        InvokeRepeating("DoShake", 0, 0.01f);
        Invoke("StopShake", length);
    }

    private void DoShake() {
        if (_shakeAmount > 0f) {
            Vector3 camPos = _mainCam.transform.position;

            // Get shake values
            float offsetX = Random.value * _shakeAmount * 2 - _shakeAmount;
            float offsetY = Random.value * _shakeAmount * 2 - _shakeAmount;

            // Apply shake
            camPos.x += offsetX;
            camPos.y += offsetY;

            // Move the main camera
            _mainCam.transform.position = camPos;
        }
    }

    private void StopShake() {
        CancelInvoke("DoShake");
        // Zero out the main camera objects transform which will just set it to where the parent is - which is following the player like always
        _mainCam.transform.localPosition = new Vector3(0f, 0f, 0f);
    }
}
