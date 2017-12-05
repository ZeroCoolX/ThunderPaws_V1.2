using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractProjectile : MonoBehaviour {
    /// <summary>
    /// Sprite animatio to play when the bullet impacts
    /// </summary>
    public Transform ImpactEffect;

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

    // Use this for initialization
    protected void Start() {
        //Validate the hit prefab is set
        //if (HitPrefab == null) {
        //    Debug.LogError("No HitPrefab was found on bullet");
        //    throw new UnassignedReferenceException();
        //}
        Invoke("MaxLifeExceededDestroy", MaxLifetime);
    }

    /// <summary>
    /// Bullets have a killswitch where they get destroyed no maatter what after x seconds.
    /// This helps cleanup any "stuck" bullets for whatever reason - I've seen a bullet here or there and not sure why at the moment
    /// </summary>
    protected void MaxLifeExceededDestroy() {
        Destroy(gameObject);
    }

    /// <summary>
    /// Once the bullet leaves the Cameras viewport destroy it
    /// </summary>
    void OnBecameInvisible() {
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

    protected abstract void Move();

    protected abstract void HitTarget(Vector3 hitPos, Collider2D hitObject);
}
