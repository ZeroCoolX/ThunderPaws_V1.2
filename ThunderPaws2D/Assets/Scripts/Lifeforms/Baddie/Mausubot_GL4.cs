using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mausubot_GL4 : GroundBaddieLifeform {

    private const float GRAVITY = -25.08f;
    private const int MOVE_SPEED = 1;
    private const float VISION_LENGTH = 10;
    private const float MAX_STOP_SECONDS = 0.25f;

    private float _mimicTimeThreshold = -1f;
    private float _mimicToggleDelay = 5f;

    private bool _mimicActive = false;

    private Queue<MimicData> _mimicData;
    private float _firstShot = 0f;

    private SimpleCollider _simpleCollider;

    void Start () {
        base.Start();

        GroundPositionData.MoveSpeed = MOVE_SPEED;
        VisionRayLength = VISION_LENGTH;
        GroundPositionData.MaxStopSeconds = MAX_STOP_SECONDS;
        Gravity = GRAVITY;

        _simpleCollider = GetComponent<SimpleCollider>();
        if(_simpleCollider == null) {
            throw new MissingComponentException("Simple collider is null");
        }

        var bounds = GetComponent<BoxCollider2D>().bounds;
        var width = bounds.max.x - bounds.min.x;
        var height = bounds.max.y - bounds.min.y;

        _simpleCollider.InvokeCollision += Apply;
        _simpleCollider.Initialize(1 << 12, new Vector2(width * 1.5f, height * 1.5f), true);
    }

    private void OnDrawGizmos() {
        var bounds = GetComponent<BoxCollider2D>().bounds;
        var width = bounds.max.x - bounds.min.x;
        var height = bounds.max.y - bounds.min.y;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector2(width * 1.5f, height * 1.5f));
    }

    void Update () {
        base.Update();

        if (!CheckTargetsExist()) {
            return;
        }

        var directionToTarget = transform.position.x - Target.position.x;

        if (Time.time >= _mimicTimeThreshold) {
            _mimicTimeThreshold = Time.time + _mimicToggleDelay;
            _mimicActive = !_mimicActive;
            _simpleCollider.enabled = _mimicActive;
            if (_mimicActive) {
                _mimicData = new Queue<MimicData>();
                _firstShot = 0f;
            }else {
                MimicShots();
            }
        }

        Animator.SetBool("mimic", _mimicActive);

        // Face that direction
        CalculateFacingDirection(directionToTarget);

        // Move in that direction
        if (Time.time > GroundPositionData.TimeStopped) {
            CalculateVelocity();
        }

        Move();
    }

    private void CalculateVelocity() {
        float targetVelocityX = GroundPositionData.MoveSpeed * (FacingRight ? 1 : -1);
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref GroundPositionData.VelocityXSmoothing, Controller2d.Collisions.FromBelow ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        ApplyGravity();
    }

    private void MimicShots() {
        while(_mimicData.Count > 0) {
            var data = _mimicData.Dequeue();
            StartCoroutine(Fire(data.Delay, data.Position, data.Damage));
        }
    }

    private IEnumerator Fire(float delay, Vector3 pos, int damage) {
        yield return new WaitForSeconds(delay);

        var combinedPos = new Vector3(ProjectileData.FirePoint.position.x, pos.y, transform.position.z);
        Transform clone = Instantiate(BulletPrefab, combinedPos, transform.rotation) as Transform;
        AudioManager.Instance.PlaySound("BasicShot");
        // Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
        AbstractProjectile projectile = clone.GetComponent<BulletProjectile>();

        // Set layermask of parent (either player or baddie)
        projectile.SetLayerMask(ProjectileData.WhatToHit);
        projectile.Damage = damage;
        projectile.MaxLifetime = 10;
        projectile.Fire((FacingRight ? Vector2.right : Vector2.left), Vector2.up);
        Invoke("ResetAttack", 0.5f);
    }

    private void Apply(Vector3 v, Collider2D c) {
        if(_mimicData.Count == 0) {
            _firstShot = Time.time;
        }
        _mimicData.Enqueue(new MimicData {
            Delay = Time.time - _firstShot,
            Position = c.transform.position,
            Damage = c.transform.GetComponent<BulletProjectile>().Damage
        });
        Destroy(c.gameObject);
    }

    public struct MimicData {
        public float Delay { get; set; }
        public Vector3 Position { get; set; }
        public int Damage { get; set; }
    }
}
