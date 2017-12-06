using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour {

    /// <summary>
    /// Necessary for collisions to the player. Allows player to pick this up
    /// </summary>
    private SimpleCollider CoinCollider;
    /// <summary>
    /// Necessary for ground and environment collisions
    /// </summary>
    private SimpleCollider BaseCollider;
    /// <summary>
    /// How fast we're moving
    /// </summary>
    private Vector3 _velocity;
    /// <summary>
    /// Gravity of coin
    /// </summary>
    private float _coinGravity = -0.1f;
    /// <summary>
    /// Maximum movement speed
    /// </summary>
    private float _maxMoveSpeed = 2f;
    /// <summary>
    /// Indicates we're sitting on the ground
    /// </summary>
    private bool _landed = false;

    // Use this for initialization
    void Start() {
        SetupCoinCollider();
        SetupBaseCollider();
    }

    private void SetupCoinCollider() {
        //Add delegate for collision detection
        CoinCollider = transform.Find("Pickup").GetComponent<SimpleCollider>();
        if (CoinCollider == null) {
            throw new MissingComponentException("No collider for the coin object");
        }
        CoinCollider.InvokeCollision += Apply;
        //PLAYER
        CoinCollider.Initialize(1 << 8);
    }

    private void SetupBaseCollider() {
        //Add delegate for collision detection
        BaseCollider = transform.Find("Base").GetComponent<SimpleCollider>();
        if (CoinCollider == null) {
            throw new MissingComponentException("No collider for the coin object");
        }
        BaseCollider.InvokeCollision += Land;
        //OBSTACLE
        BaseCollider.Initialize(1 << 10, Vector2.down, Vector2.down * 20, _maxMoveSpeed ,"");
    }

    void Update() {
        if (!_landed) {
            if (_velocity.y <= _maxMoveSpeed) {
                ApplyGravity();
            }
            transform.Translate(_velocity);
        }
    }

    private void Land(Vector3 v, Collider2D c) {
        _landed = true;
    }

    private void Apply(Vector3 v, Collider2D c) {
        var player = c.transform.GetComponent<Player>();
        player.PickupCoin();
        Destroy(gameObject);
    }

    private void ApplyGravity() {
        _velocity.y += _coinGravity * Time.deltaTime;
    }
}
