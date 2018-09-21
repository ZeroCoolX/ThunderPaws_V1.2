using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Robot_GL2 : GroundBaddieLifeform {

    private const float GRAVITY = -25.08f;
    private const float HEALTH = 15f;
    private const float SHOT_DELAY = 2f;
    private const float FIRE_DELAY = 0.5f;
    private const float FIRE_ANIMATION_DELAY = 0.15f;
    private const float VISION_LENGTH = 20f;
    private const string ATTACK_ANIMATION = "ChargeAndFire";

    public void Start() {
        base.Start();
        
        GroundPositionData.ShotDelay = SHOT_DELAY;
        GroundPositionData.FireDelay = FIRE_DELAY;
        GroundPositionData.FireAnimationDelay = FIRE_ANIMATION_DELAY;
        VisionRayLength = VISION_LENGTH;
        Gravity = GRAVITY;
        Health = HEALTH;

        OptionalAttackAnimation = ATTACK_ANIMATION;

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
        var hCollider = FireRaycast();
        if (hCollider.collider != null) {
            HaltAndFire();
        }

        CalculateFacingDirection(directionToTarget);

        ApplyGravity();
        Move();
    }
}
