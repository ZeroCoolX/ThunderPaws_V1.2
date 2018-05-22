using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScreenUI : MonoBehaviour {

	public void PlayGame() {
        SceneManager.LoadScene("PreAlphaDemoPlayerSelect");
    }

    public void PlayTutorial() {
        SceneManager.LoadScene("PreAlphaDemoTutorial1");
    }
}
