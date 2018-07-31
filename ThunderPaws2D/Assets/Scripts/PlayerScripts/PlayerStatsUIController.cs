using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class actually interacts with the UI by representing the PlayerStats on screen
/// </summary>
public class PlayerStatsUIController : MonoBehaviour {
    [SerializeField]
    private RectTransform _healthBarRect;
    [SerializeField]
    private RectTransform _ultimateBarRect;

    private Image _playerHealthImage;
    private Image _weaponImage;
    private Transform _ultimateIndicator;
    private Animator _ultimateBarAnimator;

    private Text _ammo;
    private Text _lives;

    private void Awake() {
        if (_healthBarRect == null) {
            Debug.LogError("No HealthBarRect found");
            throw new UnassignedReferenceException();
        }

        if (_ultimateBarRect == null) {
            Debug.LogError("No ultimateBarRect found");
            throw new UnassignedReferenceException();
        }

        _playerHealthImage = transform.Find(GameConstants.ObjectName_PlayerImage).GetComponent<Image>();
        if (_playerHealthImage == null) {
            Debug.LogError("No playerImage found");
            throw new UnassignedReferenceException();
        }

        _ultimateIndicator = transform.Find(GameConstants.ObjectName_UltimateIndicator);
        if(_ultimateIndicator == null) {
            throw new MissingComponentException("Missing an ultimate indicator");
        }

        _ultimateBarAnimator = transform.Find(GameConstants.ObjectName_BarContainer).Find(GameConstants.ObjectName_UltimateBar).GetComponent<Animator>();
        if (_ultimateBarAnimator == null) {
            Debug.LogError("No ultimateBarAnimator found");
            throw new UnassignedReferenceException();
        }

        _ammo = transform.Find(GameConstants.ObjectName_AmmoText).GetComponent<Text>();
        if (_ammo == null) {
            Debug.LogError("No AmmoText found");
            throw new UnassignedReferenceException();
        }

        _lives = transform.Find(GameConstants.ObjectName_LivesText).GetComponent<Text>();
        if (_lives == null) {
            Debug.LogError("No LivesText found");
            throw new UnassignedReferenceException();
        }

        _weaponImage = transform.Find(GameConstants.ObjectName_WeaponImage).GetComponent<Image>();
        if (_weaponImage == null) {
            Debug.LogError("No WeaponImage found");
            throw new UnassignedReferenceException();
        }
    }

    public void SetWeaponPickup(string weaponName) {
        _weaponImage.sprite = GameMasterV2.Instance.GetWeaponSpriteFromMap(weaponName.ToLower());
    }

    public void SetAmmo(int ammo = -1) {
        _ammo.text = " Ammo: " + (ammo > -1 ? ammo + "" : " infinity");
    }

    public void SetLives(int lives) {
        if (_lives == null) {
            _lives = transform.Find(GameConstants.ObjectName_LivesText).GetComponent<Text>();
            if (_lives == null) {
                Debug.LogError("No LivesText found what the FUCK");
                throw new UnassignedReferenceException();
            }
        }
        _lives.text = " Lives: " + lives;
    }

    public void SetHealthStatus(int currentHealth, int maxHealth) {
        float healthVal;
        if (currentHealth == 0 && maxHealth == 0) {
            healthVal = 1f;
        } else {
            // Calculate percentage of max health
            healthVal = (float)currentHealth / maxHealth;
        }
        _healthBarRect.localScale = new Vector3(healthVal, _healthBarRect.localScale.y, _healthBarRect.localScale.z);

        CheckPlayerImage(currentHealth, maxHealth);
    }

    public void SetUltimateStatus(int cur, int max) {
        // Calculate percentage of max health
        float value = (float)cur / max;
        _ultimateBarRect.localScale = new Vector3(value, _ultimateBarRect.localScale.y, _ultimateBarRect.localScale.z);
        _ultimateIndicator.gameObject.SetActive(cur >= max);
        _ultimateBarAnimator.SetBool("UltimateReady", cur >= max);
    }

    /// <summary>
    /// Allows dynamic calculation of the players health percentage independent of what int values are given
    /// </summary>
    private void CheckPlayerImage(int cur, int max) {
        // Safety check in case another class - Player - calls this before its had a chance to startup
        var diff = max-cur;// 100-100=0, 100-40 = 60
        var maxHalf = max / 2;// 50%
        var maxFourth = max / 4; // 25%
        var healthKey = cur <= maxFourth ? 25 : cur > maxFourth & cur <= maxHalf ? 50 : 100;
        _playerHealthImage.sprite = GameMasterV2.Instance.GetSpriteFromMap(healthKey);
    }
}
