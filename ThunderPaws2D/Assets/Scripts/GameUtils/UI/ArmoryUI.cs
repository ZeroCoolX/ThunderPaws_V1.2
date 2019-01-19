using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmoryUI : MonoBehaviour {
    public Button SelectedUltimate;
    public Button SelectedWeapon;

    public Sprite UnselectedWeaponSprite;
    public Sprite UnselectedUltimateSprite;

    public Transform CallerOriginScreen;
    public int PlayerNumber = 1;

    public Sprite[] Sprites;
    private Dictionary<string, Sprite> SpriteAccessMap;

    private const string LOCK_SUFFIX = "_lock";
    private List<Transform> _weaponButtons;
    private List<Transform> _ultimateButtons;

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

        DisplayWeaponsByLockStatus(profile);
        DisplayUltimatesByLockStatus(profile);

        var selectedWeapon = profile.GetSelectedWeapon();
        LoadWeaponSelection(selectedWeapon);

        var selectedUltimate = profile.GetSelectedUltimate();
        LoadUltimateSelection(selectedUltimate);
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
                SelectedWeapon.GetComponent<Image>().sprite = GetSpriteFromMap(selectedWeapon);//GameObject.Find(selectedWeapon).GetComponent<Image>().sprite;
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
