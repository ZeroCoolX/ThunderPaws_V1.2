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
    /// REference to the LineRendere that looks different for the ultimate
    /// </summary>
    private LineRenderer _ultLaser;
    /// <summary>
    /// Instead of constantly asking which laser should be used, we just set this one to 
    /// either the default or ult one based off UltMode status
    /// </summary>
    private LineRenderer _currentLaser;
    /// <summary>
    /// Maximum distance we allow the laser to be shot out
    /// </summary>
    private float _maxLaserLength = 10f;

    /// <summary>
    /// Maximum distance we allow the laser to be shot out when in ult mode
    /// </summary>
    private float _maxUltLaserLength = 15f;

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

    private bool _triggerPressed;
    private bool _holdingQueued;

    /// <summary>
    /// Indicates the user is holding down the fire button
    /// </summary>
    private bool _holding;

    private void Start() {
        base.Start();
        _damagePiece = Damage / 10f;
        _ultLaser = transform.Find("UltLaser").GetComponent<LineRenderer>();
        if(_ultLaser == null) {
            throw new MissingComponentException("There was no ult laser found on this weapon");
        }
    }

    private void Update() {
        WeaponAnimator.SetBool("UltMode", UltMode);
        if (UltMode) {
            _currentLaser = _ultLaser;
        }else {
            if (_ultLaser.enabled) {
                _ultLaser.enabled = false;
            }
            _currentLaser = Laser;
        }
            // Player is holding the button
            var rightTrigger = Input.GetAxis(Player.JoystickId + GameConstants.Input_RTrigger);
            if (Input.GetKeyDown(InputManager.Instance.Fire) || rightTrigger > WeaponConfig.TriggerFireThreshold || _holding) {
                _holding = true;
                // Player the laser!!!
                if (!_currentLaser.enabled) {
                _currentLaser.enabled = true;
                }
                GenerateLaser();
            }
            if (Input.GetKeyUp(InputManager.Instance.Fire) || rightTrigger == 0) {
                _holding = false;
            // Stop the laser
            _currentLaser.enabled = false;
            }
    }

    private void GenerateLaser() {
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

        Vector2 target;

        if (UltMode) {
            //Collect the hit data - distance and direction from A -> B
            RaycastHit2D[] collisions = Physics2D.RaycastAll(firePointPosition, directionInput, _maxUltLaserLength, WhatToHit);
            if (collisions.Length > 0) {
                if (Time.time > _timeElapsedSinceLastDamage) {
                    _timeElapsedSinceLastDamage = Time.time + _damageInterval;
                    foreach (var collision in collisions) {
                        //IF we hit a lifeform damage it - otherwise move on
                        var lifeform = collision.transform.GetComponent<BaseLifeform>();
                        if (lifeform != null) {
                            print("ULT MODE : hit lifeform: " + lifeform.gameObject.name + " and did " + Damage + " damage");
                            lifeform.Damage(_damagePiece);
                        }
                    }
                }
            }
            print("ULT MODE : Didn't hit anything so just do max length in direction pointing : " + directionInput);
            var endpointVector = directionInput * _maxUltLaserLength;
            target = FirePoint.position + new Vector3(endpointVector.x, endpointVector.y, 0f);
        }else {
            // Only collect the first collision if we're not in ult mode
            RaycastHit2D shot = Physics2D.Raycast(firePointPosition, directionInput, _maxLaserLength, WhatToHit);
            // We collided with something!
            if (shot.collider != null) {
                // We're within the max distance so hit that thing
                var distanceFromTarget = Vector2.Distance(shot.collider.transform.position, FirePoint.position);
                var endpointVector = directionInput * distanceFromTarget;
                target = FirePoint.position + new Vector3(endpointVector.x, endpointVector.y, 0f);
                if (Time.time > _timeElapsedSinceLastDamage) {
                    _timeElapsedSinceLastDamage = Time.time + _damageInterval;
                    //IF we hit a lifeform damage it - otherwise move on
                    var lifeform = shot.transform.GetComponent<BaseLifeform>();
                    if (lifeform != null) {
                        print("hit lifeform: " + lifeform.gameObject.name + " and did " + Damage + " damage");
                        lifeform.Damage(_damagePiece);
                    }
                }
                print("Hit something so make THAT its target");
            } else {
                print("Didn't hit anything so just do max length in direction pointing : " + directionInput);
                var endpointVector = directionInput * _maxLaserLength;
                target = FirePoint.position + new Vector3(endpointVector.x, endpointVector.y, 0f);
            }
        }

        //Laser
        _currentLaser.SetPosition(0, FirePoint.position);
        _currentLaser.SetPosition(1, target);
        if (HasAmmo) {
            Ammo -= 1;
        }
    }
}
