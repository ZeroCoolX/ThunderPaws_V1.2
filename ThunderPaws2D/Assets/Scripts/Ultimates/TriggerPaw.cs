using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriggerPaw : Ultimate {
    public Transform BulletTrailPrefab;

    private List<GameObject> _baddies;
    private Transform FirePoint;

    private BetterCameraFollow _cameraScript;
    private TriggerPawCursor _ultCursor;

    public override void Activate() {
        print("TriggerPaw activated!");

        PlayerStats.UltEnabled = true;
        PlayerStats.UltReady = false;

        Update_MarkOverInteraction();
    }

    private void Update_MarkOverInteraction() {
        StopAllMovement();
    }

    private void StopAllMovement() {
        InvokeRepeating("DepleteUltimate", 0, 0.03f);
        // After 10 seconds deactivate ultimate
        Invoke("StopUltimateDrain", 3f);

        GetComponent<Player>().enabled = false;
        _cameraScript.enabled = false;
        _ultCursor.Activate();
    }

    private void CollectTaggedBaddies(List<GameObject> tagged) {
        _baddies = tagged;
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

    private void DepleteUltimate() {
        --PlayerStats.CurrentUltimate;
        PlayerHudManager.Instance.UpdateUltimateUI(PlayerNum, PlayerStats.CurrentUltimate, PlayerStats.MaxUltimate);
    }

    private void StopUltimateDrain() {
        CancelInvoke("DepleteUltimate");
        PlayerHudManager.Instance.UpdateUltimateUI(PlayerNum, PlayerStats.CurrentUltimate, PlayerStats.MaxUltimate);
    }

    private void DeactivateUltimate() {
        GetComponent<Player>().enabled = true;
        _cameraScript.enabled = true;
        DeactivateDelegate.Invoke();
    }

    void Start() {
        FirePoint = GetComponent<PlayerWeaponManager>().GetCurrentWeapon().GetComponent<AbstractWeapon>().FirePoint;
        _cameraScript = Camera.main.GetComponentInParent<BetterCameraFollow>();
        _ultCursor = GetComponentInChildren<TriggerPawCursor>();
        _ultCursor.InvokeTaggedBaddies += CollectTaggedBaddies;
        _ultCursor.Player = GetComponent<Player>();
    }
}
