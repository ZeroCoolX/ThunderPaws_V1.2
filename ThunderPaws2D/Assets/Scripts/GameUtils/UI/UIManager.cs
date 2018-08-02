using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {
    public static UIManager Instance;

    private Dictionary<string, Transform> _uIMap = new Dictionary<string, Transform>();

    private void Awake() {
        // Ensure the object persists through the lifetime of the scene
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    void Start () {
        var ui = GameObject.FindGameObjectWithTag("UIOverlay");

        var gameOver = ui.transform.Find("GameOver");
        if(gameOver != null) { _uIMap.Add(gameOver.gameObject.name, gameOver); }

        var gameLost = ui.transform.Find("GameLost");
        if (gameLost != null) { _uIMap.Add(gameLost.gameObject.name, gameLost); }

        var inputBinding = ui.transform.Find("InputBinding");
        if (inputBinding != null) { _uIMap.Add(inputBinding.gameObject.name, inputBinding); }

        var difficultyScreen = ui.transform.Find("DifficultySelector");
        if (difficultyScreen != null) { _uIMap.Add(difficultyScreen.gameObject.name, difficultyScreen); }

        var controlScreen = ui.transform.Find("Controls");
        if (controlScreen != null) { _uIMap.Add(controlScreen.gameObject.name, controlScreen); }

    }

    public Transform GetUi(string name) {
        Transform ui;
        _uIMap.TryGetValue(name, out ui);
        return ui;
    }
}
