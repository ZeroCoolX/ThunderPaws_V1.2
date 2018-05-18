using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTimer : MonoBehaviour {

    public float TimeToLive = 0f;

    // Use this for initialization
    void Start() {
        // test to see 
        Invoke("DestroyThis", TimeToLive);
    }

    private void DestroyThis() {
        Destroy(gameObject);
    }
}
