using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour {
    /// <summary>
    /// Main camera referece
    /// </summary>
    private Camera _mainCam;
    /// <summary>
    /// how much to shake the camera by
    /// </summary>
    private float _shakeAmount = 0f;

    /// <summary>
    /// Cache Main Camera reference
    /// </summary>
    private void Awake() {
        if (_mainCam == null) {
            _mainCam = Camera.main;
        }
    }

    /// <summary>
    /// Shake for the alloted length of time
    /// </summary>
    /// <param name="amt"></param>
    /// <param name="length"></param>
    public void Shake(float amt, float length) {
        _shakeAmount = amt;
        InvokeRepeating("DoShake", 0, 0.01f);
        Invoke("StopShake", length);
    }

    /// <summary>
    /// Apply shake to the camera.
    /// Calculation found online courtesy of Brackeys
    /// </summary>
    private void DoShake() {
        if (_shakeAmount > 0f) {
            Vector3 camPos = _mainCam.transform.position;

            //Get shake values
            float offsetX = Random.value * _shakeAmount * 2 - _shakeAmount;
            float offsetY = Random.value * _shakeAmount * 2 - _shakeAmount;

            //Apply shake
            camPos.x += offsetX;
            camPos.y += offsetY;
            //Move the main camera
            _mainCam.transform.position = camPos;
        }
    }

    /// <summary>
    /// Stop shake
    /// </summary>
    private void StopShake() {
        CancelInvoke("DoShake");
        //Zero out the main camera objects transform which will just set it to where the parent is - which is following the player like always
        _mainCam.transform.localPosition = new Vector3(0f, 0f, 0f);
    }
}
