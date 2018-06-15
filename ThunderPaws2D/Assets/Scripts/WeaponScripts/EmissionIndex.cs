using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionIndex : AbstractWeapon {
    /// <summary>
    /// Reference to the LineRenderer that is actually our visual laser.
    /// Set on the object publicly
    /// </summary>
    public LineRenderer Laser;
    /// <summary>
    /// Reference to the LineRendere for the ultimate which is initated at
    /// runtime
    /// </summary>
    private LineRenderer _ultLaser;
    /// <summary>
    /// Holds the prefab for the current laser (default or ult)
    /// </summary>
    private LineRenderer _currentLaser;
    /// <summary>
    /// We must keep a reference to the the direction input because we want the laser to 
    /// follow the players movement
    /// </summary>
    private Vector2 _directionLastFrame;
    /// <summary>
    /// How much damage we do per EmissionIndexConfig.DamageInterval of a second
    /// </summary>
    private float _damagePiece;
    /// <summary>
    /// Allows for intermitten damaging of object so they flicker.
    /// It looks cooler than solid white continuous damage
    /// </summary>
    private float _timeElapsedSinceLastDamage;
    /// <summary>
    /// Indicates the user is holding down the fire button
    /// </summary>
    private bool _holding;

    private void Start() {
        base.Start();
        // Calculate damage per interval
        _damagePiece = Damage / 10f;

        // Ensure there is a seperate Ultimate line renderer
        _ultLaser = transform.Find("UltLaser").GetComponent<LineRenderer>();
        if(_ultLaser == null) {
            throw new MissingComponentException("There was no ult laser found on this weapon");
        }
    }

    private void Update() {
        WeaponAnimator.SetBool("UltMode", UltMode);

        // Set the laser type based off mode
        if (UltMode) {
            _currentLaser = _ultLaser;
        } else {
            if (_ultLaser.enabled) {
                _ultLaser.enabled = false;
            }
            _currentLaser = Laser;
        }

        // Player is holding the fire button
        var rightTrigger = Input.GetAxis(Player.JoystickId + GameConstants.Input_RTrigger);
        if (Input.GetKeyDown(InputManager.Instance.Fire) || rightTrigger > WeaponConfig.TriggerFireThreshold || _holding) {
            _holding = true;
            // Player the laser
            if (!_currentLaser.enabled) {
                _currentLaser.enabled = true;
            }
            GenerateLaser();
        }
        if (Input.GetKeyUp(InputManager.Instance.Fire) || (JoystickManagerController.Instance.ConnectedControllers() > 0 && rightTrigger == 0)) {
            _holding = false;
            // Stop the laser
            _currentLaser.enabled = false;
        }
    }

    /// <summary>
    /// Enable the LineRenderer and track the FirePoint position and player rotation/InputDirection
    /// so the laser follows the player
    /// </summary>
    private void GenerateLaser() {
        Vector2 directionInput = Player.DirectionalInput;

        // DirectionInput compensations
        var yAxis = directionInput.y;
        if (((yAxis > 0.3 && yAxis < 0.8)) || (Player.DirectionalInput == new Vector2(1f, 1f) || Player.DirectionalInput == new Vector2(-1f, 1f))) {
            directionInput = (Vector2.up + (Player.FacingRight ? Vector2.right : Vector2.left)).normalized;
        } else if (yAxis > 0.8) {
            directionInput = Vector2.up;
        } else {
            directionInput = Player.FacingRight ? Vector2.right : Vector2.left;
        }

        //Store bullet origin spawn point
        Vector2 firePointPosition = new Vector2(FirePoint.position.x, FirePoint.position.y);

        Vector2 target;

        // The ult mode allows the laser to always display at max length and collide with any object
        // in its path damaging any/all of them
        if (UltMode) {
            //Collect the hit data - distance and direction from A -> B
            RaycastHit2D[] collisions = Physics2D.RaycastAll(firePointPosition, directionInput, EmissionIndexConfig.MaxUltLaserLength, WhatToHit);
            if (collisions.Length > 0) {
                if (Time.time > _timeElapsedSinceLastDamage) {
                    _timeElapsedSinceLastDamage = Time.time + EmissionIndexConfig.DamageInterval;
                    foreach (var collision in collisions) {
                        // If we hit a lifeform damage it - otherwise move on
                        var lifeform = collision.transform.GetComponent<BaseLifeform>();
                        if (lifeform != null) {
                            print("ULT MODE : hit lifeform: " + lifeform.gameObject.name + " and did " + Damage + " damage");
                            lifeform.Damage(_damagePiece);
                        }
                    }
                }
            }
            print("ULT MODE : Didn't hit anything so just do max length in direction pointing : " + directionInput);
            // Ult mode dictates we always display the max distance
            var endpointVector = directionInput * EmissionIndexConfig.MaxUltLaserLength;
            target = FirePoint.position + new Vector3(endpointVector.x, endpointVector.y, 0f);
        }else {
            // Only collect the first collision and stop the linerenderer there
            RaycastHit2D shot = Physics2D.Raycast(firePointPosition, directionInput, EmissionIndexConfig.MaxLaserLength, WhatToHit);
            // We collided with something
            if (shot.collider != null) {
                // We're within the max distance so hit the object
                var distanceFromTarget = Vector2.Distance(shot.collider.transform.position, FirePoint.position);
                var endpointVector = directionInput * distanceFromTarget;
                target = FirePoint.position + new Vector3(endpointVector.x, endpointVector.y, 0f);
                if (Time.time > _timeElapsedSinceLastDamage) {
                    _timeElapsedSinceLastDamage = Time.time + EmissionIndexConfig.DamageInterval;
                    // If we hit a lifeform damage it - otherwise move on
                    var lifeform = shot.transform.GetComponent<BaseLifeform>();
                    if (lifeform != null) {
                        print("hit lifeform: " + lifeform.gameObject.name + " and did " + Damage + " damage");
                        lifeform.Damage(_damagePiece);
                    }
                }
                print("Hit something so make THAT its target");
            } else {
                print("Didn't hit anything so just do max length in direction pointing : " + directionInput);
                // We didn't hit anything so just play the non-ult max distance
                var endpointVector = directionInput * EmissionIndexConfig.MaxLaserLength;
                target = FirePoint.position + new Vector3(endpointVector.x, endpointVector.y, 0f);
            }
        }

        // Set the laser properties
        _currentLaser.SetPosition(0, FirePoint.position);
        _currentLaser.SetPosition(1, target);

        if (HasAmmo) {
            Ammo -= 1;
        }
    }
}
