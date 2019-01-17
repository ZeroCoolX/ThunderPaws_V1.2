using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour {
    public Transform[] Levels;
    public Button[] LevelButtons;
    public int StageNum;

    private Profile _player1Profile;

    private void Start() {
        _player1Profile = ProfilePool.Instance.GetPlayerProfile(1);

        var unlockedLevels = _player1Profile.GetLevelsUnlockedForStage(StageNum);
        for (var i = 0; i < Levels.Length; ++i) {
            Levels[i].GetComponent<TextMeshProUGUI>().text = GameConstants.GetLevelTest(unlockedLevels[i]);
            LevelButtons[i].gameObject.SetActive(unlockedLevels[i] > 0);
        }
    }



    public void SelectLevel(int level) {
        DifficultyManager.Instance.LevelToPlay = level;
    }
}
