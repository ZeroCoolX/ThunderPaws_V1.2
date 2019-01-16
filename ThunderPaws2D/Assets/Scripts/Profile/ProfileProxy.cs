using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileProxy : MonoBehaviour {

    public int PlayerNumber;

    private Profile _profile;

    public int GetEmissionCache() {
        return _profile.GetEmissionCache();
    }

    public int UpdateEmissionCache(int amount) {
        var currentCacheAmount = _profile.UpdateEmissionCache(amount);
        _profile.SaveProfile();
        return currentCacheAmount;
    }

    public List<string> GetUnlockedWeapons() {
        return _profile.GetUnlockedWeapons();
    }
    public void UnlockWeapon(string weapon) {
        if (!_profile.UnlockWeapon(weapon)) {
            print("Unable to unlock weapon [" + weapon + "] - Key was not found");
        }
        _profile.SaveProfile();
    }

    public List<string> GetUnlockedUltimates() {
        return _profile.GetUnlockedUltimates();
    }
    public void UnlockUltimate(string ultimate) {
        if (!_profile.UnlockUltimate(ultimate)) {
            print("Unable to unlock ultimate [" + ultimate + "] - Key was not found");
        }
        _profile.SaveProfile();
    }

    public List<string> GetUnlockedLevels() {
        return _profile.GetUnlockedLevels();
    }
    public void UnlockLevel(string level) {
        if (!_profile.UnlockLevel(level)) {
            print("Unable to unlock level [" + level + "] - Key was not found");
        }
        _profile.SaveProfile();
    }

    public string PrettyPrint() {
        return ProfilePool.Instance.PrettyPrint(_profile.ProfileName);
    }

    private void Awake() {
        _profile = ProfilePool.Instance.GetPlayerProfile(PlayerNumber);
    }
}
