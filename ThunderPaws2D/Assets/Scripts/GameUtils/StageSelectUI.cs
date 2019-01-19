using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectUI : MonoBehaviour {
    public Transform[] Stages;
    public Button[] StageButtons;

    public Transform LevelSelectUI;
    private Profile _player1Profile;

    private void Start() {
        if (LevelSelectUI == null) {
            throw new MissingComponentException("Missing the level select UI");
        }

        _player1Profile = ProfilePool.Instance.GetPlayerProfile(1);

        var unlockedStages = _player1Profile.GetStagesUnlocked();

        for(var i = 0; i < Stages.Length; ++i) {
            var stage = (i + 1) * unlockedStages[i];
            Stages[i].GetComponent<TextMeshProUGUI>().text = GameConstants.GetStage(stage);
            StageButtons[i].gameObject.SetActive(stage > 0);
        }
    }

    public void SetStageLevels(int stage) {
        LevelSelectUI.GetComponent<LevelSelectUI>().StageNum = stage;
    }
}
