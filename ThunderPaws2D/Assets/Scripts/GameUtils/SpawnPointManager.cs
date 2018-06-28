using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour {
    public static SpawnPointManager Instance;

    /// <summary>
    /// Scene specific spawn points.
    /// the first index must always be the initial spawn point of the scene
    /// </summary>
    public Transform[] SpawnPoints = new Transform[1];
    /// <summary>
    /// Indicates which spawn point the player should spawn from
    /// Used for checkpoints
    /// </summary>
    private int _spawnPointIndex = -1;

    /// <summary>
    /// For NOW others need this.
    /// </summary>
    /// <returns></returns>
    public int GetSpawnIndex() {
        return _spawnPointIndex;
    }

    /// <summary>
    /// Return the current spawn in the list
    /// </summary>
    /// <returns></returns>
    public Transform GetCurrentSpawn() {
        return SpawnPoints[_spawnPointIndex == -1 ? 0 : _spawnPointIndex];
    }

    /// <summary>
    /// Incrmenet the index
    /// </summary>
    public void UpdateSpawnIndex() {
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
