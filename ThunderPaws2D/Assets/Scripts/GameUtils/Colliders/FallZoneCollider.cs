using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallZoneCollider : MonoBehaviour {

    SimpleCollider _collider;

	// Use this for initialization
	void Start () {
        _collider = GetComponent<SimpleCollider>();
        if (_collider == null) {
            throw new MissingComponentException("SimpleCollider on HiddenAreaCollider [" + gameObject.name + "] is missing");
        }
        _collider.Initialize(1 << GameConstants.Layer_Player, new Vector2(15f, 5f));
        _collider.InvokeCollision += KillObject;
    }

    public void KillObject(Vector3 v, Collider2D c) {
        c.GetComponent<Player>().Damage(999);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector2(15f, 5f));
    }
}
