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

    private float FallDeathHeight = -19;



    /// <summary>
    /// Once the bullet leaves the Cameras viewport destroy it
    /// </summary>
    void OnBecameInvisible() {
        _seenRecently = false;
        Invoke("Kill", (KillImmediatelyOnInvisible ? 0f : AllowLifetimeWhileInvisible));
    }

    private void Kill() {
        print("Killing Object because onBecameInvisble for object: " + gameObject.name);
        if (KillImmediatelyOnInvisible || !_seenRecently) {
            if(ParentCheck && transform.parent != null) {
                Destroy(transform.parent.gameObject);
            } else {
                Destroy(gameObject);
            }
        }
    }

    void OnBecameVisible() {
        print(" Object " + gameObject.name+" became visible!");
        _seenRecently = true;
    }

    private void Update() {
        //FallCheck();
        // special case for when they spawn outside the field of view - they never "become visible" - therefor never "become invisible" therfor kill never gets called
        if(KillImmediatelyOnInvisible && !_seenRecently) {
            Kill();
        }
    }

    protected void FallCheck() {
        if (transform.position.y <= FallDeathHeight && transform.position.y > -40f) {
            // Ensure nothing can survive
            var damageable = gameObject.GetComponent<DamageableLifeform>();
            if (damageable != null) {
                damageable.Damage(999);
            } else {
                Destroy(gameObject);
            }
        }
    }
}
