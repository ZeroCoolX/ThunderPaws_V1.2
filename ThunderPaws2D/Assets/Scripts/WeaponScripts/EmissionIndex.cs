    using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionIndex : AbstractWeapon {
    /// <summary>
    /// Reference to the LineRenderer that is actually our visual laser.
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
    /// How much damage we do per EmissionIndexConfig.DamageInterval of a second
    /// </summary>
    private float _damagePiece;
    /// <summary>
    /// Allows for intermitten damaging of object so they flicker.
    /// It looks cooler than solid white continuous damage
    /// </summary>
    private float _timeElapsedSinceLastDamage;
    private bool _holdingDownFire;
    /// <summary>
    /// Only subtract ammo 5 per second
    /// </summary>
    private float _ammoDepleteTime;
    /// <summary>
    /// How long in between firings we should deplete ammo
    /// </summary>
    private float _ammoDepleteDelay = 1f;
    /// <summary>
    /// Indicates the laser sound is already playing
    /// </summary>
    private bool _laserSoundPlaying = false;
    private float _damageModifier = 10f;

    private void Start() {
        base.Start();
        // Calculate damage per interval
        _damagePiece = Damage / _damageModifier;

        _ultLaser = transform.Find("UltLaser").GetComponent<LineRenderer>();
        if(_ultLaser == null) {
            throw new MissingComponentException("There was no ult laser found on this weapon");
        }
    }

    private void Update() {
        if (HasAmmo) {
            CheckAmmo();
        }

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

        // Check if player is holding the fire button
        var rightTrigger = Input.GetAxis(Player.JoystickId + GameConstants.Input_RTrigger);
        if (Input.GetKeyDown(InputManager.Instance.Fire) || rightTrigger > WeaponConfig.TriggerFireThreshold || _holdingDownFire) {
            _holdingDownFire = true;
            // Player the laser
            if (!_currentLaser.enabled) {
                _currentLaser.enabled = true;
            }
            if (!_laserSoundPlaying) {
                _laserSoundPlaying = true;
                AudioManager.Instance.playSound(GameConstants.Audio_EmissionIndexShot);
            }
            GenerateLaser();
        }
        if (Input.GetKeyUp(InputManager.Instance.Fire) || (JoystickManagerController.Instance.ConnectedControllers() > 0 && rightTrigger == 0)) {
            _holdingDownFire = false;
            // Stop the laser
            _currentLaser.enabled = false;
            _laserSoundPlaying = false;
            AudioManager.Instance.stopSound(GameConstants.Audio_EmissionIndexShot);
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

        Vector2 target = Vector2.zero;

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
                            AudioManager.Instance.playSound(GameConstants.Audio_EmissionIndexImpact);
                            if (lifeform.Damage(_damagePiece)) {
                                // increment the stats for whoever shot the bullet
                                GameStatsManager.Instance.AddBaddie(Player.PlayerNumber);
                            }
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
            RaycastHit2D[] collisions = Physics2D.RaycastAll(firePointPosition, directionInput, EmissionIndexConfig.MaxLaserLength, WhatToHit);
            if (collisions.Length == 0) {
                print("3 Didn't hit anything so just do max length in direction pointing : " + directionInput);
                // We didn't hit anything so just play the non-ult max distance
                var endpointVector = directionInput * EmissionIndexConfig.MaxLaserLength;
                target = FirePoint.position + new Vector3(endpointVector.x, endpointVector.y, 0f);
            } else {

                for (var i = 0; i < collisions.Length; ++i) {
                    var collision = collisions[i];
                    // Special short sircuit scenario where we hit something - that something was Obstacle-Through tagged, and this was NOT the only collision in the array
                    if (collision.collider.tag == GameConstants.Tag_ObstacleThrough) {
                        continue;
                    }
                    // We're within the max distance so hit the object
                    var distanceFromTarget = Vector2.Distance(collision.collider.transform.position, FirePoint.position);
                    var endpointVector = directionInput * distanceFromTarget;
                    target = FirePoint.position + new Vector3(endpointVector.x, endpointVector.y, 0f);
                    if (Time.time > _timeElapsedSinceLastDamage) {
                        _timeElapsedSinceLastDamage = Time.time + EmissionIndexConfig.DamageInterval;
                        // If we hit a lifeform damage it - otherwise move on
                        var lifeform = collision.transform.GetComponent<BaseLifeform>();
                        if (lifeform != null) {
                            print("hit lifeform: " + lifeform.gameObject.name + " and did " + Damage + " damage");
                            AudioManager.Instance.playSound(GameConstants.Audio_EmissionIndexImpact);
                            if (lifeform.Damage(_damagePiece)) {
                                // increment the stats for whoever shot the bullet
                                GameStatsManager.Instance.AddBaddie(Player.PlayerNumber);
                            }
                        }
                    }
                    print(" Hit something so make THAT its target");
                }
            }
        }

        // Set the laser properties
        _currentLaser.SetPosition(0, FirePoint.position);
        if(target == Vector2.zero) {
            print("Didn't hit anything so just do max length in direction pointing : " + directionInput);
            // We didn't hit anything so just play the non-ult max distance
            var endpointVector = directionInput * EmissionIndexConfig.MaxLaserLength;
            target = FirePoint.position + new Vector3(endpointVector.x, endpointVector.y, 0f);
        }
        _currentLaser.SetPosition(1, target);

        if (Time.time >= _ammoDepleteTime) {
            _ammoDepleteTime = Time.time + _ammoDepleteDelay;
            Ammo -= 5;
            PlayerHudManager.Instance.GetPlayerHud(Player.PlayerNumber).SetAmmo(Ammo);
        }
    }

    private void OnDestroy() {
        AudioManager.Instance.stopSound(GameConstants.Audio_EmissionIndexShot);
    }
}
