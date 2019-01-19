using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour {

    private Transform _currentWeapon;
    private List<Transform> _ownedWeapons = new List<Transform>();
    private Transform _weaponAnchorPoint;
    private Animator _weaponAnchorAnimator;
    private int _playerNumber;
    private const string DEFAULT_WEAPON_NAME = "fuzzbuster";



    public void InitializeWeapon(int playerNumber, Transform anchorPoint) {
        _playerNumber = playerNumber;

        _weaponAnchorPoint = anchorPoint;

        _weaponAnchorAnimator = _weaponAnchorPoint.GetComponent<Animator>();
        if (_weaponAnchorAnimator == null) {
            throw new MissingComponentException("The weapon anchor is missing an animator.");
        }

        // Always create the default
        CreateAndEquipWeapon(GameConstants.ObjectName_DefaultWeapon);
        if (_currentWeapon == null) {
            throw new MissingComponentException("There was no weapon attached to the Player");
        }

        // Create secondary if its selected
        if (!ProfilePool.Instance.Debug) {
            CreateSecondaryWeapon();
        }
    }

    private void CreateSecondaryWeapon() {
        var playerProfile = ProfilePool.Instance.GetPlayerProfile(_playerNumber);

        var selectedWeapon = playerProfile.GetSelectedWeapon();
        if (!string.IsNullOrEmpty(selectedWeapon)) {
            CreateAndEquipWeapon(selectedWeapon);
        }
    }

    public void SetWeaponRotation(Quaternion rotation) {
        _weaponAnchorPoint.rotation = rotation;
    }

    public void AnimateWeapon(string animation, bool active) {
        _weaponAnchorAnimator.SetBool(animation, active);
    }

    public void ToggleUltimateForAllWeapons(bool ultstatus) {
        if (ultstatus) {
            _currentWeapon.GetComponent<AbstractWeapon>().FillAmmoFromUlt();
        }
        foreach (var weapon in _ownedWeapons) {
            weapon.GetComponent<AbstractWeapon>().UltMode = ultstatus;
        }
    }

    public void ToggleWeaponActiveStatus(bool status) {
        _currentWeapon.gameObject.SetActive(status);
    }

    public void RemoveOtherWeapon(Transform weapon) {
        _ownedWeapons.Remove(weapon);
        Destroy(_currentWeapon.gameObject);

        _currentWeapon = _ownedWeapons[0];
        _currentWeapon.position = _weaponAnchorPoint.position;
        ToggleWeaponActiveStatus(true);

        PlayerHudManager.Instance.UpdateWeaponPickup(_playerNumber, DEFAULT_WEAPON_NAME);
        if (_currentWeapon == null) {
            throw new KeyNotFoundException("ERROR: Default weapon was not found in weapon map");
        }
    }

    public void SwitchWeapon() {
        if (_ownedWeapons.Count > 1) {
            var rotation = _currentWeapon.rotation;
            ToggleWeaponActiveStatus(false);
            // _currentWeapon is either 1 or 2, so to map to the correct index in the array subtract 1 : 0 is default, 1 is special weapon
            var index = _ownedWeapons.IndexOf(_currentWeapon);
            _currentWeapon = _ownedWeapons[Mathf.Abs(-1 + index)];
            ToggleWeaponActiveStatus(true);
            PlayerHudManager.Instance.GetPlayerHud(_playerNumber).SetAmmo(_currentWeapon.GetComponent<AbstractWeapon>().Ammo);
            PlayWeaponSoundEffect(GameConstants.Audio_WeaponSwitch);
            // TODO: The name of the object could have (clone) on it so...its not very elegant right now
            PlayerHudManager.Instance.UpdateWeaponPickup(_playerNumber, _currentWeapon.gameObject.name.ToLower().Substring(0, _currentWeapon.gameObject.name.IndexOf("(")));
        }
    }

    public void CreateAndEquipWeapon(string weaponKey) {
        CreateWeapon(weaponKey);
        StoreOrOverwriteNewWeapon();
        UpdatePlayerHudForWeapon(weaponKey);
        PlayWeaponSoundEffect(GameConstants.Audio_WeaponPickup);
    }

    private void CreateWeapon(string weaponKey) {
        if (weaponKey != GameConstants.ObjectName_DefaultWeapon) {
            ToggleWeaponActiveStatus(false);
        }
        _currentWeapon = Instantiate(GameMasterV2.Instance.GetWeaponFromMap(weaponKey.ToLower()), _weaponAnchorPoint.position, _weaponAnchorPoint.rotation, _weaponAnchorPoint);
        ToggleWeaponActiveStatus(true);
        print("Created weapon: " + _currentWeapon.gameObject.name);
    }

    private void StoreOrOverwriteNewWeapon() {
        if (_ownedWeapons.Count == 2) {
            var previousWeapon = _ownedWeapons[1];
            Destroy(previousWeapon.gameObject);
            _ownedWeapons[1] = _currentWeapon;
        } else {
            _ownedWeapons.Add(_currentWeapon);
        }
    }

    private void UpdatePlayerHudForWeapon(string weaponKey) {
        PlayerHudManager.Instance.UpdateWeaponPickup(_playerNumber, weaponKey);
        PlayerHudManager.Instance.GetPlayerHud(_playerNumber).SetAmmo(_currentWeapon.GetComponent<AbstractWeapon>().Ammo);
    }

    private void PlayWeaponSoundEffect(string soundEffect) {
        try {
            AudioManager.Instance.PlaySound(soundEffect);
        } catch (System.Exception e) {
            print("Either the game master or the audiomanager doesn't exist yet");
        }
    }
}
