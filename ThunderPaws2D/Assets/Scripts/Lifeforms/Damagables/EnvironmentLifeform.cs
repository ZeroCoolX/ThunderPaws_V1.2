using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentLifeform : BaseLifeform {

    private bool _damage = false;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (_damage) {
            GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0.8f);
            _damage = false;
        }else {
            GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount", 0f);
        }

    }

    public override void Damage(float dmg) {
        _damage = true;
    }

}
