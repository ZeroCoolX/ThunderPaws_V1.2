using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEndOfLevel : MonoBehaviour {
    private SimpleCollider Collider;
    public Transform Camera;
    private const int PLAYER_LAYER = 8;
    private bool _beginMovement = false;
    private Transform _player;
    private Vector3 _velocity;

    void Start() {
        // Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << PLAYER_LAYER, 5);
    }

    private void Update() {
        if (_beginMovement) {
            _player.transform.GetComponent<Player>().Get2DController().Move(_velocity * Time.deltaTime);
        }
    }

    private void EndLevel() {
        GameMasterV2.Instance.GameOver();
    }

    private void Apply(Vector3 v, Collider2D c) {
        // Lock Camera
        Camera.GetComponent<Camera2DFollow>().Target = transform;

        // Remove player input
        _velocity = c.transform.GetComponent<Player>().GetCurrentVelocity();
        c.transform.GetComponent<PlayerInputController>().enabled = false;
        c.transform.GetComponent<Player>().enabled = false;
        _player = c.transform;
        _beginMovement = true;

        Invoke("EndLevel", 2.5f);
        //Move player out of frame then show game over screen

    }
}
