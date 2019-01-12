using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Profile {
    public string ProfileName;

    private Dictionary<string, bool> _weaponCache = new Dictionary<string, bool> {
        { "gauss", false},
        { "shotgun", false},
        { "emissionindex", false},
        { "fatcat", false}
    };
    private Dictionary<string, bool> _ultimateCache = new Dictionary<string, bool> {
        { "lighteneingclaw", false},
        { "thunderpounce", false},
        { "triggerpaw", false},
        { "reflectfurball", false}
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
        string path = @"Assets/Profiles/" + ProfileName;
        var profileModel = JsonUtility.FromJson<ProfileModel>(File.ReadAllText(path));

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
        string path = @"Assets/Profiles/" + ProfileName;


        var model = new ProfileModel {
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
