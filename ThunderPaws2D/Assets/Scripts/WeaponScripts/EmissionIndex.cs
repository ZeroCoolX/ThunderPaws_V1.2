    using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionIndex : AbstractWeapon {
    [Header("Line Renderer")]
    public LineRenderer Laser;
    private LineRenderer _ultLaser;
    private LineRenderer _currentLaser;

    /// <summary>
    /// This represents the actual amount of damage we take away from the object we hit's health
    /// </summary>
    private float _damagePiece;
    private float _damageRate = 10f;
    
    /// <summary>
    /// Allows for intermitten damaging of object so they flicker. It looks cooler than solid white continuous damage
    /// </summary>
    private float _timeElapsedSinceLastDamage;
    private bool _holdingDownFire;

    private float _ammoDepleteTime;
    private float _ammoDepleteDelay = 1f;

    private bool _laserSoundPlaying = false;



    private new void Start() {
        base.Start();

        // Calculate damage per interval
        _damagePiece = Damage / _damageRate;

        _ultLaser = transform.Find("UltLaser").GetComponent<LineRenderer>();
        if (_ultLaser == null) {
            throw new MissingComponentException("There was no ult laser found on this weapon");
        }
    }

    private void Update() {
        if (HasAmmo) {
            CheckAmmo();
        }

        WeaponAnimator.SetBool("UltMode", UltMode);

        ResolveLaserType();

        var fireButton = Input.GetAxis(Player.JoystickId + GameConstants.Input_RTrigger);
        if (Input.GetKeyDown(InputManager.Instance.Fire) || fireButton > WeaponConfig.TriggerFireThreshold || _holdingDownFire) {
            PrepareLaser();
            GenerateLaser();
        }
        if (Input.GetKeyUp(InputManager.Instance.Fire) || (JoystickManagerController.Instance.ConnectedControllers() > 0 && fireButton == 0)) {
            _holdingDownFire = false;
            _currentLaser.enabled = false;
            _laserSoundPlaying = false;
            AudioManager.Instance.stopSound(GameConstants.Audio_EmissionIndexShot);
        }
    }

    private void ResolveLaserType() {
        if (UltMode) {
            _currentLaser = _ultLaser;
        } else {
            if (_ultLaser.enabled) {
                _ultLaser.enabled = false;
            }
            _currentLaser = Laser;
        }
    }

    private void HandleShootingInput(float fireButton) {
        // Check if player is holding the fire button
        if (Input.GetKeyDown(InputManager.Instance.Fire) || fireButton > WeaponConfig.TriggerFireThreshold || _holdingDownFire) {
            PrepareLaser();
            GenerateLaser();
        }
    }

    private void PrepareLaser() {
        _holdingDownFire = true;
        if (!_currentLaser.enabled) {
            _currentLaser.enabled = true;
        }
        if (!_laserSoundPlaying) {
            _laserSoundPlaying = true;
            AudioManager.Instance.playSound(GameConstants.Audio_EmissionIndexShot);
        }
    }

    private bool PointingWeaponAtAngle(float yAxis) {
        return ((yAxis > 0.3 && yAxis < 0.8)) || (Player.DirectionalInput == new Vector2(1f, 1f) || Player.DirectionalInput == new Vector2(-1f, 1f));
    }

    /// <summary>
    /// The ult mode allows the laser to always display at max length and collide with any object in its path damaging any/all of them
    /// </summary>
    private Vector2 GenerateUltLaser(Vector2 directionInput) {
        Vector2 target = Vector2.zero;
        Vector2 bulletOriginPosition = new Vector2(FirePoint.position.x, FirePoint.position.y);

        RaycastHit2D[] collisions = Physics2D.RaycastAll(bulletOriginPosition, directionInput, EmissionIndexConfig.MaxUltLaserLength, WhatToHit);
        if (collisions.Length > 0) {
            if (Time.time > _timeElapsedSinceLastDamage) {
                _timeElapsedSinceLastDamage = Time.time + EmissionIndexConfig.DamageInterval;
                foreach (var collision in collisions) {
                    CollideWithLifeform(collision);
                }
            }
        }

        // Ult mode dictates we always display the max distance
        var endpointVector = directionInput * EmissionIndexConfig.MaxUltLaserLength;
        target = FirePoint.position + new Vector3(endpointVector.x, endpointVector.y, 0f);

        return target;
    }

    private void CollideWithLifeform(RaycastHit2D collision) {
        var lifeform = collision.transform.GetComponent<BaseLifeform>();
        if (lifeform != null) {
            AudioManager.Instance.playSound(GameConstants.Audio_EmissionIndexImpact);

            if (lifeform.Damage(_damagePiece)) {
                GameStatsManager.Instance.AddBaddie(Player.PlayerNumber);
            }
        }
    }

    /// <summary>
    /// The default laser will stop at any obstacle or baddie it hits.
    /// It can go through OBSTACLE-THROUGH tagged objects however so a collection of
    /// everything the raycast hits is still needed - only we stop processing the hits after the first instance
    /// of something either an Object or we can hit
    /// </summary>
    /// <returns></returns>
    private Vector2 GenerateDefaultLaser(Vector2 directionInput) {
        Vector2 target = Vector2.zero;
        Vector2 bulletOriginPosition = new Vector2(FirePoint.position.x, FirePoint.position.y);

        // Only collect the first collision and stop the linerenderer there
        RaycastHit2D[] collisions = Physics2D.RaycastAll(bulletOriginPosition, directionInput, EmissionIndexConfig.MaxLaserLength, WhatToHit);
        if (collisions.Length == 0) {
            // We didn't hit anything so just play the non-ult max distance line renderer
            var endpointVector = directionInput * EmissionIndexConfig.MaxLaserLength;
            target = FirePoint.position + new Vector3(endpointVector.x, endpointVector.y, 0f);
        } else {
            // Indexed for loop is used instead of a foreach loop because we MUST gaurentee the order from first hit to last
            // Foreach loops do not gaurentee order
            for (var i = 0; i < collisions.Length; ++i) {
                var collision = collisions[i];
                
                // Special short circuit scenario where we hit something - that something was Obstacle-Through tagged, and this was NOT the only collision in the array
                if (collision.collider.tag == GameConstants.Tag_ObstacleThrough) {
                    continue;
                }

                var distanceFromTarget = Vector2.Distance(collision.collider.transform.position, FirePoint.position);
                var endpointVector = directionInput * distanceFromTarget;
                target = FirePoint.position + new Vector3(endpointVector.x, endpointVector.y, 0f);

                if (Time.time > _timeElapsedSinceLastDamage) {
                    _timeElapsedSinceLastDamage = Time.time + EmissionIndexConfig.DamageInterval;
                    CollideWithLifeform(collision);
                }
            }
        }

        return target;
    }

    /// <summary>
    /// Enable the LineRenderer and track the FirePoint position and player rotation/InputDirection
    /// so the laser follows the player
    /// </summary>
    private void GenerateLaser() {
        Vector2 directionInput = Player.DirectionalInput;

        // DirectionInput compensations
        if (PointingWeaponAtAngle(directionInput.y)) {
            directionInput = (Vector2.up + (Player.FacingRight ? Vector2.right : Vector2.left)).normalized;
        } // Player is pointing up enough to consider it straight up
        else if (directionInput.y > 0.8) {
            directionInput = Vector2.up;
        } else {
            directionInput = Player.FacingRight ? Vector2.right : Vector2.left;
        }

        Vector2 target = Vector2.zero;

        if (UltMode) {
            target = GenerateUltLaser(directionInput);
        }else {
            target = GenerateDefaultLaser(directionInput);
        }

        // Set the laser start and endpoint
        _currentLaser.SetPosition(0, FirePoint.position);
        if(target == Vector2.zero) {
            // We didn't hit anything so just play the non-ult max distance
            var endpointVector = directionInput * EmissionIndexConfig.MaxLaserLength;
            target = FirePoint.position + new Vector3(endpointVector.x, endpointVector.y, 0f);
        }
        _currentLaser.SetPosition(1, target);

        UpdateAmmo();
    }

    private void UpdateAmmo() {
        if (Time.time >= _ammoDepleteTime) {
            _ammoDepleteTime = Time.time + _ammoDepleteDelay;
            Ammo -= 3;
            PlayerHudManager.Instance.GetPlayerHud(Player.PlayerNumber).SetAmmo(Ammo);
        }
    }

    private void OnDestroy() {
        try {
            AudioManager.Instance.stopSound(GameConstants.Audio_EmissionIndexShot);
        }catch(Exception e) {
            print("Failed to stop laser sounds because the object was already destroyed.");
        }
    }
}
