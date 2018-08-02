using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour {
    public float MaxLifetime = 10f;

    /// <summary>
    /// Necessary for collisions to the player. Allows player to pick this up
    /// </summary>
    private SimpleCollider CoinCollider;
    /// <summary>
    /// Necessary for ground and environment collisions
    /// </summary>
    private CollisionController2D _controller;
    private Vector3 _velocity;
    private float _coinGravity = -25.08f;
    private float _maxMoveSpeed = 6f;
    /// <summary>
    /// Indicates we're sitting on the ground
    /// </summary>
    private bool _landed = false;
    /// <summary>
    /// Indicates we've turned off physics and collisions
    /// </summary>
    private bool _collected = false;
    /// <summary>
    /// Either negative or positive offset for the collection point based off which direction the player is moving
    /// </summary>
    private int _coinCollectionOffset = 1;
    private Animator _shrinkAnimator;

    private Vector2 _bounceMin = new Vector2(0.25f, 7f);
    private Vector2 _bounceMax = new Vector2(0.6f, 15f);
    private Vector2 _totalBounceEffect;

    private const int PLAYER_LAYER = 8;


    public void Initialize(Vector2 initalVelocity) {
        _velocity = initalVelocity;
    }

    void Start() {
        AddCollisionDelegate();
        _controller = GetComponent<CollisionController2D>();
        _shrinkAnimator = GetComponent<Animator>();
        //Just a failsafe so the animator is disabled by default
        if (_shrinkAnimator.enabled) {
            _shrinkAnimator.enabled = false;
        }
        //Generate how much this particular coin will move around when it contacts surfaces
        GenerateBounceEffectValues();

        // Begin lifetime countdown
        Invoke("MaxLifeExceededDestroy", MaxLifetime);
    }

    private void AddCollisionDelegate() {
        CoinCollider = transform.Find(GameConstants.ObjectName_Pickup).GetComponent<SimpleCollider>();
        if (CoinCollider == null) {
            throw new MissingComponentException("No collider for the coin object");
        }
        CoinCollider.InvokeCollision += Apply;
        CoinCollider.Initialize(1 << PLAYER_LAYER);
    }

    void Update() {
        if (_collected) {
            if (_controller.enabled) {
                _controller.enabled = false;
            }
            FlyToUltimateMeter();
            return;
        }

        if (_controller.Collisions.FromBelow || _controller.Collisions.FromAbove) {
            CalculateBounce();
        }
        ApplyGravity();
        _controller.Move(_velocity * Time.deltaTime, Vector2.zero);
    }

    /// <summary>
    /// Everytime the coin collides with the ground we send it vertically in the opposite direction.
    /// Each time it hits the ground we half the speed of the last time so it eventually stops, thus creating a bounce effect
    /// </summary>
    private void CalculateBounce() {
        if (_totalBounceEffect.y > 0) {
            _totalBounceEffect.y = _totalBounceEffect.y / 2;
            _velocity.y = _totalBounceEffect.y;
            _totalBounceEffect.x = _totalBounceEffect.x / 1.1f;
            _velocity.x = _totalBounceEffect.x;
        } else {
            _velocity.y = 0;
            _velocity.x = 0;
        }
    }

    private void GenerateBounceEffectValues() {
        var bounceX = UnityEngine.Random.Range(_bounceMin.x, _bounceMax.x);
        bounceX *= Mathf.Sign(UnityEngine.Random.Range(-1, 2));
        var bounceY = UnityEngine.Random.Range(_bounceMin.y, _bounceMax.y);
        _totalBounceEffect = new Vector2(bounceX, bounceY);
    }

    private void FlyToUltimateMeter() {
        var collectionPoint = GameMasterV2.Instance.CoinCollectionOrigin;
        collectionPoint.x += _coinCollectionOffset;
        transform.position = Vector3.Lerp(transform.position, collectionPoint, 3f * Time.deltaTime);
    }

    private void Apply(Vector3 v, Collider2D c) {
        var player = c.transform.GetComponent<Player>();
        player.PickupCoin();
        AudioManager.Instance.playSound("Coin");
        // Must set the script reference so we can tell where to put the coin collection offset
        _coinCollectionOffset = player.FacingRight ? 3 : -2;
        _collected = true;
        _shrinkAnimator.enabled = true;
        Invoke("DestroyCoin", 0.75f);
    }

    /// <summary>
    /// Pickupable have a killswitch where they get destroyed no maatter what after x seconds.
    /// This helps cleanup any "stuck" objects for whatever reason
    /// </summary>
    protected void MaxLifeExceededDestroy() {
        DestroyCoin();
    }

    private void OnBecameInvisible() {
        DestroyCoin();
    }

    private void DestroyCoin() {
        Destroy(gameObject);
    }

    private void ApplyGravity() {
        _velocity.y += _coinGravity * Time.deltaTime;
    }
}
