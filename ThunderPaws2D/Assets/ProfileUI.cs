using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour {
    public InputField Profile;
    public InputField Level;

    public void SubmitProfile() {
        ProfilePool.Instance.CreateProfile(Profile.text);
        PrintProfile();
    }

    public void PrintProfile() {
        var profile = ProfilePool.Instance.GetProfile(Profile.text);
        print("Profile [" + profile.ProfileName + "]");

        print("Weapons [" + string.Join(",", profile.GetUnlockedWeapons().Select(x => string.Format("{0}", x)).ToArray()) + "]");
        print("Ultimates [" + string.Join(",", profile.GetUnlockedUltimates().Select(x => string.Format("{0}", x)).ToArray()) + "]");
        print("Levels [" + string.Join(",", profile.GetUnlockedLevels().Select(x => string.Format("{0}", x)).ToArray()) + "]");
        print("Emission Cache [" + profile.GetEmissionCache() + "]");
    }

    public void UnlockLevel() {
        var profile = ProfilePool.Instance.GetProfile(Profile.text);
        profile.UnlockLevel(Level.text);
        PrintProfile();
    }
}
