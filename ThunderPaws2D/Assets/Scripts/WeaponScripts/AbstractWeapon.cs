using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractWeapon : MonoBehaviour {
    [Header("Camera Properties")]
    public float CamShakeAmount = 0.025f;
    public float CamShakeLength = 0.1f;
    protected CameraShake CamShake;

    [Header("Fire Effect Properties")]
    public float ShotEffectDelay = 0f;
    public float ShotEffectSpawnRate = 10f;
    protected Animator WeaponAnimator;

    [Header("Fire Properties")]
    public float FireRate = 0f;
    public float FireDelay = 1;
    protected float TimeToFire = 0f;
    protected Transform FirePoint { get; private set; }

    [Header("Weapon Properties")]
    public int Damage;
    public int Ammo;
    public int MaxAmmo;
    public bool HasAmmo;
    public bool UltMode { get; set; }
    public float BulletSpeed;
    public float MaxLifetime = 0.5f;

    [Header("Prefabs")]
    public Transform BulletPrefab;
    public Player Player;
    public LayerMask WhatToHit;
    protected AudioManager AudioManager;


    // Implementations can override the following method but do not need to (in the case of Shotgun and EmissionIndex)
    protected virtual void ApplyRecoil() { }
    protected virtual void CalculateShot(int bulletCount = 1) { }
    protected virtual void GenerateShot(Vector3 shotPos, Vector3 shotNormal, LayerMask whatToHit, string layer, int bulletCount = 1) { }


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

        WeaponAnimator = transform.GetComponent<Animator>();
        if (WeaponAnimator == null) {
            throw new MissingComponentException("No Weaapon animator was found on the weapon");
        }

        AudioManager = AudioManager.Instance;
        if (AudioManager == null) {
            throw new MissingComponentException("No AudioManager was found");
        }

        CamShake = GameMasterV2.Instance.GetComponent<CameraShake>();
        if (CamShake == null) {
            Debug.LogError("Weapon.cs: No CameraShake found on game master");
            throw new MissingComponentException();
        }

        MaxAmmo = Ammo;
    }

    public void FillAmmoFromUlt() {
        Ammo = MaxAmmo;
    }

    protected void CheckAmmo() {
        if (Ammo <= 0) {
            Player.RemoveOtherWeapon(transform);
        }
    }

    /// <summary>
    /// Resets the animator between each fire animation (kickback)
    /// </summary>
    protected IEnumerator ResetWeaponPosition() {
        yield return new WaitForSeconds(0f);
        WeaponAnimator.SetBool("ApplyRecoil", false);
    }

    /// <summary>
    /// Special case handling for negative rotataion neeeded. Only time its needed is for the -45degree shot
    /// </summary>
    protected Quaternion CompensateQuaternion(Quaternion rotation) {
        if ((int)(rotation.z * 10) == -3) {
            return rotation;
        }
        return new Quaternion(Mathf.Abs(rotation.x), Mathf.Abs(rotation.y), Mathf.Abs(rotation.z), rotation.w);
    }

    protected void GenerateCameraShake() {
        CamShake.Shake(CamShakeAmount, CamShakeLength);
    }
}
