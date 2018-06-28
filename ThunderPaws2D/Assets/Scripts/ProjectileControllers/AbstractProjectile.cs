﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractProjectile : MonoBehaviour {
    /// <summary>
    /// Indicates that even if the bullet didn't collide with anything it should explode upon death
    /// </summary>
    public bool ExplodeOnDeath;
    /// <summary>
    /// Sprite animatio to play when the bullet impacts
    /// </summary>
    public Transform ImpactEffect;

    /// <summary>
    /// Optional audio to play on impact - otherwise a random of two 
    /// default small explosion will play
    /// </summary>
    public string ImpactSound;

    /// <summary>
    /// Some projectiles explode and cause AOE damage
    /// </summary>
    public SimpleCollider AoeDamageCollider;

    /// <summary>
    /// How fast the bullet travels
    /// Set from the weapon calling its creation
    /// 30f is the defaualt speed if one is not set
    /// </summary>
    public float MoveSpeed = 30f;

    /// <summary>
    /// How much damage this bullet does
    /// </summary>
    public int Damage;

    /// <summary>
    /// Max time in seconds this object can stay alive
    /// </summary>
    public float MaxLifetime = 0.5f;

    /// <summary>
    /// Precalculated values necessary for determining how to spray the particles, where we THINK the collision will take place
    /// </summary>
    protected Vector3 TargetPos;
    /// <summary>
    /// POSSIBLY DEPRECATED
    /// </summary>
    protected Vector3 TargetNormal;
    /// <summary>
    /// Specifies what direction the bullet should move
    /// </summary>
    protected Vector3 TargetDirection;
    /// <summary>
    /// Prefab referense for hit particles
    /// </summary>
    //public Transform HitPrefab;
    /// <summary>
    /// LayerMask indicating what to hit
    /// </summary>
    [SerializeField]
    public LayerMask WhatToHit;

    protected SimpleCollider Collider;

    // Use this for initialization
    protected void Start() {
        SetupSimpleCollider();

        //Begin lifetime countdown
        Invoke("MaxLifeExceededDestroy", MaxLifetime);
    }

    /// <summary>
    /// Setup the SimpleCollider with the data it needs
    /// </summary>
    private void SetupSimpleCollider() {
        //Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += HitTarget;
        Collider.Initialize(WhatToHit, TargetDirection, TargetPos, MoveSpeed, GameConstants.Layer_ObstacleThrough);
    }

    /// <summary>
    /// Bullets have a killswitch where they get destroyed no maatter what after x seconds.
    /// This helps cleanup any "stuck" bullets for whatever reason - I've seen a bullet here or there and not sure why at the moment
    /// </summary>
    protected void MaxLifeExceededDestroy() {
        if (ExplodeOnDeath) {
            GenerateEffect();
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// Set the layermask
    /// </summary>
    /// <param name="parentLayerMask"></param>
    public void SetLayerMask(LayerMask parentLayerMask) {
        WhatToHit = parentLayerMask;
    }

    /// <summary>
    /// Tell the update statement wwhere to move the bullet.
    /// </summary>
    /// <param name="targetPos"></param>
    /// <param name="targetNormal"></param>
    public void Fire(Vector3 targetPos, Vector3 targetNormal) {
        TargetPos = targetPos;
        TargetNormal = targetNormal;
        TargetDirection = TargetPos;
    }

    /// <summary>
    /// Once the bullet leaves the Cameras viewport destroy it
    /// </summary>
    void OnBecameInvisible() {
        Destroy(gameObject);
    }

    // This is for AOE Damage
    protected void Apply(Vector3 v, Collider2D c) {
        var lifeform = c.transform.GetComponent<BaseLifeform>();
        if (lifeform != null) {
            print("hit lifeform: " + lifeform.gameObject.name + " and did " + Damage + " damage");
            lifeform.Damage(Damage);
        }
    }

    protected void GenerateEffect() {
        var clone = Instantiate(ImpactEffect, transform.position, transform.rotation);
        clone.GetComponent<SpriteRenderer>().sortingOrder = 2;
        clone.GetComponent<DeathTimer>().TimeToLive = 0.25f;
        clone.GetComponent<Animator>().SetBool("Invoke", true);
        // Set the optional AOE damage controller if it has one
        var aoe = clone.GetComponent<AoeDamageController>();
        if(aoe != null) {
            // AOE damage should not be 100% of the initial damage so just give off 75% of it
            aoe.Damage = Damage * 0.75f;
        }
        AudioManager.Instance.playSound(string.IsNullOrEmpty(ImpactSound) ? "SmallExplosion" : ImpactSound);
    }

    protected abstract void Move();

    protected abstract void HitTarget(Vector3 hitPos, Collider2D hitObject);
}
