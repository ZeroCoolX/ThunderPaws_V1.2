using System;
using UnityEngine;
using UnityEngine.UI;

public class ArmoryUI : MonoBehaviour {
    public Button SelectedUltimate;
    public Button SelectedWeapon;
    public Sprite UnselectedWeaponSprite;
    public Sprite UnselectedUltimateSprite;

    public Transform CallerOriginScreen;
    public int PlayerNumber = 1;

    public void SelectUltimate(Button selection) {
        SelectedUltimate.GetComponent<Image>().sprite = selection.GetComponent<Image>().sprite;
        var profile = ProfilePool.Instance.GetPlayerProfile(PlayerNumber);
        profile.SetSelectedUltimate(selection.gameObject.name);
    } 

    public void SelectWeapon(Button selection) {
        SelectedWeapon.GetComponent<Image>().sprite = selection.GetComponent<Image>().sprite;
        var profile = ProfilePool.Instance.GetPlayerProfile(PlayerNumber);
        profile.SetSelectedWeapon(selection.gameObject.name);
    }

    public void LoadProfileSelection() {
        var profile = ProfilePool.Instance.GetPlayerProfile(PlayerNumber);

        var selectedWeapon = profile.GetSelectedWeapon();
        LoadWeaponSelection(selectedWeapon);

        var selectedUltimate = profile.GetSelectedUltimate();
        LoadUltimateSelection(selectedUltimate);
    }

    private void LoadWeaponSelection(string selectedWeapon) {
        if (!string.IsNullOrEmpty(selectedWeapon)) {
            try {
                SelectedWeapon.GetComponent<Image>().sprite = GameObject.Find(selectedWeapon).GetComponent<Image>().sprite;
            } catch (Exception e) {
                print("Failed to set selected weapon on startup for weapon [" + selectedWeapon + "]");
            }
        } else {
            SelectedWeapon.GetComponent<Image>().sprite = UnselectedWeaponSprite;
        }
    }

    private void LoadUltimateSelection(string selectedUltimate) {
        if (!string.IsNullOrEmpty(selectedUltimate)) {
            try {
                SelectedUltimate.GetComponent<Image>().sprite = GameObject.Find(selectedUltimate).GetComponent<Image>().sprite;
            } catch (Exception e) {
                print("Failed to set selected ultimate on startup for ultimate [" + selectedUltimate + "]");
            }
        } else {
            SelectedUltimate.GetComponent<Image>().sprite = UnselectedUltimateSprite;
        }
    }

    public void SetOrigin(Transform origin) {
        CallerOriginScreen = origin;
    }

    public void GoBackToCaller() {
        CallerOriginScreen.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
