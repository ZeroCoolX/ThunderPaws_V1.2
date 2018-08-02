using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MARKED FOR DEPRECATION / REMOVAL
/// </summary>
public class DifficultyUI : MonoBehaviour {
    // Im fairly cetain this is deprecated
    public void Update() {
    }

	public void SelectDifficulty(string diff) {
        DifficultyManager.Instance.Difficulty = diff.ToLower();
        DifficultyManager.Instance.SetDifficulty();
    }
}
