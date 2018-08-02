using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMover : MonoBehaviour {

    public LayerMask HoverOver;
    public float Gravity;
    public float HoverDistance;

    private Vector2 _velocity;
    private float _moveSpeed = 5f;



	void Update () {
        Debug.DrawRay(transform.position, Vector3.down * HoverDistance, Color.green);
        HoverCheck();
        Move();
    }



    private void HoverCheck() {
        RaycastHit2D hitCheck = Physics2D.Raycast(transform.position, Vector2.down, HoverDistance, HoverOver);
        if (hitCheck.collider != null) {
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
