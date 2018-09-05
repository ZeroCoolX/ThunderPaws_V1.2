using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectUI : MonoBehaviour {

	public void SelectLevel(string level) {
        DifficultyManager.Instance.LevelToPlay = GameConstants.GetLevel(level);
    }
}
