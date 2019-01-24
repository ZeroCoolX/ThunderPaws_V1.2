using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HiddenAreaCollider : MonoBehaviour {
    public Vector2 ZoneDimensions;

    private SimpleCollider _collider;
    private Tilemap _tilemap;
    private bool _hidden = true;

	// Use this for initialization
	void Start () {
		// while someone collides with me i want to set the alpha of the target to 0.24
        _collider = GetComponent<SimpleCollider>();
        if(_collider == null) {
            throw new MissingComponentException("SimpleCollider on HiddenAreaCollider [" + gameObject.name + "] is missing");
        }
        _collider.Initialize(1 << GameConstants.Layer_Player, ZoneDimensions, true);
        _collider.InvokeCollision += RevealArea;
        _collider.InvokeCollisionStopped += HideArea;

        var tilemapObj = GameObject.Find("Tilemap_HiddenArea");
        if(tilemapObj == null) {
            throw new MissingReferenceException("Could not locate a hidden area tilemap");
        }
        _tilemap = tilemapObj.GetComponent<Tilemap>();

    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, ZoneDimensions);
    }

    public void RevealArea(Vector3 v, Collider2D c) {
        if (_hidden) {
            print("revealing area!");
            ApplyTilemapAlpha(0.65f);
            _hidden = false;
        }
    }

    public void HideArea() {
        if (!_hidden) {
            print("hiding area");
            ApplyTilemapAlpha(1f);
            _hidden = true;
        }
    }

    private void ApplyTilemapAlpha(float alpha) {
        var color = _tilemap.color;
        color.a = alpha;
        _tilemap.color = color;
    }
}
