using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlayerMover : MonoBehaviour {
    public float MoveForSeconds;

    private GameObject[] Players;
    private SimpleCollider _collider;
    private const int PLAYER_LAYER = 8;

    void Start() {
        _collider = GetComponent<SimpleCollider>();
        if (_collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        _collider.InvokeCollision += Apply;
        _collider.Initialize(1 << PLAYER_LAYER, 5);
    }

    private void Apply(Vector3 v, Collider2D c) {
        StartCoroutine(FindPlayers());

        foreach (var player in Players) {
            player.transform.GetComponent<PlayerInputController>().enabled = false;
            player.transform.GetComponent<Player>().DirectionalInput = new Vector2(1f, 0f);
        }
        Invoke("StopMovement", MoveForSeconds);
    }

    private IEnumerator FindPlayers() {
        print("finding players");
        var players = GameObject.FindGameObjectsWithTag(GameConstants.Tag_Player);

        if (players != null || players.Length >= 0) {
            print(players.Length + " players found!");
            Players = players;
            yield return new WaitForSeconds(0f);
        } else {
            print("No Players found, waiting 1 second then trying again");
            yield return new WaitForSeconds(1f);
            StartCoroutine(FindPlayers());
        }
    }

    private void StopMovement() {
        foreach (var player in Players) {
            player.transform.GetComponent<Player>().DirectionalInput = Vector2.zero;
            player.transform.GetComponent<PlayerInputController>().enabled = true;
        }
    }
}
