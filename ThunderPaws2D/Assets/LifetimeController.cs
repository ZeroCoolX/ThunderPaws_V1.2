using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifetimeController : MonoBehaviour {
    [Header("WARNING! Only set true if Renderer is on child")]
    public bool ParentCheck = false;
    /// <summary>
    /// Allows for some objects to dip in and out of the frame withouth being killed immediately if this is false.
    /// Otherwise its killed immediately
    /// </summary>
    public bool KillImmediatelyOnInvisible = true;
    /// <summary>
    /// /Indicates it was in the frame within 0-AllowLifetimeWhileInvisible seconds
    /// </summary>
    private bool _seenRecently = false;
    /// <summary>
    /// How long we should wait when it becomes invisible to kill it
    /// </summary>
    public float AllowLifetimeWhileInvisible = 10f;
    /// <summary>
    /// Indicates how far this can fall before it dies
    /// </summary>
    private float FallDeathHeight = -19;
    /// <summary>
    /// Once the bullet leaves the Cameras viewport destroy it
    /// </summary>
    void OnBecameInvisible() {
        _seenRecently = false;
        //print(gameObject.name + " has become invisible. Waiting " + AllowLifetimeWhileInvisible + " seconds to kill him");
        Invoke("Kill", (KillImmediatelyOnInvisible ? 0f : AllowLifetimeWhileInvisible));
    }

    private void Kill() {
        if (KillImmediatelyOnInvisible || !_seenRecently) {
            if(ParentCheck && transform.parent != null) {
                Destroy(transform.parent.gameObject);
            } else {
                Destroy(gameObject);
            }
        }
    }

    void OnBecameVisible() {
        _seenRecently = true;
    }

    private void Update() {
        FallCheck();
    }

    protected void FallCheck() {
        if (transform.position.y <= FallDeathHeight) {
            // Ensure nothing can survive
            Destroy(gameObject);
        }
    }
}
