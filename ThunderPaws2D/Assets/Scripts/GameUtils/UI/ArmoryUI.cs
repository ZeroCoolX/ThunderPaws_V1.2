using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArmoryUI : MonoBehaviour {
    public Button SelectedUltimate;
    public Button SelectedWeapon;
    public Transform EmissionCacheDisplay;

    public Transform WeaponUnlockScreen;
    public Transform UltimateUnlockScreen;

    public Sprite UnselectedWeaponSprite;
    public Sprite UnselectedUltimateSprite;

    public Transform CallerOriginScreen;
    public int PlayerNumber = 1;

    public Sprite[] Sprites;
    private Dictionary<string, Sprite> SpriteAccessMap;

    private GameObject _eventSystem;

    private const string LOCK_SUFFIX = "_lock";
    private const string EMISSION_SUFFIX = "EMS";

    private List<Transform> _weaponButtons;
    private GameObject _weaponSelectorEventSystem;
    private List<Transform> _ultimateButtons;
    private GameObject _ultimateSelectorEventSystem;

    // Indicates we have successfully unlocked this item, or it was previously unlocked so just select it
    public void WeaponItemSelected(Button selection) {
        WeaponUnlockScreen.gameObject.SetActive(false);

        foreach (var btn in _weaponButtons) {
            btn.GetComponent<Button>().enabled = false;
        }
        _weaponSelectorEventSystem.SetActive(false);

        SelectedWeapon.enabled = true;
        SelectedUltimate.enabled = true;

        var selectionName = selection.gameObject.name;

        var profile = ProfilePool.Instance.GetPlayerProfile(PlayerNumber);
        var sprite = selection.GetComponent<Image>().sprite;
        if (!profile.IsWeaponUnlocked(selectionName)) {
            sprite = GetSpriteFromMap(sprite.name.Substring(0, sprite.name.IndexOf(LOCK_SUFFIX)));
            profile.UnlockWeapon(selectionName);
            UpdateEmissionCache(selection.GetComponent<ArmoryItem>().Price);
        }
        profile.SetSelectedWeapon(selectionName);

        SelectedWeapon.GetComponent<Image>().sprite = sprite;

        _eventSystem.SetActive(true);
        _eventSystem.GetComponent<EventSystem>().SetSelectedGameObject(SelectedWeapon.gameObject);

        // refresh the sprites
        DisplayWeaponsByLockStatus(profile);
    }

    public void UltimateItemSelected(Button selection) {
        UltimateUnlockScreen.gameObject.SetActive(false);

        foreach (var btn in _ultimateButtons) {
            btn.GetComponent<Button>().enabled = false;
        }
        _ultimateSelectorEventSystem.SetActive(false);

        SelectedWeapon.enabled = true;
        SelectedUltimate.enabled = true;

        var selectionName = selection.gameObject.name;

        var profile = ProfilePool.Instance.GetPlayerProfile(PlayerNumber);
        var sprite = selection.GetComponent<Image>().sprite;
        if (!profile.IsUltimateUnlocked(selectionName)) {
            sprite = GetSpriteFromMap(sprite.name.Substring(0, sprite.name.IndexOf(LOCK_SUFFIX)));
            profile.UnlockUltimate(selectionName);
            UpdateEmissionCache(selection.GetComponent<ArmoryItem>().Price);
        }
        profile.SetSelectedUltimate(selectionName);

        SelectedUltimate.GetComponent<Image>().sprite = sprite;

        _eventSystem.SetActive(true);
        _eventSystem.GetComponent<EventSystem>().SetSelectedGameObject(SelectedUltimate.gameObject);

        // refresh the sprites
        DisplayUltimatesByLockStatus(profile);
    }

    // Either the user backed out of the unlock, or they didn't have enough funds
    public void GoBackToWeaponSelection(Button selection) {
        WeaponUnlockScreen.gameObject.SetActive(false);

        ToggleSelectionScreenActivation(true, _weaponButtons, _weaponSelectorEventSystem);
        _weaponSelectorEventSystem.GetComponent<EventSystem>().SetSelectedGameObject(selection.gameObject);
    }

    public void GoBackToUltimateSelection(Button selection) {
        UltimateUnlockScreen.gameObject.SetActive(false);

        ToggleSelectionScreenActivation(true, _ultimateButtons, _ultimateSelectorEventSystem);
        _ultimateSelectorEventSystem.GetComponent<EventSystem>().SetSelectedGameObject(selection.gameObject);
    }

    public void SelectUltimate(Button selection) {
        var profile = ProfilePool.Instance.GetPlayerProfile(PlayerNumber);

        if (_ultimateSelectorEventSystem == null) {
            _ultimateSelectorEventSystem = FindEventSystem("UltimateSelectorEventSystem");
        }

        if (profile.IsUltimateUnlocked(selection.gameObject.name)) {
            UltimateItemSelected(selection);
        } else {
            print("Ultimate is not unlocked yet, try unlocking it");
            TryToUnlockUltimate(selection);
        }
    } 

    public void SelectWeapon(Button selection) {
        var profile = ProfilePool.Instance.GetPlayerProfile(PlayerNumber);

        if(_weaponSelectorEventSystem == null) {
            _weaponSelectorEventSystem = FindEventSystem("WeaponSelectorEventSystem");
        }

        if (profile.IsWeaponUnlocked(selection.gameObject.name)) {
            WeaponItemSelected(selection);
        }else {
            print("Weapon is not unlocked yet, try unlocking it");
            TryToUnlockWeapon(selection);
        }
    }

    private GameObject FindEventSystem(string es) {
        var eventSystem = GameObject.Find(es);
        if (eventSystem == null) {
            throw new MissingComponentException("Missing " + es);
        }
        return eventSystem;
    }

    private void TryToUnlockWeapon(Button selection) {
        // Show the unlock screen
        WeaponUnlockScreen.gameObject.SetActive(true);
        // Shut off selection events and turn on Unlock screen events
        ToggleSelectionScreenActivation(false, _weaponButtons, _weaponSelectorEventSystem);

        var profile = ProfilePool.Instance.GetPlayerProfile(PlayerNumber);
        WeaponUnlockScreen.GetComponent<ArmoryItemUI>().SelectItem(selection, profile.GetEmissionCache());
    }

    private void TryToUnlockUltimate(Button selection) {
        // Show the unlock screen
        UltimateUnlockScreen.gameObject.SetActive(true);
        // Shut off selection events and turn on Unlock screen events
        ToggleSelectionScreenActivation(false, _ultimateButtons, _ultimateSelectorEventSystem);

        var profile = ProfilePool.Instance.GetPlayerProfile(PlayerNumber);
        UltimateUnlockScreen.GetComponent<ArmoryItemUI>().SelectItem(selection, profile.GetEmissionCache());
    }

    public void LoadProfileSelection() {
        var profile = ProfilePool.Instance.GetPlayerProfile(PlayerNumber);

        DisplayWeaponsByLockStatus(profile);
        DisplayUltimatesByLockStatus(profile);

        var selectedWeapon = profile.GetSelectedWeapon();
        LoadWeaponSelection(selectedWeapon);

        var selectedUltimate = profile.GetSelectedUltimate();
        LoadUltimateSelection(selectedUltimate);

        EmissionCacheDisplay.GetComponent<TextMeshProUGUI>().text = "[" + profile.GetEmissionCache() + EMISSION_SUFFIX + "]";
    }

    public void UpdateEmissionCache(int cost) {
        var profile = ProfilePool.Instance.GetPlayerProfile(PlayerNumber);
        profile.UpdateEmissionCache(cost * -1);
        EmissionCacheDisplay.GetComponent<TextMeshProUGUI>().text = "[" + profile.GetEmissionCache() + EMISSION_SUFFIX + "]";
    }

    public void SetOrigin(Transform origin) {
        CallerOriginScreen = origin;
    }

    public void GoBackToCaller() {
        CallerOriginScreen.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    private void ToggleSelectionScreenActivation(bool active, List<Transform> buttonItems, GameObject eventSystem) {
        foreach (var btn in buttonItems) {
            btn.GetComponent<Button>().enabled = active;
        }
        eventSystem.SetActive(active);
    }

    private void DisplayWeaponsByLockStatus(Profile profile) {
        foreach(var button in _weaponButtons) {
            var spriteName = button.gameObject.name;
            
            if (!profile.IsWeaponUnlocked(spriteName)) {
                spriteName += LOCK_SUFFIX;
            }
            button.GetComponent<Image>().sprite = GetSpriteFromMap(spriteName);
        }
    }

    private void DisplayUltimatesByLockStatus(Profile profile) {
        foreach (var button in _ultimateButtons) {
            var spriteName = button.gameObject.name;

            if (!profile.IsUltimateUnlocked(spriteName)) {
                spriteName += LOCK_SUFFIX;
            }
            button.GetComponent<Image>().sprite = GetSpriteFromMap(spriteName);
        }
    }

    private void LoadWeaponSelection(string selectedWeapon) {
        if (!string.IsNullOrEmpty(selectedWeapon)) {
            try {
                SelectedWeapon.GetComponent<Image>().sprite = GetSpriteFromMap(selectedWeapon);
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
                SelectedUltimate.GetComponent<Image>().sprite = GetSpriteFromMap(selectedUltimate);//GameObject.Find(selectedUltimate).GetComponent<Image>().sprite;
            } catch (Exception e) {
                print("Failed to set selected ultimate on startup for ultimate [" + selectedUltimate + "]");
            }
        } else {
            SelectedUltimate.GetComponent<Image>().sprite = UnselectedUltimateSprite;
        }
    }

    private Sprite GetSpriteFromMap(string key) {
        Sprite sprite;
        if(!SpriteAccessMap.TryGetValue(key, out sprite)) {
            print("GetSpriteFromMap was unable to locate the following sprite in the armory access map [" + key + "]");
            sprite = UnselectedWeaponSprite;
        }
        return sprite;
    }

    private void Awake() {
        SpriteAccessMap = new Dictionary<string, Sprite>();

        foreach (var sprite in Sprites) {
            SpriteAccessMap.Add(sprite.name, sprite);
        }

        _eventSystem = GameObject.Find("ArmoryEventSystem");
        if(_eventSystem == null) {
            throw new MissingComponentException("Missing ArmoryEventSystem");
        }

        _weaponButtons = new List<Transform>();
        _weaponButtons.Add(GameObject.Find(GameConstants.ObjectName_ShotgunWeapon).transform);
        _weaponButtons.Add(GameObject.Find(GameConstants.ObjectName_GaussWeapon).transform);
        _weaponButtons.Add(GameObject.Find(GameConstants.ObjectName_EmissionIndexWeapon).transform);
        _weaponButtons.Add(GameObject.Find(GameConstants.ObjectName_FatCatWeapon).transform);

        _ultimateButtons = new List<Transform>();
        _ultimateButtons.Add(GameObject.Find(GameConstants.ObjectName_TriggerPaw).transform);
        _ultimateButtons.Add(GameObject.Find(GameConstants.ObjectName_LighteningClaw).transform);
        _ultimateButtons.Add(GameObject.Find(GameConstants.ObjectName_ThunderPounce).transform);
        _ultimateButtons.Add(GameObject.Find(GameConstants.ObjectName_ReflectFurball).transform);
    }
}
