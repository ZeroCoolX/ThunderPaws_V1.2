using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Profile {
    public string ProfileName;

    private string _selectedWeapon;
    private string _selectedUltimate;

    private Dictionary<string, bool> _weaponCache = new Dictionary<string, bool> {
        { GameConstants.ObjectName_GaussWeapon, false},
        {  GameConstants.ObjectName_ShotgunWeapon, false},
        {  GameConstants.ObjectName_EmissionIndexWeapon, false},
        {  GameConstants.ObjectName_FatCatWeapon, false}
    };
    private Dictionary<string, bool> _ultimateCache = new Dictionary<string, bool> {
        { GameConstants.ObjectName_LighteningClaw, false},
        { GameConstants.ObjectName_ThunderPounce, false},
        { GameConstants.ObjectName_TriggerPaw, false},
        { GameConstants.ObjectName_ReflectFurball, false}
    };
    private Dictionary<string, bool> _levelCache = new Dictionary<string, bool> {
        { "S1L1", true},
        { "S1L2", false},
        { "S1L3", false},
        { "S1L4", false},

        { "S2L1", false},
        { "S2L2", false},
        { "S2L3", false},
        { "S2L4", false},

        { "S3L1", false},
        { "S3L2", false},
        { "S3L3", false},
        { "S3L4", false},

        { "S4L1", false},
        { "S4L2", false},
    };
    private int _emissionCache;

    public void LoadProfileFromFile() {
        string path = Path.Combine("Assets", "Profiles");
        path = Path.Combine(path, ProfileName);

        var profileModel = JsonUtility.FromJson<ProfileModel>(File.ReadAllText(path));

        _emissionCache = profileModel.EmissionCache;
        _selectedWeapon = profileModel.SelectedWeapon;
        _selectedUltimate = profileModel.SelectedUltimate;

        foreach (var weapon in profileModel.Weapons) {
            UnlockWeapon(weapon);
        }
        foreach (var ultimate in profileModel.Ultimates) {
            UnlockUltimate(ultimate);
        }
        foreach (var level in profileModel.Levels) {
            UnlockLevel(level);
        }
    }

    public void SaveProfile() {
        string path = Path.Combine("Assets", "Profiles");
        path = Path.Combine(path, ProfileName);


        var model = new ProfileModel {
            SelectedUltimate = _selectedUltimate,
            SelectedWeapon = _selectedWeapon,
            EmissionCache = _emissionCache,
            Weapons = GetUnlockedWeapons(),
            Ultimates = GetUnlockedUltimates(),
            Levels = GetUnlockedLevels(),
        };

        using (StreamWriter sw = new StreamWriter(File.Open(path, FileMode.Create))) {
            var json = JsonUtility.ToJson(model);
            sw.Write(json);
            sw.Close();
        }
    }

    public byte[] GetLevelsUnlockedForStage(int stageNum) {
        var stage = "S" + stageNum;
        var levels = new byte[4];
        for (var i = 0; i < levels.Length; ++i) {
            // If at least the first level of the stage is unlocked then the entire stage is unlocked
            levels[i] = Convert.ToByte(IsLevelUnlocked(stage + "L" + (i + 1)));
        }
        return levels;
    }


    public bool IsLevelUnlocked(string level) {
        var levels = GetUnlockedLevels();
        return levels.Contains(level);
    }

    public byte[] GetStagesUnlocked() {
        var stages = new byte[4];
        for (var i = 0; i < stages.Length; ++i) {
            // If at least the first level of the stage is unlocked then the entire stage is unlocked
            stages[i] = Convert.ToByte(IsLevelUnlocked("S" + (i+1) + "L1"));
        }
        return stages;
    }

    public string GetSelectedWeapon() {
        return _selectedWeapon;
    }
    public string SetSelectedWeapon(string weapon) {
        if (!IsWeaponUnlocked(weapon)) {
            UnlockWeapon(weapon);
        }
        _selectedWeapon = weapon;
        SaveProfile();
        return _selectedWeapon;
    }

    public bool IsWeaponUnlocked(string weapon) {
        var weapons = GetUnlockedWeapons();
        return weapons.Contains(weapon);
    }

    public string GetSelectedUltimate() {
        return _selectedUltimate;
    }

    public string SetSelectedUltimate(string ultimate) {
        if (!IsUltimateUnlocked(ultimate)) {
            UnlockUltimate(ultimate);
        }
        _selectedUltimate = ultimate;
        SaveProfile();
        return _selectedUltimate;
    }

    public bool IsUltimateUnlocked(string ultimate) {
        var ultimates = GetUnlockedUltimates();
        return ultimates.Contains(ultimate);
    }

    public int GetEmissionCache() {
        return _emissionCache;
    }
    public int UpdateEmissionCache(int amount) {
        _emissionCache += amount;
        return _emissionCache;
    }

    public List<string> GetUnlockedWeapons() {
        return _weaponCache.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
    }
    public bool UnlockWeapon(string weapon) {
        return Unlock(_weaponCache, weapon);
    }

    public List<string> GetUnlockedUltimates() {
        return _ultimateCache.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
    }
    public bool UnlockUltimate(string ultimate) {
        return Unlock(_ultimateCache, ultimate);
    }

    public List<string> GetUnlockedLevels() {
        return _levelCache.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
    }
    public bool UnlockLevel(string level) {
        return Unlock(_levelCache, level);
    }

    private bool Unlock(Dictionary<string, bool> map, string key) {
        if (!map.ContainsKey(key)) {
            return false;
        }
        map[key] = true;
        SaveProfile();
        return true;
    }
}
