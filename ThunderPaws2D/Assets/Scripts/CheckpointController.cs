using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour {
    /// <summary>
    /// Necessary for collisions
    /// </summary>
    private SimpleCollider Collider;
    public Transform BaddiesInCheckpointRange;
    public Transform BaddieSpawn;
    public string BaddieCheckpointName;
    public int CheckpointIndex;
    public Transform[] Checkpoints;
    public int CheckpointSpawnIndex;
    public Transform[] CheckpointSpawns;

    // Use this for initialization
    void Start() {
        //Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << 8, 8);
    }

    public void DeactivateBaddiesInCheckpoint() {
        //GameMaster.Instance.SpawnPointIndex -= 1;
        print("Destroying BADDIEDDSS");
        Destroy(BaddiesInCheckpointRange.gameObject);
    }

    public void SpawnFreshBaddiesForCheckpoint() {
        BaddieSpawn = CheckpointSpawns[CheckpointSpawnIndex];
        print("CreatingBaddies for spawn : " + gameObject.name + " with spawn index : " + GameMaster.Instance.SpawnPointIndex);
        BaddiesInCheckpointRange = Checkpoints[CheckpointIndex];
        var clone = (Instantiate(BaddiesInCheckpointRange, BaddieSpawn.position, BaddieSpawn.rotation) as Transform);
        BaddiesInCheckpointRange = clone;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 8);
    }


    private void Apply(Vector3 v, Collider2D c) {
        // Increment spawn!
        print("Hit Checkpoint!!!!!");
        GameMaster.Instance.SpawnPointIndex += 1;
        SpawnFreshBaddiesForCheckpoint();
    }
}
