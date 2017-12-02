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
    /// Compile time sollection of any sprites that need to be swapped out during the course of the game
    /// </summary>
    public Sprite[] PlayerSprites = new Sprite[3];
    /// <summary>
    /// Mapping from angle of rotation to sprite
    /// </summary>
    private Dictionary<int, Sprite> _playerSpiteMap = new Dictionary<int, Sprite>();

    /// <summary>
    /// CameraShake instance so we know we can shake the screen
    /// </summary>
    public CameraShake CamShake;

	// Use this for initialization
	void Awake () {
        if (Instance == null) {
            Instance = GameObject.FindGameObjectWithTag("GAMEMASTER").GetComponent<GameMaster>();
        }

        //Load sprite map
        _playerSpiteMap.Add(0, PlayerSprites[0]);
        _playerSpiteMap.Add(45, PlayerSprites[1]);
        _playerSpiteMap.Add(90, PlayerSprites[2]);
    }

    private void Start() {
        CamShake = transform.GetComponent<CameraShake>();
        if (CamShake == null) {
            throw new MissingReferenceException("No CameraShake found on gamemaster");
        }
        
    }

    // Update is called once per frame
    void Update () {
		
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
