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

    /// <summary>
    /// Used to handle where to spawn the player
    /// </summary>
    private SpawnPointManager _spawnManager;

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
        //GameMasterV2.Instance.SpawnPointIndex -= 1;
        print("Destroying BADDIEDDSS" + " at Time [" + Time.time + "]");
        if(SpawnPointManager.Instance.GetSpawnIndex() != 2) {
            Destroy(BaddiesInCheckpointRange.gameObject);
        }
    }

    public void SpawnFreshBaddiesForCheckpoint(float waitTime = 0f) {
        Invoke("Spawn", waitTime);
    }

    private void Spawn() {
        BaddieSpawn = CheckpointSpawns[CheckpointSpawnIndex];
        print("CreatingBaddies for spawn : " + gameObject.name + " with spawn index : " + SpawnPointManager.Instance.GetSpawnIndex() + " at Time [" + Time.time+"]");
        BaddiesInCheckpointRange = Checkpoints[CheckpointIndex];
        var clone = (Instantiate(BaddiesInCheckpointRange, BaddieSpawn.position, BaddieSpawn.rotation) as Transform);
        BaddiesInCheckpointRange = clone;
    }

    //void OnDrawGizmosSelected() {
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawSphere(transform.position, 8);
    //}


    private void Apply(Vector3 v, Collider2D c) {
        // Increment spawn!
        print("Hit Checkpoint!!!!!");
        SpawnPointManager.Instance.UpdateSpawnIndex();
        if(SpawnPointManager.Instance.GetSpawnIndex() != 2) {
            SpawnFreshBaddiesForCheckpoint();
        }
        // Refresh the players health once they hit the checkpoint
        c.transform.GetComponent<Player>().RegenerateAllHealth();
    }
}
