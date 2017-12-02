using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class that has functionality all weapons share
/// </summary>
public abstract class AbstractWeapon : MonoBehaviour {

    /// <summary>
    /// How fast the weapon can shoot per second in addition to the first click
    /// </summary>
    [Header("Abstract: Attributes")]
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
    /// How fast the bullet travels
    /// </summary>
    public float BulletSpeed;

    /// <summary>
    /// Bullet graphics
    /// </summary>
    [Header("Abstract: Effects")]
    public Transform BulletPrefab;
    //public Transform HitPrefab;
    //public Transform MuzzleFlashPrefab;
    /// <summary>
    /// Position where the bullet will spawn
    /// </summary>
    public Transform FirePoint;

    /// <summary>
    /// Graphics spawning: delay from spawning
    /// </summary>
    [Header("Abstract: TimeAttributes")]
    public float TimeToSpawnEffect = 0f;
    /// <summary>
    /// Rate at which the effect should spawn
    /// </summary>
    public float EffectSpawnRate = 10f;
    /// <summary>
    /// Delay between firing
    /// </summary>
    public float _timeToFire = 0f;
    /// <summary>
    /// Indicates whether the fire mode should be burst
    /// </summary>
    public bool IsBurst = false;

    protected void Start() {
        FirePoint = transform.Find("FirePoint");
        if (FirePoint == null) {
            Debug.LogError("AbstractWeapon.cs: No firePoint found");
            throw new UnassignedReferenceException();
        }
    }

    /// <summary>
    /// Generate particle effect, spawn bullet, then destroy after allotted time
    /// </summary>
    /// <param name="shotPos"></param>
    /// <param name="shotNormal"></param>
    /// <param name="whatToHit"></param>
    public virtual void GenerateEffect(Vector3 shotPos, Vector3 shotNormal, LayerMask whatToHit, string layer, float freeFlyDelay = 0.5f) {
        //Fire the projectile - this will travel either out of the frame or hit a target - below should instantiate and destroy immediately
        Transform bulletInstance = Instantiate(BulletPrefab, FirePoint.position, FirePoint.rotation) as Transform;
        //Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
        AbstractProjectile projectile = bulletInstance.GetComponent<BulletProjectile>();

        //Set layermask of parent (either player or baddie)
        projectile.SetLayerMask(whatToHit);
        projectile.gameObject.layer = LayerMask.NameToLayer(layer);
        projectile.Damage = Damage;
        projectile.MoveSpeed = BulletSpeed;
        print(projectile.MoveSpeed);
        //Fire at the point clicked
        projectile.Fire(shotPos, shotNormal);

        //Generate muzzleFlash
        //Transform muzzleFlash = Instantiate(MuzzleFlashPrefab, FirePoint.position, FirePoint.rotation) as Transform;
        //Parent to firepoint
        //muzzleFlash.parent = FirePoint;
        //Randomize its size a bit
        //float size = Random.Range(0.2f, 0.5f);
        //muzzleFlash.localScale = new Vector3(size, size, size);
        //Destroy muzzle flash
        //Destroy(muzzleFlash.gameObject, 0.035f);
        //Generate shot sound
//        _audioManager.playSound(WeaponShootSound);
    }

}
