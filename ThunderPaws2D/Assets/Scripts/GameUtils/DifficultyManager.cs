using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultyManager : MonoBehaviour {
    public static DifficultyManager Instance;
    public string Difficulty = GameConstants.Difficulty_Easy;
    public int LevelToPlay = 0;

    private Dictionary<string, int[]> _difficulties = new Dictionary<string, int[]>();

    private SceneLoader _sceneLoader;

    private void Start() {
        var loader = GameObject.Find("SceneLoader");
        if (loader != null) {
            _sceneLoader = loader.GetComponent<SceneLoader>();
        }
    }

    /// <summary>
    /// Store the difficulty by way of  Health and Lives in the 
    /// LivesManager static object that can be accessed at anytime
    /// thanks to how static objects live in the lifetime of an application
    /// </summary>
    public void SetDifficulty() {
        if (_sceneLoader == null) {
            print("Getting scene loader as a backup");
            var loader = GameObject.Find("SceneLoader");
            _sceneLoader = loader.GetComponent<SceneLoader>();
        }

        int[] values;
        var livesAndHealth = _difficulties.TryGetValue(Difficulty.ToLower(), out values);
        if (values == null) {
            print("Something went wrong - using the default difficulty");
            livesAndHealth = _difficulties.TryGetValue("easy", out values);
        }
        LivesManager.Lives = values[0];
        print("Setting heath to " + values[1]);
        LivesManager.Health = values[1];

        print("Beginning level : " + LevelToPlay);
        if(LevelToPlay == 11) {
            print("Loading scene");
            try {
                AudioManager.Instance.StopSound(GameConstants.Audio_MenuMusic);
            } catch (Exception e) {
                print("Menu Music could not be stopped");
            }
            SceneManager.LoadScene(GameConstants.Scene_Backstory_Menu);
        }
        else{
            if (LevelToPlay > 0) {
                if (_sceneLoader != null) {
                    print("Loading scene async");
                    _sceneLoader.LoadScene(GameConstants.GetLevel(LevelToPlay), GameConstants.Audio_MenuMusic);
                } else {
                    SceneManager.LoadScene(GameConstants.GetLevel(LevelToPlay));
                }
            }
        }
    }

    private void Awake() {
        if (Instance == null) {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }

        // Pre-store difficulties
        _difficulties.Add(GameConstants.Difficulty_Easy, new int[] { 7, 300 });
        _difficulties.Add(GameConstants.Difficulty_Normal, new int[] { 5, 200 });
        _difficulties.Add(GameConstants.Difficulty_Hard, new int[] { 3, 150 });
    }

}
