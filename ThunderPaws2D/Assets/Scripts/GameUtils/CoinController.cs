﻿using System.Collections;
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
    private CollisionController2D _controller;
    /// <summary>
    /// How fast we're moving
    /// </summary>
    private Vector3 _velocity;
    /// <summary>
    /// Gravity of coin
    /// </summary>
    private float _coinGravity = -25.08f;
    /// <summary>
    /// Maximum movement speed
    /// </summary>
    private float _maxMoveSpeed = 6f;
    /// <summary>
    /// Indicates we're sitting on the ground
    /// </summary>
    private bool _landed = false;
    /// <summary>
    /// Indicates we've turned off physics and collision and instead
    /// </summary>
    private bool _collected = false;
    /// <summary>
    /// Either negative or positive offset for the collection point based off which direction the player is moving
    /// </summary>
    private int _coinCollectionOffset = 1;

    // Use this for initialization
    void Start() {
        SetupCoinCollider();
        _controller = GetComponent<CollisionController2D>();
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

    void Update() {
        if (_collected) {
            if (_controller.enabled) {
                _controller.enabled = false;
            }
            FlyToUltimateMeter();
            return;
        }
        //Do not accumulate gravity if colliding with anythig vertical
        if (_controller.Collisions.FromBelow || _controller.Collisions.FromAbove) {
            _velocity.y = 0;
        }
        ApplyGravity();
        _controller.Move(_velocity * Time.deltaTime, Vector2.zero);
    }

    private void FlyToUltimateMeter() {
        var collectionPoint = GameMaster.Instance.CoinCollectionOrigin;
        collectionPoint.x += _coinCollectionOffset;
        transform.position = Vector3.Lerp(transform.position, collectionPoint, 3f * Time.deltaTime);
    }

    private void Apply(Vector3 v, Collider2D c) {
        var player = c.transform.GetComponent<Player>();
        player.PickupCoin();
        //Must set the script reference so we can tell where to put the coin collection offset
        _coinCollectionOffset = player.FacingRight ? 3 : -2;
        _collected = true;
        Invoke("DestroyCoin", 0.75f);
    }

    private void DestroyCoin() {
        Destroy(gameObject);
    }

    private void ApplyGravity() {
        _velocity.y += _coinGravity * Time.deltaTime;
    }
}
