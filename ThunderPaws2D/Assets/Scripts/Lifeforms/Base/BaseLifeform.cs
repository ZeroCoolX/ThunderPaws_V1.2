using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic needs of a lifeform.
/// Movement.
/// Take damage
/// </summary>
public abstract class BaseLifeform : MonoBehaviour {
    public Vector3 GetVelocity { get { return Velocity; } }
    public abstract bool Damage(float damage);

    protected CollisionController2D Controller2d;
    protected Vector3 Velocity;
    protected float Gravity;
    /// <summary>
    /// Base implementation of movement is nothing.
    /// If the lifeform moves the extending must override this.
    /// </summary>
    protected virtual void Move() { }

    private bool _useFlashDamage = false;



    protected void Awake() {
        Controller2d = GetComponent<CollisionController2D>();
        if(Controller2d == null) {
            throw new MissingComponentException("BaseLifeform : Missing CollisionController2d");
        }
    }

    public CollisionController2D Get2DController() {
        return Controller2d;
    }

    protected void Update() {
        if (_useFlashDamage) {
            GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0.8f);
            _useFlashDamage = false;
        } else {
            GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0f);
        }
    }

    protected void ApplyGravity() {
        Velocity.y += Gravity * Time.deltaTime;
    }

    protected void ActivateFlash() {
        _useFlashDamage = true;
    }
}
