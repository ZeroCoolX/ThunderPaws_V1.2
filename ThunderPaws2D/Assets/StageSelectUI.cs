using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectUI : MonoBehaviour {
    public Transform[] Stages;
    public Button[] StageButtons;

    private Profile _player1Profile;

    private void Start() {
        _player1Profile = ProfilePool.Instance.GetPlayerProfile(1);

        var unlockedStages = _player1Profile.GetStagesUnlocked();
        foreach(var stage in unlockedStages) { print("stage " + stage); }
        for(var i = 0; i < Stages.Length; ++i) {
            var stage = (i + 1) * unlockedStages[i];
            Stages[i].GetComponent<TextMeshProUGUI>().text = GameConstants.GetStage(stage);
            StageButtons[i].gameObject.SetActive(stage > 0);
        }
    }
}
