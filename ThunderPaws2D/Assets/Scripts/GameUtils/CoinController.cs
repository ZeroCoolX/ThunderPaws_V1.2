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
    private CollisionController2D _controller;
    /// <summary>
    /// Animator reference to play shrinking animation
    /// </summary>
    private Animator _shrinkAnimator;
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
    /// <summary>
    /// Min bounce values
    /// </summary>
    private Vector2 _bounceMin = new Vector2(0.25f, 7f);
    /// <summary>
    /// Max bounce values
    /// </summary>
    private Vector2 _bounceMax = new Vector2(0.6f, 15f);
    /// <summary>
    /// Generated randomly for a cool effect
    /// </summary>
    private Vector2 _totalBounceEffect;

    /// <summary>
    /// Optional method to pass in a start velocity for the coin
    /// </summary>
    public void Initialize(Vector2 initalVelocity) {
        _velocity = initalVelocity;
    }

    // Use this for initialization
    void Start() {
        SetupCoinCollider();
        _controller = GetComponent<CollisionController2D>();
        _shrinkAnimator = GetComponent<Animator>();
        //Just a failsafe so the animator is disabled by default
        if (_shrinkAnimator.enabled) {
            _shrinkAnimator.enabled = false;
        }
        //Generate how much this particular coin will move around when it contacts surfaces
        GenerateBounceEffectValues();
    }

    private void SetupCoinCollider() {
        //Add delegate for collision detection
        CoinCollider = transform.Find(GameConstants.ObjectName_Pickup).GetComponent<SimpleCollider>();
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
            CalculateBounce();
        }
        ApplyGravity();
        _controller.Move(_velocity * Time.deltaTime, Vector2.zero);
    }

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
        _shrinkAnimator.enabled = true;
        Invoke("DestroyCoin", 0.75f);
    }

    private void DestroyCoin() {
        Destroy(gameObject);
    }

    private void ApplyGravity() {
        _velocity.y += _coinGravity * Time.deltaTime;
    }
}
