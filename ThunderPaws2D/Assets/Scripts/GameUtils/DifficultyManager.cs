using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultyManager : MonoBehaviour {
    public static DifficultyManager Instance;

    /// <summary>
    /// Map that contains the different levels
    /// </summary>
    private Dictionary<string, int[]> _difficulties = new Dictionary<string, int[]>();
    /// <summary>
    /// This is really just fluff.
    /// Its the physical objects (Bman heads) that the user interacts with in order to
    /// be able to pick a difficulty. This can/will be changed later but for its fun
    /// </summary>
    //public Transform[] DifficultyObjects = new Transform[3];
    /// <summary>
    /// Set by the player in the menu. Default is easy
    /// </summary>
    public string Difficulty = GameConstants.Difficulty_Easy;

    /// <summary>
    /// Store the difficulty by way of  Health and Lives in the 
    /// LivesManager static object that can be accessed at anytime
    /// thanks to how static objects live in the lifetime of an application
    /// </summary>
    public void SetDifficulty() {
        int[] values;
        var livesAndHealth = _difficulties.TryGetValue(Difficulty.ToLower(), out values);
        if (values == null) {
            print("Something went wrong - using the default difficulty");
            livesAndHealth = _difficulties.TryGetValue("easy", out values);
        }
        LivesManager.Lives = values[0];
        print("Setting heath to " + values[1]);
        LivesManager.Health = values[1];

        // Load World
        SceneManager.LoadScene(GameConstants.Scene_LevelName_1);
    }

    private void Awake() {
        if (Instance != null) {
            if (Instance != this) {
                Destroy(this.gameObject);
            }
        } else {
            Instance = this;
        }

        // Difficulty, [lives, max health]
        _difficulties.Add(GameConstants.Difficulty_Easy, new int[] { 10, 500 });
        _difficulties.Add(GameConstants.Difficulty_Normal, new int[] { 5, 250 });
        _difficulties.Add(GameConstants.Difficulty_Hard, new int[] { 3, 100 });
    }

}
