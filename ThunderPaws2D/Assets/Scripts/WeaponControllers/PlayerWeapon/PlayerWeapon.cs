using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : AbstractWeapon {
    /// <summary>
    /// Amount to shake camera by
    /// </summary>
    [Header("Camera Attributes")]
    public float CamShakeAmount = 0.025f;
    /// <summary>
    /// Length of time to shake camera
    /// </summary>
    public float CamShakeLength = 0.1f;
    /// <summary>
    /// Camera shake reference
    /// </summary>
    //private CameraShake _camShake;
    /// <summary>
    /// LayuerMask indicating what to hit
    /// </summary>
    public LayerMask WhatToHit;

    /// <summary>
    /// Need a reference to the player this weapon is aattached to so we can get the direction input
    /// </summary>
    private Player _player;

    protected void Start() {
        base.Start();
        _player = transform.parent.parent.GetComponent<Player>();
        if(_player == null) {
            throw new MissingComponentException("This is massively bad... No Player.cs found on the Player");
        }
        //_camShake = GameMaster.Instance.GetComponent<CameraShake>();
        //if (_camShake == null) {
        //    Debug.LogError("Weapon.cs: No CameraShake found on game master");
        //    throw new MissingComponentException();
        //}
    }

    private void Update() {
        if (FireRate == 0) {//Single fire
            if (Input.GetButtonDown("Fire1")) {
                Shoot();
            }
        } 
        //else if (IsBurst) {
        //    if (Input.GetButtonDown("Fire1") && Time.time > _timeToFire) {
        //        //Update time to fire
        //        _timeToFire = Time.time + FireDelay / FireRate;
        //        Invoke("Shoot", 0f);
        //        Invoke("Shoot", 0.025f);
        //        Invoke("Shoot", 0.05f);
        //    }
        //} else {//Automatic fire is currently deprecated since its way too OP
        //    if (Input.GetButton("Fire1") && Time.time > _timeToFire) {
        //        //Update time to fire
        //        _timeToFire = Time.time + FireDelay / FireRate;
        //        Shoot();
        //    }
        //}
    }

    /// <summary>
    /// Fire projectile from origin to mouse position
    /// </summary>
    private void Shoot() {
        Vector2 directionInput = _player.GetDirectionalInput() * 20;
        //Store bullet origin spawn popint (A)
        Vector2 firePointPosition = new Vector2(FirePoint.position.x, FirePoint.position.y);
        //Collect the hit data - distance and direction from A -> B
        RaycastHit2D shot = Physics2D.Raycast(firePointPosition, (Mathf.Sign(directionInput.x) * Vector2.right * 50), 100, WhatToHit);

        //Generate bullet effect
        if (/*IsBurst || */Time.time >= TimeToSpawnEffect) {
            //Bullet effect position data
            Vector3 hitPosition;
            Vector3 hitNormal;

            //Precalculate so if we aren't shooting at anything at least the normal is correct
            //Arbitrarily laarge number so the bullet trail flys off the camera
            hitPosition = (Mathf.Sign(directionInput.x) * Vector2.right) * 100;
            if (shot.collider != null) {
                //If we most likely hit something store the normal so the particles make sense when they shoot out
                hitNormal = shot.normal;
                hitPosition = shot.point;
            } else {
                //Rediculously huge so we can use it as a sanity check for the effect
                hitNormal = new Vector3(999, 999, 999);
            }

            //Actually instantiate the effect
            GenerateEffect(hitPosition, hitNormal, WhatToHit, "PLAYER_PROJECTILE");//PBP = PlayerBulletProjectile
            GenerateCamShake();
            TimeToSpawnEffect = Time.time + 1 / EffectSpawnRate;
        }
    }

    /// <summary>
    /// Generate camera shake
    /// </summary>
    private void GenerateCamShake() {
        //Generate camera shake
        //_camShake.Shake(CamShakeAmount, CamShakeLength);
        //TODO: generate audio
    }

}