using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugGameStatsController : MonoBehaviour {

    public Text Player1CoinText;
    public Text Player1BaddieText;
    public Text Player1UltsText;
    public Text Player1DiedText;
    public Text Player1TimeText;

    public Text Player2CoinText;
    public Text Player2BaddieText;
    public Text Player2UltsText;
    public Text Player2DiedText;
    public Text Player2TimeText;


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Player1CoinText.text = "Coins Collected: " + GameStatsManager.Instance.CoinsCollected(1);
        Player1BaddieText.text = "Baddies Killed: " + GameStatsManager.Instance.BaddiesKilled(1);
        Player1UltsText.text = "Times Died: " + GameStatsManager.Instance.TimesDied(1);
        Player1DiedText.text = "Ults Used: " + GameStatsManager.Instance.UltimatesUsed(1);
        Player1TimeText.text = "Elapsed Time: " + GameStatsManager.Instance.LevelTime(1);

        Player2CoinText.text = "Coins Collected: " + GameStatsManager.Instance.CoinsCollected(2);
        Player2BaddieText.text = "Baddies Killed: " + GameStatsManager.Instance.BaddiesKilled(2);
        Player2UltsText.text = "Times Died: " + GameStatsManager.Instance.TimesDied(2);
        Player2DiedText.text = "Ults Used: " + GameStatsManager.Instance.UltimatesUsed(2);
        Player2TimeText.text = "Elapsed Time: " + GameStatsManager.Instance.LevelTime(2);
    }
}
