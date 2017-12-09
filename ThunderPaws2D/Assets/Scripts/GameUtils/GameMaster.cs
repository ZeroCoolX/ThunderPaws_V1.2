using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Overseer of the game.
/// Handles static mappings needed throughout and persists data which needs to be carried over from level to level/ life to life...etc
/// </summary>
public class GameMaster : MonoBehaviour {
    /// <summary>
    /// Static reference for anyone who needs it
    /// </summary>
    public static GameMaster Instance;

    /// <summary>
    /// Compile time collection of any sprites that need to be swapped out during the course of the game
    /// </summary>
    public Sprite[] PlayerSprites = new Sprite[6];
    /// <summary>
    /// Mapping from angle of rotation to sprite
    /// </summary>
    private Dictionary<int, Sprite> _playerSpiteMap = new Dictionary<int, Sprite>();

    public Transform[] WeaponList = new Transform[2];
    private Dictionary<string, Transform> _weaponMap = new Dictionary<string, Transform>();

    /// <summary>
    /// Delegate for switching weapons
    /// </summary>
    /// <param name="choice"></param>
    public delegate void WeaponSwitchCallback();
    public WeaponSwitchCallback OnWeaponSwitch;

    /// <summary>
    /// CameraShake instance so we know we can shake the screen
    /// </summary>
    public CameraShake CamShake { get; private set; }

    /// <summary>
    /// Reference to the UI layer
    /// </summary>
    public GameObject UIOverlay;
    /// <summary>
    /// Player 1 stats UI
    /// </summary>
    private PlayerStatsUIController _player1Stats;
    /// <summary>
    /// Player 2 stats UI
    /// </summary>
    private PlayerStatsUIController _player2Stats;

    // Use this for initialization
    void Awake () {
        if (Instance == null) {
            Instance = GameObject.FindGameObjectWithTag("GAMEMASTER").GetComponent<GameMaster>();
        }

        //Load sprites for player animation map
        _playerSpiteMap.Add(0, PlayerSprites[0]);
        _playerSpiteMap.Add(45, PlayerSprites[1]);
        _playerSpiteMap.Add(90, PlayerSprites[2]);
        //Load sprites for player health UI state
        _playerSpiteMap.Add(100, PlayerSprites[3]);
        _playerSpiteMap.Add(50, PlayerSprites[4]);
        _playerSpiteMap.Add(25, PlayerSprites[5]);

        //Load weapon map
        _weaponMap.Add(WeaponList[0].gameObject.name, WeaponList[0]);
        _weaponMap.Add(WeaponList[1].gameObject.name, WeaponList[1]);
    }

    private void Start() {
        CamShake = transform.GetComponent<CameraShake>();
        if (CamShake == null) {
            throw new MissingReferenceException("No CameraShake found on gamemaster");
        }

        _player1Stats = GetPlayerStatsUi(1);
    }

    private void Update() {
        if (Input.GetButtonUp("X360_LBumper")) {
            OnWeaponSwitch.Invoke();
        }
    }

    public void UpdateHealthUI(int player, int current, int max) {
        if (player == 1) {
            _player1Stats.SetHealthStatus(current, max);
        } else {
            throw new System.Exception("This is bad because there is only one player...");
        }
    }

    public void UpdateUltimateUI(int player, int current, int max) {
        if(player == 1) {
            _player1Stats.SetUltimateStatus(current, max);
        }else {
            throw new System.Exception("This is bad because there is only one player...");
        }
    }

    public Transform GetWeaponFromMap(string weaponKey) {
        Transform weapon;
        _weaponMap.TryGetValue(weaponKey, out weapon);
        if(weapon == null) {
            weapon = WeaponList[0];
        }
        return weapon;
    }

    /// <summary>
    /// Based off a positive angle value, get the corresponding sprite from the map
    /// </summary>
    /// <param name="degreeKey"></param>
    /// <returns></returns>
    public Sprite GetSpriteFromMap(int degreeKey) {
        Sprite sprite;
        _playerSpiteMap.TryGetValue(degreeKey, out sprite);
        if(sprite == null) {
            sprite = PlayerSprites[0];
            //throw new KeyNotFoundException("degreeKey: " + degreeKey + " did not exist in the mapping");
        }
        return sprite;
    }

    /// <summary>
    /// Helper to extract the player ui objects
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public PlayerStatsUIController GetPlayerStatsUi(int player) {
        var statsName = "Player" + player + "Stats";
        var stats = UIOverlay.transform.Find(statsName).GetComponent<PlayerStatsUIController>();
        if (stats == null) {
            throw new MissingComponentException("AAttempted to extract " + statsName + " but found none");
        }
        return stats;
    }

}
