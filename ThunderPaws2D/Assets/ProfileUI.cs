using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour {
    public InputField Profile;
    public InputField Level;

    public Text P1PlayerProfileSelection;
    public Text P2PlayerProfileSelection;

    public Dropdown AvailableProfiles;

    private void Start() {
        RefreshProfilesDropdown();
    }

    private void RefreshProfilesDropdown() {
        AvailableProfiles.ClearOptions();
        //Add the options created in the List above
        AvailableProfiles.AddOptions(ProfilePool.Instance.GetPublicProfiles());
    }

    public void SetPlayerProfile(int playerNum) {
        var profileSelected = AvailableProfiles.value;
        var profileName = AvailableProfiles.options[profileSelected].text;
        var text = BuildPrettyPrint(profileName);
        if (playerNum == 1) {
            P1PlayerProfileSelection.text = text;
        } else {
            P2PlayerProfileSelection.text = text;
        }
    }

    public void OnDropdownOptionSelected() {
        var profileSelected = AvailableProfiles.value;
        var profileName = AvailableProfiles.options[profileSelected].text;
        print("Selected Profile: " + profileName);
    }

    public void SubmitProfile() {
        ProfilePool.Instance.CreateProfile(Profile.text);
        PrintProfile();
        RefreshProfilesDropdown();
    }

    public void PrintProfile() {
        var profile = ProfilePool.Instance.GetProfile(Profile.text);
        print("Profile [" + ProfilePool.DecodeProfileName(profile.ProfileName) + "]");

        print("Weapons [" + string.Join(",", profile.GetUnlockedWeapons().Select(x => string.Format("{0}", x)).ToArray()) + "]");
        print("Ultimates [" + string.Join(",", profile.GetUnlockedUltimates().Select(x => string.Format("{0}", x)).ToArray()) + "]");
        print("Levels [" + string.Join(",", profile.GetUnlockedLevels().Select(x => string.Format("{0}", x)).ToArray()) + "]");
        print("Emission Cache [" + profile.GetEmissionCache() + "]");
    }

    private string BuildPrettyPrint(string profileName) {
        var profile = ProfilePool.Instance.GetProfile(profileName);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Profile [" + ProfilePool.DecodeProfileName(profile.ProfileName) + "]");
        sb.AppendLine("Weapons [" + string.Join(",", profile.GetUnlockedWeapons().Select(x => string.Format("{0}", x)).ToArray()) + "]");
        sb.AppendLine("Ultimates [" + string.Join(",", profile.GetUnlockedUltimates().Select(x => string.Format("{0}", x)).ToArray()) + "]");
        sb.AppendLine("Levels [" + string.Join(",", profile.GetUnlockedLevels().Select(x => string.Format("{0}", x)).ToArray()) + "]");
        sb.AppendLine("Emission Cache [" + profile.GetEmissionCache() + "]");

        return sb.ToString();
    }

    public void UnlockLevel() {
        var profile = ProfilePool.Instance.GetProfile(Profile.text);
        profile.UnlockLevel(Level.text);
        PrintProfile();
        RefreshProfilesDropdown();
    }
}
