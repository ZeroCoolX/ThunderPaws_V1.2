
using UnityEngine;

public class TriggerPaw : Ultimate {
    private PlayerWeaponManager _weaponManager;

    public override void Activate() {
        _weaponManager = transform.GetComponent<PlayerWeaponManager>();
        if (_weaponManager == null) {
            throw new MissingComponentException("No PlayerWeaponManager found on Player object");
        }

        _weaponManager.ToggleUltimateForAllWeapons(true);

        PlayerStats.UltEnabled = true;
        PlayerStats.UltReady = false;

        InvokeRepeating("DepleteUltimate", 0, 0.07f);
        // After 10 seconds deactivate ultimate
        Invoke("DeactivateUltimate", 7f);
    }

    private void DepleteUltimate() {
        --PlayerStats.CurrentUltimate;
        PlayerHudManager.Instance.UpdateUltimateUI(PlayerNum, PlayerStats.CurrentUltimate, PlayerStats.MaxUltimate);
    }

    private void DeactivateUltimate() {
        _weaponManager.ToggleUltimateForAllWeapons(false);
        CancelInvoke("DepleteUltimate");
        DeactivateDelegate.Invoke();
    }


    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
