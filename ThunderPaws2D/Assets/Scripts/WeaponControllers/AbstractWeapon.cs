using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class that has functionality all weapons share
/// </summary>
public abstract class AbstractWeapon : MonoBehaviour {
    /// <summary>
    /// Animator for the weaapon
    /// </summary>
    protected Animator WeaponAnimator;

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
    protected Transform FirePoint { get; private set; }

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
        WeaponAnimator = transform.GetComponent<Animator>();
        if(WeaponAnimator == null) {
            throw new MissingComponentException("No Weaapon animator was found on the weapon");
        }
    }

    protected abstract void HandleShootingInput();

    /// <summary>
    /// Generate particle effect, spawn bullet, then destroy after allotted time
    /// </summary>
    /// <param name="shotPos"></param>
    /// <param name="shotNormal"></param>
    /// <param name="whatToHit"></param>
    public virtual void GenerateEffect(Vector3 shotPos, Vector3 shotNormal, LayerMask whatToHit, string layer, float freeFlyDelay = 0.5f) {
        //Fire the projectile - this will travel either out of the frame or hit a target - below should instantiate and destroy immediately
        var projRotation = CompensateQuaternion(FirePoint.rotation);
        Transform bulletInstance = Instantiate(BulletPrefab, FirePoint.position, projRotation) as Transform;
        //Parent the bullet to who shot it so we know what to hit (parents LayerMask whatToHit)
        AbstractProjectile projectile = bulletInstance.GetComponent<BulletProjectile>();
        //TODO will have to be changed when diagonal directional shooting comes into play - take out when we pass in the rotation of the bullet
        if(Mathf.Sign(shotPos.x) < 0) {
            Vector3 theScale = projectile.transform.localScale;
            theScale.x *= -1;
            projectile.transform.localScale = theScale;
        }

        //Set layermask of parent (either player or baddie)
        projectile.SetLayerMask(whatToHit);
        projectile.gameObject.layer = LayerMask.NameToLayer(layer);
        projectile.Damage = Damage;
        projectile.MoveSpeed = BulletSpeed;
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

    protected abstract void ApplyRecoil();

    protected IEnumerator ResetWeaponPosition() {
        yield return new WaitForSeconds(0f);
        WeaponAnimator.SetBool("ApplyRecoil", false);
    }

    private Quaternion CompensateQuaternion(Quaternion rot) {
        //Special case handling for negative rotataion neeeded. Only time its needed is for the -45degree shot
        if((int)(rot.z * 10) == -3) {
            return rot;
        }
        return new Quaternion(Mathf.Abs(rot.x), Mathf.Abs(rot.y), Mathf.Abs(rot.z), rot.w);
    }

}
