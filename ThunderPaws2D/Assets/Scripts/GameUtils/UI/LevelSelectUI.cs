using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectUI : MonoBehaviour {

	public void SelectLevel(int level) {
        DifficultyManager.Instance.LevelToPlay = level;
    }
}
