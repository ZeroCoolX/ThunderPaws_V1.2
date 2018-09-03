using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeSpawnPoint : MonoBehaviour {

    public Transform BaddiePrefab;

    public int MaxBaddiesAllowed;
    public int MinBaddiesAllowed;

    private int _baddiesSpawned;

    // Use this for initialization
    void Start () {
        var interval = 0f;
        for (int i = 0; i < MaxBaddiesAllowed; ++i) {
            Invoke("SpawnNewRoundOfBaddies", interval);
            interval += 0.5f;
        }
    }
	
	// Update is called once per frame
	void Update () {
        //if (_baddiesSpawned <= 0) {
        //    for (int i = 0; i < MaxBaddiesAllowed; ++i) {
        //        Invoke("SpawnNewRoundOfBaddies", 0.5f);
        //    }
        //}
	}

    private void SpawnNewRoundOfBaddies() {
        ++_baddiesSpawned;
        var clone = Instantiate(BaddiePrefab, transform.position, transform.rotation);
        clone.GetComponent<SpriteRenderer>().color = Color.red;
        clone.GetComponent<Robot_GL1>().Health = 100;
        clone.GetComponent<Robot_GL1>().LedgeBound = true;
        clone.GetComponent<LifetimeController>().enabled = false;
        clone.GetComponent<Robot_GL1>().enabled = true;
    }

}
