using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeSpawnController : MonoBehaviour {

    public GameObject Maze;
    private SimpleCollider Collider;
    private const int PLAYER_LAYER = 8;


    void Start() {
        // Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << PLAYER_LAYER, 5);
    }

    private void Apply(Vector3 v, Collider2D c) {
        Maze.SetActive(true);
    }
}
