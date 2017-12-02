using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    public static GameMaster Instance;

    public Sprite[] PlayerSprites = new Sprite[3];

    private Dictionary<int, Sprite> _playerSpiteMap = new Dictionary<int, Sprite>();

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

    // Update is called once per frame
    void Update () {
		
	}

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
