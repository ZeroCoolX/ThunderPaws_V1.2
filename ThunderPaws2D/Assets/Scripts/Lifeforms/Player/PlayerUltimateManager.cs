using System.Collections.Generic;
using UnityEngine;

public class PlayerUltimateManager : MonoBehaviour {
    public Ultimate[] Ultimates;

    private Ultimate _currentUltimate;

    public void ActivatePlayerUlt() {
        _currentUltimate.Activate();
    }

    public Ultimate GetCurrentUltimate() {
        return _currentUltimate;
    }

	// Use this for initialization
	public void Initialize (int playerNum, PlayerStats playerStats) {
        switch (ProfilePool.Instance.GetPlayerProfile(playerNum).GetSelectedUltimate()) {
            case GameConstants.ObjectName_TriggerPaw:
                _currentUltimate = Ultimates[0];
                break;
            case GameConstants.ObjectName_LighteningClaw:
                _currentUltimate = Ultimates[1];
                break;
            case GameConstants.ObjectName_ThunderPounce:
                _currentUltimate = Ultimates[2];
                break;
            case GameConstants.ObjectName_ReflectFurball:
                _currentUltimate = Ultimates[3];
                break;
        }
        _currentUltimate.PlayerStats = playerStats;
        _currentUltimate.PlayerNum = playerNum;
    }
}
