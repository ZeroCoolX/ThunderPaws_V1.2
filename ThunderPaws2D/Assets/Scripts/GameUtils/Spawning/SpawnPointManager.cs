using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour {
    public static SpawnPointManager Instance;
    public Transform[] SpawnPoints = new Transform[1];

    private int _spawnPointIndex = -1;



    public int GetSpawnIndex() {
        return _spawnPointIndex;
    }

    public Transform GetCurrentSpawn() {
        return SpawnPoints[_spawnPointIndex == -1 ? 0 : _spawnPointIndex];
    }

    public void IncrementSpawnIndex() {
        if(_spawnPointIndex == -1) {
            _spawnPointIndex = 1;
        }else {
            _spawnPointIndex++;
        }
    }

    private void Awake() {
        if (Instance != null) {
            if (Instance != this) {
                Destroy(this.gameObject);
            }
        } else {
            Instance = this;
        }
        _spawnPointIndex = -1;
    }
}
