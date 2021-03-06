﻿
using System.Collections.Generic;
using UnityEngine;

public class ReflectFurball : Ultimate {
    public Transform ReflectBubblePrefab;
    private Transform _bubbleEffect;

    private HashSet<int> _reflectedProjectiles;
    private bool _active = false;

    private float _ultDuration = 10f;

    public override void Activate() {
        print("ReflectFurball activated!");
        InvokeRepeating("DepleteUltimate", 0, 0.1f);
        Invoke("StopUltimateDrain", 10f);

        _bubbleEffect = Instantiate(ReflectBubblePrefab, transform.position, transform.rotation, transform) as Transform;
        _bubbleEffect.GetComponent<Animator>().SetBool("bubble_on", true);
        _bubbleEffect.GetComponent<SpriteRenderer>().sortingOrder = 100;
        _reflectedProjectiles = new HashSet<int>();
        _active = true;

        PlayerStats.UltEnabled = true;
        PlayerStats.UltReady = false;

        Invoke("ResetState", _ultDuration);
    }

    private void ResetState() {
        _reflectedProjectiles = new HashSet<int>();
        _active = false;
    }

    private void Deactivate() {
        _bubbleEffect.GetComponent<Animator>().SetBool("bubble_on", false);
        Invoke("DestroyBubble", 0.25f);
        ResetState();
        DeactivateDelegate.Invoke();
    }

    void DestroyBubble() {
        Destroy(_bubbleEffect.gameObject);
    }

    private void Update() {
        if (_active) {
            CheckForMultiCircleCollisions();
        }
    }

    private void CheckForMultiCircleCollisions() {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1.75f, 1 << 11);
        foreach (var collider in colliders) {
            if (collider != null &&
                !_reflectedProjectiles.Contains(collider.gameObject.GetInstanceID())) {
                // reverse direction of bullet
                // change layermask
                // make target direction opposite of current
                _reflectedProjectiles.Add(collider.gameObject.GetInstanceID());
                ReverseProjectile(collider.gameObject);
            }
        }
    }

    private void ReverseProjectile(GameObject bullet) {
        var bulletScript = bullet.GetComponent<AbstractProjectile>();
        if(bulletScript == null) {
            print("there was no AbstractProjectile on collided object : " + bullet.name);
        }

        bulletScript.SetLayerMask(1 << 10 | 1 << 14); // obstacle and damageable
        bulletScript.MoveSpeed += 5;
        bulletScript.ResetTargetDirection(1 << 10 | 1 << 14);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }

    private void DepleteUltimate() {
        --PlayerStats.CurrentUltimate;
        PlayerHudManager.Instance.UpdateUltimateUI(PlayerNum, PlayerStats.CurrentUltimate, PlayerStats.MaxUltimate);
    }

    private void StopUltimateDrain() {
        CancelInvoke("DepleteUltimate");
        PlayerHudManager.Instance.UpdateUltimateUI(PlayerNum, PlayerStats.CurrentUltimate, PlayerStats.MaxUltimate);
        Deactivate();
    }
}
