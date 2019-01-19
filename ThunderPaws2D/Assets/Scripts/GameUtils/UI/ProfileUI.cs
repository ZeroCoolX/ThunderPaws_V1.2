using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour {
    public Transform SelectedProfileMetaData;
    public Transform SelectedProfile;

    public Transform[] AvailableProfiles;

    public Transform CreateProfileText;
    private const int MAX_PROFILE_LENGTH = 25;
    private string _profileText = "";

    private void Start() {
    }

    public void OnAlphaSelect(string alpha) {
        if(_profileText.Length == MAX_PROFILE_LENGTH) {
            AudioManager.Instance.PlaySound(GameConstants.Audio_AccessDenied);
            return;
        }
        _profileText += alpha;
        UpdateProfileText();
    }

    public void OnDeleteSelect() {
        if (_profileText.Length <= 1) {
            if (!string.IsNullOrEmpty(_profileText)) {
                _profileText = "";
            }
        }else {
            _profileText = _profileText.Substring(0, _profileText.Length - 1);
        }
        UpdateProfileText();
    }

    private void UpdateProfileText() {
        CreateProfileText.GetComponent<TextMeshProUGUI>().text = _profileText;
    }

    public void OnEnterSelect(int playerNum) {
        CreateNewProfile(playerNum);
    }

    public void CreateNewProfile(int playerNum) {
        var createdProfile = ProfilePool.Instance.CreateProfile(_profileText);
        ProfilePool.Instance.ActivateProfile(createdProfile, playerNum);
        SetOnScreenProfile(createdProfile);
    }

    private void SetOnScreenProfile(string profile) {
        SelectedProfile.GetComponent<TextMeshProUGUI>().text = profile;
        SelectedProfileMetaData.GetComponent<TextMeshProUGUI>().text = BuildPrettyPrint(profile);
    }

    public void RefreshProfilesList() {
        if(AvailableProfiles == null) {
            return;
        }
        var profiles = ProfilePool.Instance.GetPublicProfiles();
        for (var i = 0; i < profiles.Count(); ++i) {
            AvailableProfiles[i].GetComponent<TextMeshProUGUI>().text = profiles[i];
        }
    }


    public Transform ProfileConfirmationScreen;
    public Transform ProfileLoadScreen;
    public void SetPlayerProfile(int profileIndex) {
        if (IsEmptyProfile(AvailableProfiles[profileIndex].GetComponent<TextMeshProUGUI>().text)) {
            AudioManager.Instance.PlaySound(GameConstants.Audio_AccessDenied);
            return;
        }

        var profile = AvailableProfiles[profileIndex].GetComponent<TextMeshProUGUI>().text;
        ProfilePool.Instance.ActivateProfile(profile, 1);
        print("Profile : " + profile + " has been activated");

        SetOnScreenProfile(profile);

        // Have to do this manually in case the user selects one that doesn't exist
        ProfileConfirmationScreen.gameObject.SetActive(true);
        ProfileLoadScreen.gameObject.SetActive(false);
    }

    private bool IsEmptyProfile(string profile) {
        return "Empty...".Equals(profile);
    }

    private string BuildPrettyPrint(string profileName) {
        var profile = ProfilePool.Instance.GetProfile(profileName);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Weapons [" + string.Join(",", profile.GetUnlockedWeapons().Select(x => string.Format("{0}", x)).ToArray()) + "]");
        sb.AppendLine("Ultimates [" + string.Join(",", profile.GetUnlockedUltimates().Select(x => string.Format("{0}", x)).ToArray()) + "]");
        sb.AppendLine("Levels [" + string.Join(",", profile.GetUnlockedLevels().Select(x => string.Format("{0}", x)).ToArray()) + "]");
        sb.AppendLine("Emission Cache [" + profile.GetEmissionCache() + "]");
        sb.AppendLine("Selected Weapon [" + profile.GetSelectedWeapon() + "]");
        sb.AppendLine("Selected Ultimate [" + profile.GetSelectedUltimate() + "]");

        return sb.ToString();
    }
}
