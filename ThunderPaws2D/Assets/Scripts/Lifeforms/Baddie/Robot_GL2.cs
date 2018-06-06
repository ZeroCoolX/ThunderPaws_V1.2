using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Robot_GL2 : GroundBaddieLifeform {
    // This could be extracted out into configs but I don't mind them living here
    // They're just used for initializing the property values that live on the 
    // parent classes
    private readonly float _gravity = -25.08f;
    private readonly float _health = 15f;
    private readonly float _shotDelay = 2f;
    private readonly int _moveSpeed = 0;
    private readonly float _visionLength = 20f;
    private readonly string _attackAnimation = "ChargeAndFire";

    public void Start() {
        base.Start();
        
        // Set baddie specific data
        GroundPositionData.ShotDelay = _shotDelay;
        GroundPositionData.MoveSpeed = _moveSpeed;
        VisionRayLength = _visionLength;
        Gravity = _gravity;
        Health = _health;

        // Optional animation to play when atacking
        OptionalAttackAnimation = _attackAnimation;

        // This baddie has an animation to play to therefor we set the Animator
        // Not all baddies have an Animator
        Animator = transform.GetComponent<Animator>();
        if(Animator == null) {
            throw new MissingComponentException("There is no animator on this baddie");
        }

        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - Target.position.x;
        CalculateFacingDirection(directionToTarget);
    }

    public void Update() {
        base.Update();

        if (!CheckTargetsExist()) {
            return;
        }

        // Find out where the target is in reference to this.
        var directionToTarget = transform.position.x - Target.position.x;
        // Check if we can shoot at the target
        CheckForHorizontalEquality(0.5f, Time.time > GroundPositionData.TimeSinceLastFire);
    }
}
