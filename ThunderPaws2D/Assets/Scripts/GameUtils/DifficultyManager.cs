using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultyManager : MonoBehaviour {
    public static DifficultyManager Instance;
    public string Difficulty = GameConstants.Difficulty_Easy;
    public string LevelToPlay;

    private Dictionary<string, int[]> _difficulties = new Dictionary<string, int[]>();



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

        try {
            AudioManager.Instance.StopSound(GameConstants.Audio_MenuMusic);
        }catch(System.Exception e) {
            print("We couldn't stop the music because it wasn't playing. Move along");
        }
        print("Beginning level : " + LevelToPlay);
        if(string.IsNullOrEmpty(LevelToPlay) || "S1L1".Equals(LevelToPlay)) {
            SceneManager.LoadScene(GameConstants.Scene_Backstory_Menu);
        }
        else{
            SceneManager.LoadScene(LevelToPlay);
        }
    }

    private void Awake() {
        if (Instance != null) {
            if (Instance != this) {
                Destroy(this.gameObject);
            }
        } else {
            Instance = this;
        }

        // Pre-store difficulties
        _difficulties.Add(GameConstants.Difficulty_Easy, new int[] { 10, 500 });
        _difficulties.Add(GameConstants.Difficulty_Normal, new int[] { 5, 250 });
        _difficulties.Add(GameConstants.Difficulty_Hard, new int[] { 3, 100 });
    }

}
