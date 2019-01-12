using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class ProfilePool : MonoBehaviour{

    public static ProfilePool Instance;

    private const string ENCODE_DELIM = "-SIERRA117-";
    public static string EncodeProfileNameFirstTime(string profileName) {
        StringBuilder sb = new StringBuilder();
        sb.Append(profileName.ToLower());
        sb.Append(ENCODE_DELIM);
        sb.Append(Guid.NewGuid());
        return sb.ToString();
    }

    public static string DecodeProfileName(string profileName) {
        return profileName.Substring(0, profileName.IndexOf(ENCODE_DELIM));
    }

    private string _path = Path.Combine("Assets", "Profiles");

    private List<string> _activeProfiles;
    // <K,V> = <Full Unique Profile Name, Profile>
    private Dictionary<string, Profile> _profiles;
    // <K,V> = <Full Unique Profile Name, Publicly Visible Profile Name>
    private Dictionary<string, string> _decodeProfileMap;

    public void CreateProfile(string profile) {
        if (_profiles.ContainsKey(profile)) {
            print("Profile map already contains entry for this profile");
            return;
        }else {
            // Create a new one
            var newProfile = new Profile { ProfileName = EncodeProfileNameFirstTime(profile) };
            print("New Profile: " + newProfile.ProfileName + " created!");
            newProfile.SaveProfile();
            StoreProfileInCache(newProfile);
        }
    }

    public List<string> GetAllProfiles() {
        return _profiles.Select(kvp => kvp.Key).ToList();
    }

    public List<string> GetPublicProfiles() {
        return _decodeProfileMap.Select(kvp => kvp.Key).ToList();
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
        LoadProfilesOnStart();
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
