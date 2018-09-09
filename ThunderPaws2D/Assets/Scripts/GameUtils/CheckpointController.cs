using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour {
    public Transform BaddiesInCheckpointRange;
    public Transform BaddieSpawn;
    public string BaddieCheckpointName;
    public int CheckpointIndex;
    public Transform[] Checkpoints;
    public int CheckpointSpawnIndex;
    public Transform[] CheckpointSpawns;

    private SimpleCollider Collider;
    private SpawnPointManager _spawnManager;

    private const int PLAYER_LAYER = 8;
    private const int COLLISION_RADIUS = 8;

    void Start() {
        SetupCollisionDelegate();
    }

    private void SetupCollisionDelegate() {
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << PLAYER_LAYER, COLLISION_RADIUS);
    }

    public void DeactivateBaddiesInCheckpoint() {
        print("Destroying Baddies" + " at Time [" + Time.time + "]");
        if(SpawnPointManager.Instance.GetSpawnIndex() != 3) {
            Destroy(BaddiesInCheckpointRange.gameObject);
        }
    }

    public void SpawnFreshBaddiesForCheckpoint(float waitTime = 0f) {
        Invoke("Spawn", waitTime);
    }

    private void Spawn() {
        // Only spawn if we can
        if(CheckpointSpawnIndex == CheckpointSpawns.Length) {
            return;
        }
        BaddieSpawn = CheckpointSpawns[CheckpointSpawnIndex];
        print("Creating Baddies for spawn : " + gameObject.name + " with spawn index : " + SpawnPointManager.Instance.GetSpawnIndex() + " at Time [" + Time.time+"]");
        BaddiesInCheckpointRange = Checkpoints[CheckpointIndex];
        var clone = (Instantiate(BaddiesInCheckpointRange, BaddieSpawn.position, BaddieSpawn.rotation) as Transform);
        BaddiesInCheckpointRange = clone;
    }

    private void Apply(Vector3 v, Collider2D c) {
        print("Checkpoint activated");
        SpawnPointManager.Instance.IncrementSpawnIndex();
        SpawnFreshBaddiesForCheckpoint();
        // Refresh the players health once they hit the checkpoint
        c.transform.GetComponent<Player>().RegenerateAllHealth();
    }
}
