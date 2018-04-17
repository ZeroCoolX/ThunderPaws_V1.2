using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableLifeform : BaseLifeform {
    /// <summary>
    /// This is what the explosion will generate
    /// </summary>
    public Transform PayloadContent;
    /// <summary>
    /// Stored instantiated prefabs awaiting activation
    /// </summary>
    private List<Transform> _coinPayload = new List<Transform>();
    /// <summary>
    /// Hoow many coins in the payloada
    /// </summary>
    public int _coinsInPayload = 10;
    /// <summary>
    /// Indicated we should flash white because we got damaged
    /// </summary>
    protected bool _damage = false;
    /// <summary>
    /// How much health this object has
    /// </summary>
    public float Health = 1f;
    /// <summary>
    /// Min bounce values
    /// </summary>
    private Vector2 _explodeMin = new Vector2(1f, 7f);
    /// <summary>
    /// Max bounce values
    /// </summary>
    private Vector2 _explodeMax = new Vector2(2f, 15f);

    /// <summary>
    /// Optional property that allows this object to be effected by gravity
    /// </summary>
    public float Gravity = 0f;
    /// <summary>
    /// Lifeform movement
    /// </summary>
    public Vector3 Velocity;
    /// <summary>
    /// collision detection controller
    /// </summary>
    protected CollisionController2D Controller;

    // Use this for initialization
    public void Start () {
        //Phsyics controller used for all collision detection
        Controller = transform.GetComponent<CollisionController2D>();
        if(Controller == null) {
            throw new MissingComponentException("There is no CollisionController2D on this object");
        }
    }

    // Update is called once per frame
    public void Update () {
        if (_damage) {
            GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0.8f);
            _damage = false;
        }else {
            GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0f);
        }
        if(Health <= 0) {
            GenerateCoinPayload();
            Explode();
            Destroy(gameObject);
        }
        //Do not accumulate gravity if colliding with anythig vertical
        if (Controller.Collisions.FromBelow || Controller.Collisions.FromAbove) {
            Velocity.y = 0;
        }
        ApplyGravity();
    }

    private void GenerateCoinPayload() {
        for (var i = 0; i < _coinsInPayload; ++i) {
            var coin = Instantiate(PayloadContent, transform.position, Quaternion.identity) as Transform;
            coin.gameObject.SetActive(false);
            var coinController = coin.GetComponent<CoinController>();
            coinController.Initialize(GenerateRandomExplosionDirection());
            coinController.enabled = false;
            _coinPayload.Add(coin);
        }
    }

    private Vector2 GenerateRandomExplosionDirection() {
        var explodeX = UnityEngine.Random.Range(_explodeMin.x, _explodeMax.x);
        explodeX *= Mathf.Sign(UnityEngine.Random.Range(-1, 2));
        var explodeY = UnityEngine.Random.Range(_explodeMin.y, _explodeMax.y);
        return new Vector2(explodeX, explodeY);
}

    private void Explode() {
        foreach(var coin in _coinPayload) {
            coin.GetComponent<CoinController>().enabled = true;
            coin.gameObject.SetActive(true);
        }
    }

    public override void Damage(float dmg) {
        Health -= dmg;
        _damage = true;
    }

    protected void ApplyGravity() {
        Velocity.y += Gravity * Time.deltaTime;
    }
}
