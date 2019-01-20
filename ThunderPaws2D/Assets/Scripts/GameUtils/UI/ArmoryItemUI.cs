using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArmoryItemUI : MonoBehaviour {
    public GameObject[] UnlockPossible;
    public GameObject[] UnlockImpossible;
    public Transform UnlockEventSystem;
    public Image SelectionImage;
    public Transform SelectionPrice;
    public Transform Armory;

    private Button _currentSelection;
    public void SelectItem(Button selection, int emissionCache) {
        _currentSelection = selection;

        UnlockEventSystem.gameObject.SetActive(true);

        var price = selection.GetComponent<ArmoryItem>().Price;

        SelectionImage.sprite = selection.GetComponent<Image>().sprite;
        SelectionPrice.GetComponent<TextMeshProUGUI>().text = price + "EMS";

        var unlockable = price <= emissionCache;
        foreach (var obj in UnlockPossible) {
            obj.SetActive(unlockable);
        }
        foreach (var obj in UnlockImpossible) {
            obj.SetActive(!unlockable);
        }
        UnlockEventSystem.GetComponent<EventSystem>().SetSelectedGameObject(unlockable ? UnlockPossible[0] : UnlockImpossible[0]);
    }

    public void ConfirmWeapon() {
        Armory.GetComponent<ArmoryUI>().WeaponItemSelected(_currentSelection);
    }

    public void ConfirmUltimate() {
        Armory.GetComponent<ArmoryUI>().UltimateItemSelected (_currentSelection);
    }

    public void DeclineWeapon() {
        Armory.GetComponent<ArmoryUI>().GoBackToWeaponSelection(_currentSelection);
    }

    public void DeclineUltimate() {
        Armory.GetComponent<ArmoryUI>().GoBackToUltimateSelection(_currentSelection);
    }
}
