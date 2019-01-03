using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpawner : MonoBehaviour {
    public Transform[] SpawnPrefabs;
    public Transform[] SpawnPoints;
    public int SpawnEverySeconds = 5;
    public int SpawnMin = 0;
    public int SpawnMax = 1;
    

    private bool _shouldSpawn = true;
    private float _spawnCounter;

    public Transform Boss;

    private void Start() {
        _spawnCounter = Time.time + SpawnEverySeconds;
        Boss.GetComponent<BaddieBoss>().StopSpawning += StopSpawning;
    }

    private void StopSpawning() {
        _shouldSpawn = false;
    }

    private void Update() {
        if (!_shouldSpawn) {
            return;
        }
        if (Time.time > _spawnCounter) {
            foreach(var spawn in SpawnPoints) {
                print("Spawning baddies!");
                SpawnIfPossible(spawn);
            }
            _spawnCounter = Time.time + SpawnEverySeconds;
        }
    }

    private void SpawnIfPossible(Transform position) {
        Random.InitState((int)Time.time);
        var rand = Random.Range(SpawnMin, SpawnMax);
        for(var i = 0; i < rand; ++i) {
            var rand2 = Random.Range(1, 2);
            if(rand2 % 2 == 0) { continue; }
            var randIndex = Random.Range(1, 10);
            var clone = Instantiate(SpawnPrefabs[randIndex < 8 ? 0 : 1], position.position, Quaternion.identity);
            //clone.GetComponent<BaddieLifeform>().enabled = true;
        }
    }
}
