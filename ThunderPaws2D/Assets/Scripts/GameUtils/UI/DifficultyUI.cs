﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyUI : MonoBehaviour {

    public void Update() {
        var text = transform.Find("DifficultyText").GetComponent<UnityEngine.UI.Text>();
        var diff = GameMaster.Instance.Difficulty;
        text.text = "Difficulty: " + GameMaster.Instance.Difficulty + "\n" + (diff.ToLower() == "easy" ? "10 lives (500 health)" : (diff.ToLower() == "normal" ? "5 lives (250 health)" : "3 lives (100 health)"));
    }

	public void SelectDifficulty() {
        GameMaster.Instance.SetDifficulty();
    }
}