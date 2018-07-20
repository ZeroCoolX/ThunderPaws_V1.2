using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : AbstractWeapon {
    /// <summary>
    /// Debug only - keeps track of shot raycasts so we can print them to the screen if wanted
    /// </summary>
    private Vector2[] _debugBlastRotations = new Vector2[7];
    /// <summary>
    /// The shotgun is semi-automatic - therefor the user MUST release the trigger in between shots
    /// </summary>
    private bool _triggerLetGo = true;
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
    private float _maxShotDelay;
    private float _maxTimeBetweenShots = 0.2f;


    private void Update() {
        var rightTrigger = Input.GetAxis(Player.JoystickId + GameConstants.Input_RTrigger);
        // This checks if the player released the trigger in between shots - because the shotgun is not full auto
        if (!_triggerLetGo) {
            if(rightTrigger <= WeaponConfig.TriggerFireThreshold && !Input.GetKey(InputManager.Instance.Fire)) {
                _triggerLetGo = true;
            }
        }

        // Degub only rendering the raycasts to see them in action
        for (var i = 1; i < _debugBlastRotations.Length; ++i) {
            Debug.DrawRay(_debugBlastRotations[0], _debugBlastRotations[i] * ShotgunConfig.RayLength, Color.green);
        }

        // Only fire if we're not already holding the trigger
        if (_triggerLetGo && (Input.GetKeyDown(InputManager.Instance.Fire) || rightTrigger > WeaponConfig.TriggerFireThreshold) && Time.time > _maxShotDelay) {
            _maxShotDelay = Time.time + _maxTimeBetweenShots;
            _triggerLetGo = false;
            CalculateShot();
            ApplyRecoil();
            AudioManager.playSound(GameConstants.Audio_Shotgun);
        }
        if (HasAmmo) {
            AmmoCheck();
        }
        WeaponAnimator.SetBool("UltModeActive", UltMode);

        // Temporary for now - set the damage up when ultimate is active
        if (UltMode && Damage < 100) {
            Damage = 100;
        }else {
            if(Damage != 25) {
                Damage = 25;
            }
        }
    }

    /// <summary>
    /// Implementation specific override.
    /// Apply the recoil animation and reset the weapon position
    /// </summary>
    protected override void ApplyRecoil() {
        WeaponAnimator.SetBool("ApplyRecoil", true);
        StartCoroutine(ResetWeaponPosition());
    }

    /// <summary>
    /// Generate blast effect prefab.
    /// This had to be independent because the weapon recoil animation is independent of the blast animation.
    /// Also the blast is a seperate prefab so it stays in its position and doesn't continue tracking the gun once fired
    /// </summary>
    /// <param name="shotPos"></param>
    /// <param name="shotNormal"></param>
    /// <param name="whatToHit"></param>
    protected void GenerateBlastEffect(Vector3 shotPos) {
        // Spawn the blast effect so it stays in the position it spawns at and doesn't follow the gun - because that looks super weird
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bulletCount"></param>
    protected override void CalculateShot(int bulletCount = 1) {
        if (Time.time >= TimeToSpawnEffect) {
            Vector2 dir = Player.DirectionalInput;
            // Preprocessing of which direction our shotgun is pointing which will modify the degrees at which
            // the 5 raycast shotgun blasts are fired
            var preCalcYaxis = dir.y;
            if (((preCalcYaxis > 0.3 && preCalcYaxis < 0.8)) || (Player.DirectionalInput == new Vector2(1f, 1f) || Player.DirectionalInput == new Vector2(-1f, 1f))) {
                dir = (Vector2.up + (Player.FacingRight ? Vector2.right : Vector2.left)).normalized;
            } else if (preCalcYaxis > 0.8) {
                dir = Vector2.up;
            } else {
                dir = Player.FacingRight ? Vector2.right : Vector2.left;
            }
            GenerateCameraShake();
            GenerateBlastEffect(dir);

            // Generate 5 raycasts at varying degrees
            // Rotation around the z axis
            var i = 1;
            var collisionMap = new Dictionary<string, Collider2D>();
            foreach(var rotation in ShotgunConfig.BlastRotations) {
                var finalRotation = rotation * (Player.FacingRight ? 1 : -1);

                // Get the direction the player is pointing
                Vector2 directionInput = Player.DirectionalInput;
                // Concert the degrees to radians 
                var rads = Mathf.Deg2Rad * finalRotation;

                var yAxis = directionInput.y;
                var xDir = 1.0f * (Player.FacingRight ? 1 : -1);
                var yDir = 0f;
                if (((yAxis > 0.3 && yAxis < 0.8)) || (Player.DirectionalInput == new Vector2(1f, 1f) || Player.DirectionalInput == new Vector2(-1f, 1f))) {
                    yDir = 0.5f;
                    xDir = 0.5f * (Player.FacingRight ? 1 : -1);
                } else if (yAxis > 0.8) {
                    yDir = 1.0f;
                    xDir = 0.0f * (Player.FacingRight ? 1 : -1);
                }

                // Necessary to rotate the raycast from its initial position to its desired final position
                Vector2 rotatedDirection = new Vector2(
                    xDir*Mathf.Cos(rads) - yDir*Mathf.Sin(rads),
                    xDir*Mathf.Sin(rads) + yDir*Mathf.Cos(rads)
                    );

                // Store bullet origin spawn point (A)
                Vector2 firePointPosition = new Vector2(FirePoint.position.x, FirePoint.position.y);
                _debugBlastRotations[0] = firePointPosition;
                // Collect the hit data - distance and direction from A -> B
                RaycastHit2D shot = Physics2D.Raycast(firePointPosition, rotatedDirection, ShotgunConfig.RayLength, WhatToHit);

                // Check if it hit anything
                // We want to allow bullets to pass throught obstacles that the player can pass through
                if (shot.collider != null) {
                    print("We have collided with something i: " + i);
                    Collider2D outCollider;
                    if (!collisionMap.TryGetValue(shot.collider.gameObject.name, out outCollider)) {
                        collisionMap.Add(shot.collider.gameObject.name, shot.collider);
                    }
                }

                // Store the current direction in the debug array so we can print it later
                _debugBlastRotations[i] = rotatedDirection;
                ++i;

                if (UltMode) {
                    Transform bulletInstance = Instantiate(UltBulletPrefab, firePointPosition, Quaternion.identity) as Transform;
                    //Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
                    var projectile = bulletInstance.GetComponent<BulletProjectile>();

                    //Set layermask of parent (either player or baddie)
                    projectile.SetLayerMask(WhatToHit);
                    projectile.gameObject.layer = LayerMask.NameToLayer(GameConstants.Layer_PlayerProjectile);
                    projectile.Damage = Damage;
                    projectile.MoveSpeed = (UnityEngine.Random.Range(12, 20));
                    projectile.MaxLifetime = MaxLifetime;
                    projectile.OptionalGravity = -25.08f;
                    projectile.Fire(rotatedDirection, rotatedDirection.normalized);
                }
            }

            foreach (var collider in collisionMap) {
                HitTarget(collider.Value);
            }

            TimeToSpawnEffect = Time.time + 1 / EffectSpawnRate;
            if (HasAmmo) {
                Ammo -= 1;
                print("SettingAmmo");
                PlayerHudManager.Instance.GetPlayerHud(Player.PlayerNumber).SetAmmo(Ammo);
            }else {
                PlayerHudManager.Instance.GetPlayerHud(Player.PlayerNumber).SetAmmo();
            }
        }
    }

    /// <summary>
    /// Damage, Destroy and generate effects
    /// </summary>
    /// <param name="hitObject"></param>
    private void HitTarget(Collider2D hitObject) {
        print("Hit object: " + hitObject.gameObject.tag);
        // If we hit a lifeform damage it - otherwise move on
        var lifeform = hitObject.transform.GetComponent<BaseLifeform>();
        if (lifeform != null) {
            print("hit lifeform: " + lifeform.gameObject.name + " and did " + Damage + " damage");
            if (lifeform.Damage(Damage)) {
                // increment the stats for whoever shot the bullet
                GameStatsManager.Instance.AddBaddie(Player.PlayerNumber);
            }
        }
        GenerateEffect(hitObject.transform);
    }

    /// <summary>
    /// Play the shotgun  blast animation effect
    /// </summary>
    /// <param name="impactTransform"></param>
    private void GenerateEffect(Transform impactTransform) {
        // Subtract 1 from whatever the Z position is to ensure the explosion animation is played over the top
        var fixedPosition = new Vector3(impactTransform.position.x, impactTransform.position.y, impactTransform.position.z - 1);
        var clone = Instantiate(ImpactEffect, fixedPosition, impactTransform.rotation);
        clone.GetComponent<DeathTimer>().TimeToLive = 0.25f;
        clone.GetComponent<Animator>().SetBool("Invoke", true);
    }

}
