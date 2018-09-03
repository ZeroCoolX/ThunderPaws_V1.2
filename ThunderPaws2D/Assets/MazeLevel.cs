//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MazeLevel : MonoBehaviour {

//    public Transform[] SpawnLevels;
//    //private Dictionary<>
//    private Transform _player;

//    // Use this for initialization
//    void Start () {
//        if (_player == null) {
//            FindPlayer();
//        }
//	}

//    private void FindPlayer() {
//            GameObject searchResult = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);
//            if (searchResult != null) {
//                _player = searchResult.transform;
//            }
//    }
	
//	// Update is called once per frame
//	void Update () {
//    if (_player == null) {
//        FindPlayer();
//        return;
//    }
//    foreach (var spawn in SpawnLevels) {
//            if (HorizontallyEqual(spawn.transform.position.y)) {
//            }

//        }
//	}

//    private bool HorizontallyEqual(float yPosition) {
//        return (int)yPosition == (int)_player.position.y;
//    }
//}
