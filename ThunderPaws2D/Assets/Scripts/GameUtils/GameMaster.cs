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
    public Sprite[] PlayerSprites = new Sprite[3];
    /// <summary>
    /// Mapping from angle of rotation to sprite
    /// </summary>
    private Dictionary<int, Sprite> _playerSpiteMap = new Dictionary<int, Sprite>();

    public Transform[] WeaponList = new Transform[2];
    private Dictionary<string, Transform> _weaponMap = new Dictionary<string, Transform>();

    /// <summary>
    /// CameraShake instance so we know we can shake the screen
    /// </summary>
    public CameraShake CamShake { get; private set; }

	// Use this for initialization
	void Awake () {
        if (Instance == null) {
            Instance = GameObject.FindGameObjectWithTag("GAMEMASTER").GetComponent<GameMaster>();
        }

        //Load sprite map
        _playerSpiteMap.Add(0, PlayerSprites[0]);
        _playerSpiteMap.Add(45, PlayerSprites[1]);
        _playerSpiteMap.Add(90, PlayerSprites[2]);

        //Load weapon map
        _weaponMap.Add("DEFAULT", WeaponList[0]);
        _weaponMap.Add("GUN", WeaponList[1]);
    }

    private void Start() {
        CamShake = transform.GetComponent<CameraShake>();
        if (CamShake == null) {
            throw new MissingReferenceException("No CameraShake found on gamemaster");
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

}
