using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractProjectile : MonoBehaviour {
    /// <summary>
    /// Indicates that even if the bullet didn't collide with anything it should explode upon death
    /// </summary>
    public bool ExplodeOnDeath;
    public SimpleCollider AoeDamageCollider;
    public Transform ImpactEffectPrefab;
    public string ImpactSound;
    public float MoveSpeed = 30f;
    public int Damage;
    public float MaxLifetime = 0.5f;
    /// <summary>
    /// Precalculated values necessary for determining how to spray the particles and where we THINK the collision will take place
    /// </summary>
    protected Vector3 TargetPos;
    /// <summary>
    /// Allows us to collect stats about which player fired the shot so if we kill someone we can correctly add it to their kill count
    /// </summary>
    public int FromPlayerNumber;
    /// <summary>
    /// POSSIBLY DEPRECATED
    /// </summary>
    protected Vector3 TargetNormal;
    /// <summary>
    /// Specifies what direction the bullet should move
    /// </summary>
    protected Vector3 TargetDirection;
    [SerializeField]
    public LayerMask WhatToHit;

    protected SimpleCollider Collider;

    protected void Start() {
        SetupSimpleCollider();
        // Begin lifetime countdown
        Invoke("MaxLifeExceededDestroy", MaxLifetime);
    }

    public void SetLayerMask(LayerMask parentLayerMask) {
        WhatToHit = parentLayerMask;
    }

    public void Fire(Vector3 targetPos, Vector3 targetNormal) {
        TargetPos = targetPos;
        TargetNormal = targetNormal;
        TargetDirection = TargetPos;
    }

    public void ResetTargetDirection(LayerMask newLayerMask) {
        TargetDirection = -TargetDirection;
        TargetPos = (-TargetDirection) * 50;
        ResetSimpleColliderLayermask(newLayerMask);
    }

    public void ResetSimpleColliderLayermask(LayerMask newLayerMask) {
        Collider.Initialize(newLayerMask, TargetDirection, TargetPos, MoveSpeed, GameConstants.Layer_ObstacleThrough);
    }

    private void SetupSimpleCollider() {
        //Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += HitTarget;
        Collider.Initialize(WhatToHit, TargetDirection, TargetPos, MoveSpeed, GameConstants.Layer_ObstacleThrough);
    }

    protected void Apply(Vector3 v, Collider2D c) {
        var lifeform = c.transform.GetComponent<BaseLifeform>();
        if (lifeform != null) {
            print("Hit lifeform: " + lifeform.gameObject.name + " and did " + Damage + " damage");
            if (lifeform.Damage(Damage)) {
                GameStatsManager.Instance.AddBaddie(FromPlayerNumber);
            }
        }
    }

    /// <summary>
    /// Bullets have a killswitch where they get destroyed no matter what after x seconds.
    /// This helps cleanup any "stuck" bullets for whatever reason
    /// </summary>
    protected void MaxLifeExceededDestroy() {
        if (ExplodeOnDeath) {
            GenerateEffect();
        }
        Destroy(gameObject);
    }

    protected void GenerateEffect() {
        var clone = Instantiate(ImpactEffectPrefab, transform.position, transform.rotation);
        clone.GetComponent<SpriteRenderer>().sortingOrder = 10;
        clone.GetComponent<DeathTimer>().TimeToLive = 0.25f;
        clone.GetComponent<Animator>().SetBool("Invoke", true);
        // Set the optional AOE damage controller if it has one
        var aoe = clone.GetComponent<AoeDamageController>();
        if (aoe != null) {
            // AOE damage should not be 100% of the initial damage so just give off 75% of it
            aoe.Damage = Damage * 0.75f;
        }
        AudioManager.Instance.PlaySound(string.IsNullOrEmpty(ImpactSound) ? "SmallExplosion" : ImpactSound);
    }

    protected abstract void Move();

    protected abstract void HitTarget(Vector3 hitPos, Collider2D hitObject);

    /// <summary>
    /// Once the bullet leaves the Cameras viewport destroy it
    /// </summary>
    private void OnBecameInvisible() {
        Destroy(gameObject);
    }
}
