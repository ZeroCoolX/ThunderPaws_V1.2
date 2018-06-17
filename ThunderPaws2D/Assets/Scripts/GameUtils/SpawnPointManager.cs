using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour {
    /// <summary>
    /// Scene specific spawn points.
    /// the first index must always be the initial spawn point of the scene
    /// </summary>
    public Transform[] SpawnPoints;
    /// <summary>
    /// Indicates which spawn point the player should spawn from
    /// Used for checkpoints
    /// </summary>
    private int _spawnPointIndex;

    //TODO: this needs to be taken out because its a hack
    public int GetSpawnIndex() {
        return _spawnPointIndex;
    }

    /// <summary>
    /// Return the current spawn in the list
    /// </summary>
    /// <returns></returns>
    public Transform GetCurrentSpawn() {
        return SpawnPoints[_spawnPointIndex];
    }

    /// <summary>
    /// Incrmenet the index
    /// </summary>
    public void UpdateSpawnIndex() {
        _spawnPointIndex++;
    }

    private void Awake() {
        _spawnPointIndex = 0;
    }
}
