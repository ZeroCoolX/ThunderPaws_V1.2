using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProfilePool : MonoBehaviour{

    public static ProfilePool Instance;

    private string path = @"Assets/Profiles/";

    private Dictionary<string, Profile> _profiles;

    public void CreateProfile(string profile) {
        if (_profiles.ContainsKey(profile)) {
            print("Profile map already contains entry for this profile");
            return;
        }else {
            var queriedProfile = new Profile { ProfileName = profile };

            // try loading from file
            if (File.Exists(path + profile)) {
                print("Path [" + path + profile + "] exists! Loading from file");
                queriedProfile.LoadProfileFromFile();
            }else {
                // Create a new one
                print("New Profile: " + profile + " created!");
                queriedProfile.SaveProfile();
            }
            _profiles[profile] = queriedProfile;
        }
    }

    public Profile GetProfile(string p) {
        Profile profile;
        if(!_profiles.TryGetValue(p, out profile)) {
            print("Profile not found: " + p);
            throw new KeyNotFoundException("Profile not found: " + p);
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
    }
}
