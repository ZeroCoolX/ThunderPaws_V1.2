using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : AbstractWeapon {
    /// <summary>
    /// Specific blast prefab since the gun blast is independent of the fire animation
    /// </summary>
    public Transform BlastPrefab;
    /// <summary>
    /// Specific ultimate blast prefab since the gun blast is independent of the fire animation
    /// </summary>
    public Transform UltBlastPrefab;
    /// <summary>
    /// Explosive hairballs!
    /// </summary>
    public Transform UltBulletPrefab;
    /// <summary>
    /// Sprite animation to play when we hit a target.
    /// We have to specify this since we don't actually fire out any prefab bullets (in non ult mode).
    /// </summary>
    public Transform ImpactEffect;

    private bool _triggerLetGo = true;

    private float _maxShotDelay;
    private float _maxTimeBetweenShots = 0.2f;

    private int _ultModeDamage = 100;
    private int _defaultDamage = 25;

    // Debug only - keeps track of shot raycasts so we can print them to the screen if wanted
    private Vector2[] _debugBlastRotations = new Vector2[7];



    private void Update() {
        Damage = UltMode ? _ultModeDamage : _defaultDamage;

        var rightTrigger = Input.GetAxis(Player.JoystickId + GameConstants.Input_RTrigger);

        if (!_triggerLetGo) {
            if (rightTrigger <= WeaponConfig.TriggerFireThreshold && !Input.GetKey(InputManager.Instance.Fire)) {
                _triggerLetGo = true;
            }
        }

        // Degub only rendering the raycasts to see them in action
        for (var i = 1; i < _debugBlastRotations.Length; ++i) { Debug.DrawRay(_debugBlastRotations[0], _debugBlastRotations[i] * ShotgunConfig.RayLength, Color.green); }

        HandleShootingInput(rightTrigger);

        if (HasAmmo) {
            CheckAmmo();
        }
        WeaponAnimator.SetBool("UltModeActive", UltMode);
    }

    private void HandleShootingInput(float rightTrigger) {
        if (_triggerLetGo && (Input.GetKeyDown(InputManager.Instance.Fire) || rightTrigger > WeaponConfig.TriggerFireThreshold) && Time.time > _maxShotDelay) {
            _maxShotDelay = Time.time + _maxTimeBetweenShots;
            _triggerLetGo = false;
            FireShot();
            ApplyRecoil();
            AudioManager.playSound(GameConstants.Audio_Shotgun);
        }
    }

    protected override void FireShot(int bulletCount = 1) {
        if (Time.time < ShotEffectDelay) {
            return;
        }

        PreprocessBlastEffect();

        var collisionMap = PopulateCollisionMap();
        foreach (var collider in collisionMap) {
            HitTarget(collider.Value);
        }

        ShotEffectDelay = Time.time + 1 / ShotEffectSpawnRate;

        GenerateCameraShake();
        UpdateAmmo();
    }

    /// <summary>
    /// Preprocessing must be done for the blast effect because it is a seperate prefab independent of the weapon.
    /// More details exist in PrepareBlastEffectPrefab()
    /// </summary>
    private void PreprocessBlastEffect() {
        Vector2 directionInput = Player.DirectionalInput;
        var preCalcYaxis = directionInput.y;
        if (PointingWeaponAtAngle(directionInput.y)) {
            directionInput = (Vector2.up + (Player.FacingRight ? Vector2.right : Vector2.left)).normalized;
        } else if (preCalcYaxis > 0.8) {
            directionInput = Vector2.up;
        } else {
            directionInput = Player.FacingRight ? Vector2.right : Vector2.left;
        }
        PrepareBlastEffectPrefab(directionInput);
    }

    /// <summary>
    /// Generate blast effect prefab.
    /// This had to be independent because the weapon recoil animation is independent of the blast animation.
    /// Also the blast is a seperate prefab so it stays in its position and doesn't continue tracking the gun once fired
    /// </summary>
    private void PrepareBlastEffectPrefab(Vector3 shotPos) {
        // Spawn the blast effect so it stays in the position/rotation it spawns at and doesn't follow the gun - because that looks super weird
        var blastRotation = CompensateQuaternion(FirePoint.rotation);

        // Pad the X coordinate to ensure the blast is at the tip of the gun. 0.5 is an arbitrary number that seems to fit
        var paddedX = Mathf.Abs(Player.GetVelocity.x) > 3 ? (Player.GetVelocity.x * Time.deltaTime) + FirePoint.position.x + (0.5f * (Player.FacingRight ? 1 : -1)) : FirePoint.position.x;

        // Pad the Z coordinate to ensure the blast is over the top of everything
        var fixedPosition = new Vector3(paddedX, FirePoint.position.y, FirePoint.position.z - 5);

        Transform blastInstance = Instantiate(!UltMode ? BlastPrefab : UltBlastPrefab, fixedPosition, blastRotation) as Transform;

        // Account for needing to mirror the sprite
        if (Mathf.Sign(shotPos.x) < 0) {
            Vector3 theScale = blastInstance.transform.localScale;
            theScale.x *= -1;
            blastInstance.transform.localScale = theScale;
        }

        blastInstance.GetComponent<SpriteRenderer>().sortingOrder = 2;
        blastInstance.GetComponent<DeathTimer>().TimeToLive = 0.25f;
        blastInstance.GetComponent<Animator>().SetBool("Invoke", false);
    }

    private Dictionary<string, Collider2D> PopulateCollisionMap() {
        // Generate 5 raycasts at varying degrees in front of the player. Rotation around the z axis
        var i = 1;
        var collisionMap = new Dictionary<string, Collider2D>();
        foreach (var rotation in ShotgunConfig.BlastRotations) {
            var finalRotation = rotation * (Player.FacingRight ? 1 : -1);

            Vector2 rotatedDirection = CalculateRotatedRaycast(finalRotation);

            // Store bullet origin spawn point (A)
            Vector2 bulletOriginPosition = new Vector2(FirePoint.position.x, FirePoint.position.y);
            _debugBlastRotations[0] = bulletOriginPosition;

            RaycastHit2D shot = Physics2D.Raycast(bulletOriginPosition, rotatedDirection, ShotgunConfig.RayLength, WhatToHit);

            // Check if it hit anything and add it to the collision map if it does not already exist
            // We want to allow bullets to pass throught obstacles that the player can pass through
            if (shot.collider != null) {
                Collider2D outCollider;
                if (!collisionMap.TryGetValue(shot.collider.gameObject.name, out outCollider)) {
                    collisionMap.Add(shot.collider.gameObject.name, shot.collider);
                }
            }

            // Store the current direction in the debug array so we can print it later
            _debugBlastRotations[i] = rotatedDirection;
            ++i;

            if (UltMode) {
                PrepareUltShot(bulletOriginPosition, rotatedDirection);
            }
        }

        return collisionMap;
    }

    /// <summary>
    /// Helper function to compensate the raycast that is fired out to be in the same rotation as that of the player
    /// </summary>
    private Vector2 CalculateRotatedRaycast(float desiredRotation) {
        // Get the direction the player is pointing
        Vector2 directionInput = Player.DirectionalInput;
        // Convert the degrees to radians 
        var rads = Mathf.Deg2Rad * desiredRotation;

        var yAxis = directionInput.y;
        var xDir = 1.0f * (Player.FacingRight ? 1 : -1);
        var yDir = 0f;
        if (PointingWeaponAtAngle(yAxis)) {
            yDir = 0.5f;
            xDir = 0.5f * (Player.FacingRight ? 1 : -1);
        }// Player is pointing up enough to consider it straight up 
        else if (yAxis > 0.8) {
            yDir = 1.0f;
            xDir = 0.0f * (Player.FacingRight ? 1 : -1);
        }

        // Necessary to rotate the raycast from its initial position to its desired final position
        Vector2 rotatedDirection = new Vector2(
            xDir * Mathf.Cos(rads) - yDir * Mathf.Sin(rads),
            xDir * Mathf.Sin(rads) + yDir * Mathf.Cos(rads)
            );

        return rotatedDirection;
    }

    /// <summary>
    /// The Shotgun ultimate also fires out explosive hairballs!
    /// </summary>
    private void PrepareUltShot(Vector2 bulletOriginPosition, Vector2 rotatedDirection) {
        Transform bulletInstance = Instantiate(UltBulletPrefab, bulletOriginPosition, Quaternion.identity) as Transform;
        var projectile = bulletInstance.GetComponent<BulletProjectile>();
        projectile.SetLayerMask(WhatToHit);
        projectile.gameObject.layer = LayerMask.NameToLayer(GameConstants.Layer_PlayerProjectile);
        projectile.Damage = Damage;
        projectile.MoveSpeed = (UnityEngine.Random.Range(12, 20));
        projectile.MaxLifetime = MaxLifetime;
        projectile.OptionalGravity = -25.08f;
        projectile.Fire(rotatedDirection, rotatedDirection.normalized);
    }

    private void HitTarget(Collider2D hitObject) {
        var lifeform = hitObject.transform.GetComponent<BaseLifeform>();
        if (lifeform != null) {
            if (lifeform.Damage(Damage)) {
                // increment the stats for whoever shot the bullet
                GameStatsManager.Instance.AddBaddie(Player.PlayerNumber);
            }
        }
        GenerateBlastEffect(hitObject.transform);
    }

    private void GenerateBlastEffect(Transform impactTransform) {
        // Subtract 1 from whatever the Z position is to ensure the explosion animation is played over the top
        var fixedPosition = new Vector3(impactTransform.position.x, impactTransform.position.y, impactTransform.position.z - 1);

        var clone = Instantiate(ImpactEffect, fixedPosition, impactTransform.rotation);
        clone.GetComponent<DeathTimer>().TimeToLive = 0.25f;
        clone.GetComponent<Animator>().SetBool("Invoke", true);
    }

    private bool PointingWeaponAtAngle(float yAxis) {
        return ((yAxis > 0.3 && yAxis < 0.8)) || (Player.DirectionalInput == new Vector2(1f, 1f) || Player.DirectionalInput == new Vector2(-1f, 1f));
    }

    private void UpdateAmmo() {
        if (HasAmmo) {
            Ammo -= 1;
            print("SettingAmmo");
            PlayerHudManager.Instance.GetPlayerHud(Player.PlayerNumber).SetAmmo(Ammo);
        } else {
            PlayerHudManager.Instance.GetPlayerHud(Player.PlayerNumber).SetAmmo();
        }
    }

    protected override void ApplyRecoil() {
        WeaponAnimator.SetBool("ApplyRecoil", true);
        StartCoroutine(ResetWeaponPosition());
    }
}
