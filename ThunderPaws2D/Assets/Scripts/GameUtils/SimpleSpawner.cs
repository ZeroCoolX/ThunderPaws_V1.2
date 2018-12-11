using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpawner : MonoBehaviour {
    public Transform SpawnPrefab;
    public Transform[] SpawnPoints;
    public int SpawnEverySeconds = 5;
    public int SpawnMin = 0;
    public int SpawnMax = 1;
    

    private bool _shouldSpawn;
    private float _spawnCounter;

    private void Start() {
        _spawnCounter = Time.time + SpawnEverySeconds;
    }

    private void Update() {
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
            var clone = Instantiate(SpawnPrefab, position.position, Quaternion.identity);
            //clone.GetComponent<BaddieLifeform>().enabled = true;
        }
    }
}
