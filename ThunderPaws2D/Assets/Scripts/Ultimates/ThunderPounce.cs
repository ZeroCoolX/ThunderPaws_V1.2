using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ThunderPounce : Ultimate {

    private List<GameObject> _baddies;
    private float _pounceHeightMax = 8F;
    private Vector3 _pounceApex;
    private Vector3 _originPosition;
    private float _yVelocityUp;
    private float _yVelocityDown;
    private float _smoothTimeUp = 0.15F;
    private bool _apexReached = false;
    private float _smoothTimeDown = 0.09F;
    private bool _active = false;


    public override void Activate() {
        print("ThunderPounce activated!");
        PlayerStats.UltEnabled = true;
        PlayerStats.UltReady = false;

        GetComponent<Player>().enabled = false;

        ResetCollection();
        CollectAllBaddies();

        _pounceApex = transform.position + (Vector3.up * _pounceHeightMax);
        _originPosition = transform.position;

        _active = true;
    }

    private void CollectAllBaddies() {
        var baddies = GameObject.FindGameObjectsWithTag(GameConstants.Tag_Baddie).Union(GameObject.FindGameObjectsWithTag(GameConstants.Tag_HordeBaddie));
        if (baddies == null || baddies.Count() == 0) {
            print("There were no baddies on screen");
            return;
        }

        foreach (var baddie in baddies.ToList()) {
            if (baddie.gameObject.name.IndexOf("GL") != -1) {
                print("Adding baddie " + baddie.gameObject.name + " to stack");
                _baddies.Add(baddie);
            }
        }
    }

    private void DamageAllBaddies() {
        GenerateCameraShake();
        foreach (var baddie in _baddies) {
            baddie.GetComponent<BaddieLifeform>().Damage(100);
        }
    }

    private void GenerateCameraShake() {
        GameMasterV2.Instance.GetComponent<CameraShake>().Shake(0.05F, 0.25F);
    }

    private void ResetCollection() {
        _baddies = new List<GameObject>();
    }

    private void LateUpdate() {
        if (!_active) {
            return;
        }
        if(PlayerHadLanded()) {
            DamageAllBaddies();
            ResetCollection();
            DeactivateDelegate.Invoke();
            _active = false;
            return;
        }

        if(_apexReached && transform.position.y <= (_originPosition.y + 0.5F)) {
            GetComponent<Player>().enabled = true;
        }

        CalculateVelocity();
    }

    private bool PlayerHadLanded() {
        return GetComponent<Player>().enabled && GetComponent<Player>().Get2DController().Collisions.FromBelow;
    }

    private void CalculateVelocity() {
        float newY;

        if (transform.position.y < (_pounceApex.y - 0.5f) && !_apexReached) {
            newY = Mathf.SmoothDamp(transform.position.y, _pounceApex.y, ref _yVelocityUp, _smoothTimeUp);
        } else {
            _apexReached = true;
            newY = Mathf.SmoothDamp(transform.position.y, _originPosition.y, ref _yVelocityDown, _smoothTimeDown);
        }

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
