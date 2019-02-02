using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LighteningClawUltimate : MonoBehaviour {
    // Use this for initialization
    private Stack<GameObject> _flyingBaddies;
    private GameObject _currentTarget;

    private float _xVelocity;
    private float _yVelocity;
    private float _smoothTime = 1F;

    void OnEnable() { 
        CollectAllBaddies();
        PauseBaddieMovement();
    }

    private void CollectAllBaddies() {
        var allBaddies = GameObject.FindGameObjectsWithTag(GameConstants.Tag_Baddie);
        if(allBaddies == null || allBaddies.Count() == 0) {
            print("There were no baddies on screen");
            return;
        }
        Stack<GameObject> badd = new Stack<GameObject>();
        allBaddies.Select(obj => {
                if (obj.name.IndexOf("_FL") != -1) {
                    _flyingBaddies.Push(obj);
                }
            return obj;
        });
        print("Collected " + _flyingBaddies.Count() + " baddies");
    }

    private void PauseBaddieMovement() {
        foreach(var baddie in _flyingBaddies) {
            baddie.GetComponent<BaddieLifeform>().enabled = false;
        }
    }

    void Update () {
        CheckForCollision();

		if(_currentTarget == null) {
            if(_flyingBaddies.Count() > 0) {
                _currentTarget = _flyingBaddies.Pop();
            }
        } else {
            CalculateVelocity();
        }
	}

    private void CheckForCollision() {
        print("do nothing quite yet");
    }

    private void CalculateVelocity() {
        float newX = Mathf.SmoothDamp(transform.position.x, _currentTarget.transform.position.x, ref _xVelocity, _smoothTime);
        float newY = Mathf.SmoothDamp(transform.position.y, _currentTarget.transform.position.y, ref _yVelocity, _smoothTime);
        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}
