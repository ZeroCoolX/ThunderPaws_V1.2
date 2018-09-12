using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaddieBoss : BaddieLifeform {

    //public Transform target;
    //public float yTarget = 2;
    //public float xTarget = 2;
    private float smoothTime = 1F;
    private float yVelocity = 0.3F;
    private float xVelocity = 0.3F;

    //void Update() {
    //    float newPositiony = Mathf.SmoothDamp(transform.position.y, yTarget, ref yVelocity, smoothTime);
    //    float newPositionx = Mathf.SmoothDamp(transform.position.x, xTarget, ref xVelocity, smoothTime);
    //    transform.position = new Vector3(newPositionx, newPositiony, transform.position.z);
    //}
    public Transform[] Attack1Points;

    private Vector3 _currentAttackPoint;
    private float _moveSpeed = 1f;
    private float _moveTrigger;
    private float _delayBetweenMoves = 1f;


    private new void Start() {
        base.Start();
        Gravity = 0.0f;
        RandomlySelectAttackPoint();
        _moveTrigger = Time.time + _delayBetweenMoves;
    }
    // Update is called once per frame
    private new void Update() {
        base.Update();

        if (!CheckTargetsExist()) {
            return;
        }

        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - Target.position.x;
        CalculateFacingDirection(directionToTarget);

        if (Time.time > _moveTrigger) {
            RandomlySelectAttackPoint();
            _moveTrigger = Time.time + _delayBetweenMoves;
        }
        print("Vector3.Distance(" + transform.position + ", " + _currentAttackPoint + ") = ");
        CalculateVelocity();
    }

    private void CalculateVelocity() {
        float newX = Mathf.SmoothDamp(transform.position.x, _currentAttackPoint.x, ref yVelocity, smoothTime);
        float newY = Mathf.SmoothDamp(transform.position.y, _currentAttackPoint.y, ref xVelocity, smoothTime);
        transform.position = new Vector3(newX, newY, transform.position.z);
    }

    private void RandomlySelectAttackPoint() {
        var rand = Random.Range(0, 5);
        print("Selecting random point: " + Attack1Points[rand].gameObject.name);
        _currentAttackPoint = Attack1Points[rand].position;
    }

}
