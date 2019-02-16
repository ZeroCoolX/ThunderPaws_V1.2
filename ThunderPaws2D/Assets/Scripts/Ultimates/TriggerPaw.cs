using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriggerPaw : Ultimate {
    public Transform BulletTrailPrefab;

    private List<GameObject> _baddies;
    private Transform FirePoint;

    public override void Activate() {
        print("TriggerPaw activated!");
        CollectAllBaddies();
    }

    private void CollectAllBaddies() {
        var baddies = GameObject.FindGameObjectsWithTag(GameConstants.Tag_Baddie).Union(GameObject.FindGameObjectsWithTag(GameConstants.Tag_HordeBaddie));
        if (baddies == null || baddies.Count() == 0) {
            print("There were no baddies on screen");
            return;
        }
        _baddies = baddies.ToList();
        BeginUlt();
    }

    private void BeginUlt() {
        var delay = 0.25f;
        for (var i = 0; i < _baddies.Count(); ++i) {
            Invoke("Fire", delay);
            delay += 0.25f;
        }
        Invoke("DeactivateUltimate", delay);
    }

    private void Fire() {
        var currentBaddie = _baddies.OrderByDescending(bad => Vector3.Distance(transform.position, bad.transform.position)).FirstOrDefault();
        _baddies.Remove(currentBaddie);
        //Generate bullet trail
        Transform trail = Instantiate(BulletTrailPrefab, FirePoint.position, FirePoint.rotation) as Transform;
        LineRenderer lr = trail.GetComponent<LineRenderer>();
        //allows the bullet trail to stop where the collision happenned
        if (lr != null) {
            lr.SetPosition(0, FirePoint.position);//start position index
            lr.SetPosition(1, currentBaddie.transform.position);//end position index
        }
        Destroy(trail.gameObject, 0.1f);
        currentBaddie.gameObject.GetComponent<BaddieLifeform>().Damage(999);
    }


    //private PlayerWeaponManager _weaponManager;

    //public override void Activate() {
    //    _weaponManager = transform.GetComponent<PlayerWeaponManager>();
    //    if (_weaponManager == null) {
    //        throw new MissingComponentException("No PlayerWeaponManager found on Player object");
    //    }

    //    _weaponManager.ToggleUltimateForAllWeapons(true);

    //    PlayerStats.UltEnabled = true;
    //    PlayerStats.UltReady = false;

    //    InvokeRepeating("DepleteUltimate", 0, 0.07f);
    //    // After 10 seconds deactivate ultimate
    //    Invoke("DeactivateUltimate", 7f);
    //}

    //private void DepleteUltimate() {
    //    --PlayerStats.CurrentUltimate;
    //    PlayerHudManager.Instance.UpdateUltimateUI(PlayerNum, PlayerStats.CurrentUltimate, PlayerStats.MaxUltimate);
    //}

    private void DeactivateUltimate() {
        //_weaponManager.ToggleUltimateForAllWeapons(false);
        //CancelInvoke("DepleteUltimate");
        DeactivateDelegate.Invoke();
    }


    // Use this for initialization
    void Start() {
        FirePoint = GetComponent<PlayerWeaponManager>().GetCurrentWeapon().GetComponent<AbstractWeapon>().FirePoint;
    }

    // Update is called once per frame
    void Update() {

    }
}
