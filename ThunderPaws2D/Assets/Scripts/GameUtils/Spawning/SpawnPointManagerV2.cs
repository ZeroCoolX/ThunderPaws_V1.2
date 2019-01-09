using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointManagerV2 : MonoBehaviour {
    public static SpawnPointManagerV2 Instance;
    public Transform[] SpawnPoints = new Transform[1];

    private int _spawnPointIndex = 0;

    private void Awake() {
        if (Instance != null) {
            if (Instance != this) {
                Destroy(this.gameObject);
            }
        } else {
            Instance = this;
        }
    }

    public int GetSpawnIndex() {
        return _spawnPointIndex;
    }

    public Transform GetCurrentSpawn() {
        return SpawnPoints[_spawnPointIndex];
    }

    public void IncrementSpawnIndex() {
        _spawnPointIndex++;
    }
}
