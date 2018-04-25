using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial3Controller : TutorialControllerBase {

    public bool[] Indicators = new bool[3] { false, false, false};
    private int index = 0;

    public override void IncrementProgress() {
        Indicators[index] = true;
        ++index;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        base.Update();
        foreach (var indicator in Indicators) {
            if (!indicator) {
                return;
            }
        }
        TutorialComplete = true;
	}
}
