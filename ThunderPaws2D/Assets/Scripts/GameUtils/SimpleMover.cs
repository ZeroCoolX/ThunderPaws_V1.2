using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMover : MonoBehaviour {

    /// <summary>
    /// Indicates what stops this from accumulating gravity
    /// </summary>
    public LayerMask HoverOver;
    /// <summary>
    /// How much gravity should be accumulated
    /// </summary>
    public float Gravity;
    /// <summary>
    /// How high above an obstacle the object hovers
    /// </summary>
    public float HoverDistance;
    /// <summary>
    /// Vector representing movement
    /// </summary>
    private Vector2 _velocity;
    /// <summary>
    /// How fast to move
    /// </summary>
    private float _moveSpeed = 5f;

	// Update is called once per frame
	void Update () {
        Debug.DrawRay(transform.position, Vector3.down * HoverDistance, Color.green);

        HoverCheck();
        Move();
    }



    private void HoverCheck() {
        RaycastHit2D hitCheck = Physics2D.Raycast(transform.position, Vector2.down, HoverDistance, HoverOver);
        if (hitCheck.collider != null) {
            print("WE HIT A THING!");
            _moveSpeed = 0f;
        }
    }

    private void Move() {
        ApplyGravity();
        transform.Translate(_velocity * _moveSpeed * Time.deltaTime, Space.World);
    }

    private void ApplyGravity() {
        _velocity.y += Gravity * Time.deltaTime;
    }
}
