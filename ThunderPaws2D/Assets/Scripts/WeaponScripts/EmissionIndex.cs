using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionIndex : AbstractWeapon {

    /// <summary>
    /// Reference to the LineRenderer that is actually our visual laser
    /// </summary>
    public LineRenderer Laser;

    /// <summary>
    /// Maximum distance we allow the laser to be shot out
    /// </summary>
    private float _maxLaserLength = 10f;

    /// <summary>
    /// We must keep a reference to the the direction input because we want the laser to 
    /// follow the players movement
    /// </summary>
    private Vector2 _directionLastFrame;

    /// <summary>
    /// How much damage we do per 10th of a second
    /// </summary>
    private float _damagePiece;

    private float _damageInterval = 0.1f;

    private float _timeElapsedSinceLastDamage;

    // The damage in this weapon script means that amount of damage per second since the damage is consistent
    // damagePiece = damage / 10;
    // damagePiece per 1/10th of a second = damage per second 

    /// <summary>
    /// Indicates the user is holding down the fire button
    /// </summary>
    private bool _holding;

    private void Start() {
        base.Start();
        _damagePiece = Damage / 10f;
    }

    private void Update() {
        // Player is holding the button
        if (Input.GetKeyDown(InputManager.Instance.Fire) || _holding) {
            _holding = true;
            // Player the laser!!!
            if (!Laser.enabled) {
                Laser.enabled = true;
            }
            //if(_directionLastFrame != Player.DirectionalInput) {
                CalculateShot();
            //}
        }
        if (Input.GetKeyUp(InputManager.Instance.Fire)) {
            _holding = false;
            // Stop the laser
            Laser.enabled = false;
        }
    }

    protected override void CalculateShot() {
        Vector2 directionInput = Player.DirectionalInput;

        // We were too far out
        var yAxis = directionInput.y;
        if (((yAxis > 0.3 && yAxis < 0.8)) || (Player.DirectionalInput == new Vector2(1f, 1f) || Player.DirectionalInput == new Vector2(-1f, 1f))) {
            directionInput = (Vector2.up + (Player.FacingRight ? Vector2.right : Vector2.left)).normalized;
        } else if (yAxis > 0.8) {
            directionInput = Vector2.up;
        } else {
            directionInput = Player.FacingRight ? Vector2.right : Vector2.left;
        }

        //Store bullet origin spawn popint (A)
        Vector2 firePointPosition = new Vector2(FirePoint.position.x, FirePoint.position.y);
        //Collect the hit data - distance and direction from A -> B
        RaycastHit2D shot = Physics2D.Raycast(firePointPosition, directionInput, _maxLaserLength, WhatToHit);

        Vector2 target;

        // We collided with something!
        if(shot.collider != null) {
            // We're within the max distance so hit that thing
                var distanceFromTarget = Vector2.Distance(shot.collider.transform.position, FirePoint.position);
                var endpointVector = directionInput * distanceFromTarget;
                target = FirePoint.position + new Vector3(endpointVector.x, endpointVector.y, 0f);
            if(Time.time > _timeElapsedSinceLastDamage) {
                _timeElapsedSinceLastDamage = Time.time + _damageInterval;
                //IF we hit a lifeform damage it - otherwise move on
                var lifeform = shot.transform.GetComponent<BaseLifeform>();
                if (lifeform != null) {
                    print("hit lifeform: " + lifeform.gameObject.name + " and did " + Damage + " damage");
                    lifeform.Damage(_damagePiece);
                }
            }
            //target = FirePoint.position + new Vector3(temp.x, temp.y, 0f);
            print("Hit something so make THAT its target");
        }else {
            print("Didn't hit anything so just do max length in direction pointing : " + directionInput);
            var endpointVector = directionInput * _maxLaserLength;
            target = FirePoint.position + new Vector3(endpointVector.x, endpointVector.y, 0f);
        }

        //Laser
        Laser.SetPosition(0, FirePoint.position);
        Laser.SetPosition(1, target);
        if (HasAmmo) {
            Ammo -= 1;
        }
    }

    protected override void ApplyRecoil() {
        // play laser!!!
        print("Play Laser");
    }


    protected override void GenerateShot(Vector3 shotPos, Vector3 shotNormal, LayerMask whatToHit, string layer, bool ultMode, float freeFlyDelay = 0.5F) {
        throw new NotImplementedException();
    }

}
