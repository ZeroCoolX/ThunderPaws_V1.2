using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTimer : MonoBehaviour {

    public float TimeToLive = 0f;

    void Start() {
        Invoke("DestroyThis", TimeToLive);
    }

    private void DestroyThis() {
        Destroy(gameObject);
    }
}
