using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic needs of a lifeform.
/// Movement.
/// Take damage
/// </summary>
public abstract class BaseLifeform : MonoBehaviour {
    /// <summary>
    /// Responsibe for all collision detection with the environment and other lifeforms
    /// </summary>
    protected CollisionController2D Controller2d;

    /// <summary>
    /// Movement vector of the lifeform.
    /// Given to the Controller2d to actually move
    /// </summary>
    protected Vector3 Velocity;
    /// <summary>
    /// Public accessor for Velocity since scripts that need to pad their position based off
    /// lifeforms movement need access to it.
    /// </summary>
    public Vector3 GetVelocity { get { return Velocity; } }

    /// <summary>
    /// Gravity value that is applied on this lifeform
    /// </summary>
    protected float Gravity;

    /// <summary>
    /// Trigger that extending classes use to indicate we shuold flash the sprite
    /// white for 1 frame
    /// </summary>
    private bool _flashDamage = false;

    /// <summary>
    /// Base implementation of movement is nothing.
    /// If the lifeform moves the extending must override this.
    /// </summary>
    protected virtual void Move() { }

    /// <summary>
    /// Base implementation is to flash the sprite.
    /// Returns true if the health is less than or equal to 0
    /// Returns false all other times
    /// </summary>
    public abstract bool Damage(float damage);

    /// <summary>
    /// Assign the CollisionController2d instance attached to the lifeform
    /// </summary>
    protected void Awake() {
        Controller2d = GetComponent<CollisionController2D>();
        if(Controller2d == null) {
            throw new MissingComponentException("BaseLifeform : Missing CollisionController2d");
        }
    }

    protected void Update() {
        if (_flashDamage) {
            GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0.8f);
            _flashDamage = false;
        } else {
            GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0f);
        }
    }


    /// <summary>
    /// Apply the gravity value to the Y component of the lifeforms velocity
    /// </summary>
    protected void ApplyGravity() {
        Velocity.y += Gravity * Time.deltaTime;
    }

    /// <summary>
    /// Indicates we should flash the sprite white for 1 frame
    /// </summary>
    protected void ActivateFlash() {
        _flashDamage = true;
    }
}
