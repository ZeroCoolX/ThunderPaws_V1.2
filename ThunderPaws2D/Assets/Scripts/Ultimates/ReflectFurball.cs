
using System.Collections.Generic;
using UnityEngine;

public class ReflectFurball : Ultimate {
    private HashSet<int> _reflectedProjectiles;

    public override void Activate() {
        print("ReflectFurball activated!");
        _reflectedProjectiles = new HashSet<int>();
        //DeactivateDelegate.Invoke();
    }

    private void Update() {
        CheckForMultiCircleCollisions();
    }

    private void CheckForMultiCircleCollisions() {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 2, 1 << 11);
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
}
