using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TutorialControllerBase : MonoBehaviour {

    protected bool TutorialComplete = false;
    protected bool TutorialFailedReset = false;

	// Use this for initialization
	void Start () {
		
	}

    public abstract void IncrementProgress();
	
	// Update is called once per frame
	protected void Update () {
        if (TutorialComplete) {
            print("tutorial complete!");
            TutorialManager.Instance.TutorialCompleteMoveToNext();
            TutorialComplete = false;
        }
        if (TutorialFailedReset) {
            TutorialManager.Instance.TutorialFailedReset();
            TutorialFailedReset = false;
        }
	}
}
