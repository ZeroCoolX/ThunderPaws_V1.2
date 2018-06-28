﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAudio : MonoBehaviour {

    [SerializeField]
    public string PlayOnStart;

    private bool _queryAudio = true;

    private void Update() {
        if (_queryAudio) {
            try {
                AudioManager.Instance.playSound(PlayOnStart);
                _queryAudio = false;
            } catch (System.Exception e) {
                print("SimpleAudio was unable to play the audio clip : " + PlayOnStart + "\n Error message: " + e.Message);
                return;
            }
        }
    }
}