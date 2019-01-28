using UnityEngine;

/// <summary>
/// Class specifically for Player Lifeforms.
/// </summary>
public abstract class PlayerLifeform : BaseLifeform {
    public bool NoFallCheck = false;

    protected float FallDeathHeight = -18;
    protected float VelocityXSmoothing;
    protected Animator Animator;

    protected struct MovementData {
        public float MoveSpeed;
        public float MaxJumpHeight;
        public float MinJumpHeight;
        public float MaxJumpVelocity;
        public float MinJumpVelocity;
        public float TimeToJumpApex;
        public float AccelerationTimeAirborne;
        public float AccelerationTimeGrounded;
    }
    protected MovementData MoveData;


    /// <summary>
    /// Set all constant physics values
    /// Calculate dynamic values like Gravity and JumpVelocity
    /// </summary>
    protected void InitializePhysicsValues(float moveSpeed, float maxJumpHeight, float minJumpHeight, float timeToJumpApex, float accelerationTimeAirborne, float accelerationTimeGrounded, float gravity = -1) {
        MoveData = new MovementData {
            MoveSpeed = moveSpeed,
            MinJumpHeight = minJumpHeight,
            MaxJumpHeight = maxJumpHeight,
            TimeToJumpApex = timeToJumpApex,
            AccelerationTimeAirborne = accelerationTimeAirborne,
            AccelerationTimeGrounded = accelerationTimeGrounded
        };

        SetupComponents();
        CalculateGravity(gravity);
        CalculateJumpVelocities(minJumpHeight, maxJumpHeight);

        print("Gravity: " + Gravity + "\n Jump Velocity: " + MoveData.MaxJumpVelocity);
    }

    private void SetupComponents() {
        Controller2d = GetComponent<CollisionController2D>();
        if (Controller2d == null) {
            throw new MissingComponentException("Player is missing a CollisionController2D");
        }
        Animator = GetComponent<Animator>();
        if (Animator == null) {
            throw new MissingComponentException("Player is missing an Animator");
        }
    }

    private void CalculateGravity(float gravity) {
        if (gravity == -1) {
            Gravity = -(2.5f * MoveData.MaxJumpHeight) / Mathf.Pow(MoveData.TimeToJumpApex, 2);
        } else {
            Gravity = gravity;
        }
    }

    private void CalculateJumpVelocities(float minJumpHeight, float maxJumpHeight) {
        MoveData.MaxJumpVelocity = Mathf.Abs(Gravity) * MoveData.TimeToJumpApex;
        MoveData.MinJumpVelocity = (maxJumpHeight == minJumpHeight ? MoveData.MaxJumpVelocity : Mathf.Sqrt(2 * Mathf.Abs(Gravity) * minJumpHeight));
    }

    protected void FallCheck() {
        if (NoFallCheck) {
            return;
        }
        // This is a super dumb hack to allow baddies to spawn in the last room because im so worn out I want to finish this
        if(transform.position.y <= FallDeathHeight && transform.position.y > -40f) {
            var player = transform.GetComponent<Player>();
            if (player != null && !player.Controller2d.Collisions.FromBelow) {
                print("Player fell off the map");
                //Damage(999);
            }
        }
    }
}
