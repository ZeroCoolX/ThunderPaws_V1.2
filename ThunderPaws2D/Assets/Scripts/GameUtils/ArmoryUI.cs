using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmoryUI : MonoBehaviour {
    public Button SelectedUltimate;
    public Button SelectedWeapon;

    public Transform CallerOriginScreen;

    public void SelectUltimate(Button selection) {
        SelectedUltimate.GetComponent<Image>().sprite = selection.GetComponent<Image>().sprite;
    } 

    public void SelectWeapon(Button selection) {
        SelectedWeapon.GetComponent<Image>().sprite = selection.GetComponent<Image>().sprite;
    }

    public void SetOrigin(Transform origin) {
        CallerOriginScreen = origin;
    }

    public void GoBackToCaller() {
        CallerOriginScreen.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
