using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class ProfilePool : MonoBehaviour{

    public static ProfilePool Instance;

    private const string ENCODE_DELIM = "-FELINE117-";

    private string _path = Path.Combine("Assets", "Profiles");

    // <K,V> = <Player number (1 or 2), Profile>
    private Dictionary<int, string> _activePlayerProfiles;

    // <K,V> = <Full Unique Profile Name, Profile>
    private Dictionary<string, Profile> _profiles;
    // <K,V> = <Full Unique Profile Name, Publicly Visible Profile Name>
    private Dictionary<string, string> _decodeProfileMap;

    public void ActivateProfile(string profileName, int playerNum) {
        string profile;
        if (!_activePlayerProfiles.TryGetValue(playerNum, out profile)) {
            // Add if its a new one
            _activePlayerProfiles.Add(playerNum, profileName);
        }else {
            // Otherwise overwrite the profile for this number
            _activePlayerProfiles[playerNum] = profileName;
        }
    }

    public string CreateProfile(string profile) {
        profile = profile.ToLower();

        if (_profiles.ContainsKey(profile)) {
            print("Profile map already contains entry for this profile");
            return profile;
        }else {
            // Create a new one
            var newProfile = new Profile { ProfileName = EncodeProfileNameFirstTime(profile) };
            print("New Profile: " + newProfile.ProfileName + " created!");
            newProfile.SaveProfile();
            StoreProfileInCache(newProfile);
        }
        return profile;
    }

    public List<string> GetAllProfiles() {
        return _profiles.Select(kvp => kvp.Key).ToList();
    }

    public List<string> GetPublicProfiles() {
        return _decodeProfileMap.Select(kvp => kvp.Key).ToList();
    }

    public Profile GetPlayerProfile(int playerNum) {
        string profile;
        if(!_activePlayerProfiles.TryGetValue(playerNum, out profile)) {
            throw new KeyNotFoundException("Player profile not found for number " + playerNum);
        }
        return GetProfile(profile);
    }

    public Profile GetProfile(string p) {
        Profile profile;
        if(!_profiles.TryGetValue(p, out profile)) {
            string profileFullName;
            var cleanedProfileName = p.ToLower();
            if(!_decodeProfileMap.TryGetValue(cleanedProfileName, out profileFullName)) {
                print("Profile not found: " + p);
                throw new KeyNotFoundException("Profile not found: " + p);
            }
            if(!_profiles.TryGetValue(profileFullName, out profile)) {
                print("Profile not found: " + p);
                throw new KeyNotFoundException("Profile not found: " + p);
            }
        }
        return profile;
    }

    public string PrettyPrint(string profileName) {
        var profile = GetProfile(profileName);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Weapons [" + string.Join(",", profile.GetUnlockedWeapons().Select(x => string.Format("{0}", x)).ToArray()) + "]");
        sb.AppendLine("Ultimates [" + string.Join(",", profile.GetUnlockedUltimates().Select(x => string.Format("{0}", x)).ToArray()) + "]");
        sb.AppendLine("Levels [" + string.Join(",", profile.GetUnlockedLevels().Select(x => string.Format("{0}", x)).ToArray()) + "]");
        sb.AppendLine("Emission Cache [" + profile.GetEmissionCache() + "]");
        sb.AppendLine("Selected Weapon [" + profile.GetSelectedWeapon() + "]");
        sb.AppendLine("Selected Ultimate [" + profile.GetSelectedUltimate() + "]");

        return sb.ToString();
    }

    void Awake() {
        if (Instance != null) {
            if (Instance != this) {
                Destroy(this.gameObject);
            }
        } else {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        _profiles = new Dictionary<string, Profile>();
        _decodeProfileMap = new Dictionary<string, string>();
        _activePlayerProfiles = new Dictionary<int, string>();
        LoadProfilesOnStart();
    }

    private string EncodeProfileNameFirstTime(string profileName) {
        StringBuilder sb = new StringBuilder();
        sb.Append(profileName.ToLower());
        sb.Append(ENCODE_DELIM);
        sb.Append(Guid.NewGuid());
        return sb.ToString();
    }

    private string DecodeProfileName(string profileName) {
        return profileName.Substring(0, profileName.IndexOf(ENCODE_DELIM));
    }

    private void StoreProfileInCache(Profile p) {
        _profiles[p.ProfileName] = p;
        _decodeProfileMap[DecodeProfileName(p.ProfileName)] = p.ProfileName;
    }

    private void LoadProfilesOnStart() {
        foreach(var file in Directory.GetFiles(_path)) {
            print("Full file path [" + file + "]");
            if (file.Contains(".meta")) {
                print("Skipping .meta file");
                continue;
            }
            var profileName = file.Substring(file.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            print("Processing file [" + profileName + "]");
            var profile = new Profile { ProfileName = profileName };
            profile.LoadProfileFromFile();
            StoreProfileInCache(profile);
        }
    }
}
