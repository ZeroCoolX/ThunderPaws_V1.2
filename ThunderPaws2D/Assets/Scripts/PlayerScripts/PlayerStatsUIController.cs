﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUIController : MonoBehaviour {
    /// <summary>
    /// Image indicating player health
    /// </summary>
    private Image _playerImage;
    /// <summary>
    /// Rectangle that indicates health level
    /// </summary>
    [SerializeField]
    private RectTransform _healthBarRect;
    /// <summary>
    /// Text within rectangle indicating health level
    /// </summary>
    [SerializeField]
    private RectTransform _ultimateBarRect;

    /// <summary>
    /// Throbbing Y button once the ultimate is ready
    /// </summary>
    private Transform _ultimateIndicator;
    /// <summary>
    /// The border around the ultimate bar
    /// </summary>
    private Animator _ultimateBarAnimator;

    private void Awake() {
        if (_healthBarRect == null) {
            Debug.LogError("No HealthBarRect found");
            throw new UnassignedReferenceException();
        }
        if (_ultimateBarRect == null) {
            Debug.LogError("No ultimateBarRect found");
            throw new UnassignedReferenceException();
        }
        _playerImage = transform.Find(GameConstants.ObjectName_PlayerImage).GetComponent<Image>();
        if (_playerImage == null) {
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
    }


    /// <summary>
    /// Update health references and visual indicator
    /// </summary>
    /// <param name="_cur"></param>
    /// <param name="_max"></param>
    public void SetHealthStatus(int cur, int max) {
        //Calculate percentage of max health
        float value = (float)cur / max;
        //TODO: Change color of bar over time
        _healthBarRect.localScale = new Vector3(value, _healthBarRect.localScale.y, _healthBarRect.localScale.z);
        CheckPlayerImage(cur);
    }

    private void CheckPlayerImage(int cur) {
        //Safety check in casae another class - Player - calls this before its had a chance to startup
        var healthKey = cur > 50 ? 100 : cur > 25 ? 50 : 25;
        _playerImage.sprite = GameMaster.Instance.GetSpriteFromMap(healthKey);
    }

    /// <summary>
    /// Update ultimate references and visual indicator
    /// </summary>
    /// <param name="_cur"></param>
    /// <param name="_max"></param>
    public void SetUltimateStatus(int cur, int max) {
        //Calculate percentage of max health
        float value = (float)cur / max;
        //TODO: Change color of bar over time
        _ultimateBarRect.localScale = new Vector3(value, _ultimateBarRect.localScale.y, _ultimateBarRect.localScale.z);
        _ultimateIndicator.gameObject.SetActive(cur >= max);
        _ultimateBarAnimator.SetBool("UltimateReady", cur >= max);
    }
}
