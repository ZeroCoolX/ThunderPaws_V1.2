using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractWeapon : MonoBehaviour {

    //******************************************* Camera properties *******************************************//
    /// <summary>
    /// Reference to the camera shake script
    /// </summary>
    protected CameraShake CamShake;
    /// <summary>
    /// Amount to shake camera by
    /// </summary>
    public float CamShakeAmount = 0.025f;
    /// <summary>
    /// Length of time to shake camera
    /// </summary>
    public float CamShakeLength = 0.1f;


    //******************************************* Animation Properties *******************************************//
    /// <summary>
    /// Weapon for the weapon
    /// </summary>
    protected Animator WeaponAnimator;


    //******************************************* Weapon Properties *******************************************//
    /// <summary>
    /// Graphics spawning: delay from spawning
    /// </summary>
    public float TimeToSpawnEffect = 0f;
    /// <summary>
    /// Rate at which the effect should spawn
    /// </summary>
    public float EffectSpawnRate = 10f;
    /// <summary>
    /// Delay between firing
    /// </summary>
    protected float TimeToFire = 0f;
    /// <summary>
    /// Position where the bullet will spawn
    /// </summary>
    protected Transform FirePoint { get; private set; }
    /// <summary>
    /// How fast the weapon can shoot per second in addition to the first click
    /// </summary>
    public float FireRate = 0f;
    /// <summary>
    /// Value to add onto the current time.
    /// Default value of 1
    /// </summary>
    public float FireDelay = 1;
    /// <summary>
    /// How much damage it does
    /// </summary>
    public int Damage;
    /// <summary>
    /// Every weapon aside from the default one has ammo that runs out eventually
    /// </summary>
    public int Ammo;


    //******************************************* Triggers *******************************************//
    /// <summary>
    /// Indicates if this weapon has ammo. All weapons have a finite amount of ammo except the default weapon
    /// </summary>
    public bool HasAmmo;
    /// <summary>
    /// Indicates the weapon is in Ultimate Mode!
    /// </summary>
    public bool UltMode { get; set; }


    //******************************************* Optional Projectile Properties *******************************************//
    /// <summary>
    /// Bullet graphics
    /// </summary>
    public Transform BulletPrefab;
    /// <summary>
    /// Optionally set param to indicate How fast the bullet travels
    /// </summary>
    public float BulletSpeed;
    /// <summary>
    /// Optionally set parameter to indicate how long we want the bullet to stay alive. For shotguns we want this to be halved
    /// </summary>
    public float MaxLifetime = 0.5f;

    //******************************************* Extra Properties *******************************************//
    /// <summary>
    /// Need a reference to the player this weapon is aattached to so we can get the direction input
    /// </summary>
    protected Player Player;
    /// <summary>
    /// LayuerMask indicating what to hit
    /// </summary>
    public LayerMask WhatToHit;
    /// <summary>
    /// Reference to the audio manager
    /// </summary>
    protected AudioManager AudioManager;


    // Implementations must override how and when to apply recoil
    protected abstract void ApplyRecoil();

    protected abstract void CalculateShot();

    protected abstract void GenerateShot(Vector3 shotPos, Vector3 shotNormal, LayerMask whatToHit, string layer, bool ultMode, float freeFlyDelay = 0.5f);



    /// <summary>
    /// Setup the weapon
    /// </summary>
    protected void Start() {
        // Get the player this weapon is attached to
        Player = transform.parent.parent.GetComponent<Player>();
        if (Player == null) {
            throw new MissingComponentException("This is massively bad... No Player.cs found on the Player");
        }

        // Find the fire point - where the bullet origin will be
        FirePoint = transform.Find(GameConstants.ObjectName_FirePoint);
        if (FirePoint == null) {
            Debug.LogError("AbstractWeapon.cs: No firePoint found");
            throw new UnassignedReferenceException();
        }
        // Every weapon has its own animator
        WeaponAnimator = transform.GetComponent<Animator>();
        if (WeaponAnimator == null) {
            throw new MissingComponentException("No Weaapon animator was found on the weapon");
        }

        AudioManager = AudioManager.instance;
        if (AudioManager == null) {
            throw new MissingComponentException("No AudioManager was found");
        }

        CamShake = GameMaster.Instance.GetComponent<CameraShake>();
        if (CamShake == null) {
            Debug.LogError("Weapon.cs: No CameraShake found on game master");
            throw new MissingComponentException();
        }
    }

    /// <summary>
    /// Check if this weapon still has ammo
    /// </summary>
    protected void AmmoCheck() {
        if (Ammo == 0) {
            Player.RemoveOtherWeapon(transform);
        }
    }

    /// <summary>
    /// Resets the animator
    /// </summary>
    /// <returns></returns>
    protected IEnumerator ResetWeaponPosition() {
        yield return new WaitForSeconds(0f);
        WeaponAnimator.SetBool("ApplyRecoil", false);
    }

    /// <summary>
    /// Special case handling for negative rotataion neeeded. Only time its needed is for the -45degree shot
    /// </summary>
    /// <param name="rot"></param>
    /// <returns></returns>
    protected Quaternion CompensateQuaternion(Quaternion rot) {
        if ((int)(rot.z * 10) == -3) {
            return rot;
        }
        return new Quaternion(Mathf.Abs(rot.x), Mathf.Abs(rot.y), Mathf.Abs(rot.z), rot.w);
    }

    protected void GenerateCameraShake() {
        CamShake.Shake(CamShakeAmount, CamShakeLength);
    }
}
