using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour {

    public static TutorialManager Instance;

    /// <summary>
    /// Reference to all the tutorial scenes
    /// </summary>
    public string[] TutorialScenes;

    public int CurrentTutorial = 0;

    private void Awake() {
        if (Instance == null) {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    public void TutorialCompleteMoveToNext() {
        // Need to check if we're on the last tutorial
        ++CurrentTutorial;
        Invoke("ChangeScenes", 1f);
    }

    private void ChangeScenes() {
        SceneManager.LoadScene(TutorialScenes[CurrentTutorial]);
    }

    public void TutorialFailedReset() {
        SceneManager.LoadScene(TutorialScenes[CurrentTutorial]);
    }
}
