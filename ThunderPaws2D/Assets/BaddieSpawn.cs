﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieSpawn : MonoBehaviour {
    public Transform BaddiePrefab;
    public Vector2 SpawnAmount = new Vector2(1, 1);
    public float SpawnDelaySeconds = 0f;
    public Vector3 SpawnOffset = Vector3.zero;

    private void Start() {
        SetSpawnOffset();
        Invoke("SpawnBaddies", SpawnDelaySeconds);
    }

    private void SetSpawnOffset() {
        var spriteBounds = BaddiePrefab.GetComponent<SpriteRenderer>().bounds;
        var width = spriteBounds.max.x - spriteBounds.min.x;
        SpawnOffset += new Vector3(width, 0, 0);
    }

    private void SpawnBaddies() {
        Random.InitState((int)Time.time);
        var numberOfBaddies = Random.Range(SpawnAmount.x, SpawnAmount.y);
        var initialSpawn = transform.position;
        for (int i = 0; i < numberOfBaddies; ++i) {
            Spawn(initialSpawn + (SpawnOffset * i));
        }
    }

    private void Spawn(Vector3 position) {
        var clone = Instantiate(BaddiePrefab, position, Quaternion.identity) as Transform;
    }
}
