using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotator : MonoBehaviour {

    private int _rotationAmount;

	// Use this for initialization
	void Start () {
        Random.InitState((int)Time.time);
        _rotationAmount = Random.Range(3, 35);
    }
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(new Vector3(0, 0, 1) * (_rotationAmount));
    }
}
