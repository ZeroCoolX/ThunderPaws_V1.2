using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmoryUI : MonoBehaviour {
    public Button SelectedUltimate;
    public Button SelectedWeapon;

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

    public void SetOrigin(Transform origin) {
        CallerOriginScreen = origin;
    }

    public void GoBackToCaller() {
        CallerOriginScreen.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
