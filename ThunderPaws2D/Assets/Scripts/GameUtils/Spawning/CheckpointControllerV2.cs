using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointControllerV2 : MonoBehaviour {
    public Transform BaddieSpawnGroup;
    private Transform _activeBaddieSpawnGroup;

    private SimpleCollider _collider;

    private const int PLAYER_LAYER = 8;
    private const int COLLISION_RADIUS = 8;

    void Start() {
        SetupCollisionDelegate();
    }

    private void SetupCollisionDelegate() {
        _collider = GetComponent<SimpleCollider>();
        if (_collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        _collider.InvokeCollision += Apply;
        _collider.Initialize(1 << PLAYER_LAYER, COLLISION_RADIUS);
    }

    private void Apply(Vector3 v, Collider2D c) {
        print("Checkpoint " + gameObject.name + " Activated");
        SpawnPointManagerV2.Instance.IncrementSpawnIndex();
        SpawnFreshBaddiespawnGroup();
        c.transform.GetComponent<Player>().RegenerateAllHealth();
    }

    public void ResetBaddieSpawnGroup(float waitTime = 0f) {
        CleanupLeftoverBaddieSpawnGroup();
        SpawnFreshBaddiespawnGroup(waitTime);
    }

    private void CleanupLeftoverBaddieSpawnGroup() {
        print("Destroying BaddieSpawnGroup at Time [" + Time.time + "]");
        if (_activeBaddieSpawnGroup != null) {
            Destroy(_activeBaddieSpawnGroup.gameObject);
        }
    }

    private void SpawnFreshBaddiespawnGroup(float waitTime = 0f) {
        Invoke("Spawn", waitTime);
    }

    private void Spawn() {
        print("Creating baddie spawn group : " + BaddieSpawnGroup.gameObject.name + " for spawn index : " + SpawnPointManagerV2.Instance.GetSpawnIndex() + " at Time [" + Time.time + "]");
        _activeBaddieSpawnGroup = Instantiate(BaddieSpawnGroup, transform.position, Quaternion.identity, transform) as Transform;
    }
}
