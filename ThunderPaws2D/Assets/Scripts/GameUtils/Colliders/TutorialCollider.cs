using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCollider : MonoBehaviour {

    /// <summary>
    /// Necessary for collisions
    /// </summary>
    private SimpleCollider Collider;

    private SpriteRenderer _spriteRenderer;

    public Sprite[] Sprites = new Sprite[2];

    public TutorialControllerBase Controller;

    public int WhatToCollideWith = 8;
    public float SizeOfCollider = 2f;

    // Use this for initialization
    void Start() {
        //Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << WhatToCollideWith, SizeOfCollider, false);

        _spriteRenderer = transform.GetComponent<SpriteRenderer>();
        if(_spriteRenderer == null) {
            throw new MissingComponentException("Missing sprite on tutorial controller");
        }
    }

    //void OnDrawGizmosSelected() {
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawSphere(transform.position, 0.5f);
    //}


    private void Apply(Vector3 v, Collider2D c) {
        print("Apply");
        _spriteRenderer.sprite = Sprites[1];
        Controller.IncrementProgress();
    }
}
