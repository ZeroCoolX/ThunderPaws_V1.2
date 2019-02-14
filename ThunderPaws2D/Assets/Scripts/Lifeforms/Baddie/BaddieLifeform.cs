using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class specifically for Baddie lifeform types.
/// </summary>
public class BaddieLifeform : DamageableLifeform {
    public Animator Animator;
    public Transform BulletPrefab;
    /// <summary>
    /// Indicates this baddie is part of the horde and should inform 
    /// the HordeController of its death
    /// </summary>
    public bool PartOfHorde = false;
    public bool ForceHalt;

    public int BulletSpeed;
    public int BulletDamage;

    /// <summary>
    /// Delegate for informing the HordeController of our death so it can appropriately update its counts
    /// </param>
    public delegate void InvokeHordeUpdateDelegate(string baddie);
    public InvokeHordeUpdateDelegate InvokeHordeUpdate;

    /// <summary>
    /// List of all possible players this baddie could potentially target.
    /// Most often this will be 0-1 (dead or alive single player).
    /// However for co-op if one player dies that was this target, it needs to
    /// shift to the next available target
    /// </summary>
    protected List<Transform> Targets = new List<Transform>();
    protected Transform Target;
    protected bool FacingRight = false;

    private const int PLAYER_LAYER = 8;
    private const int OBSTACLE_LAYER = 10;
    private const float TARGET_FIND_DELAY = 10f;

    protected struct ProjectileModel {
        public Transform FirePoint;
        public LayerMask WhatToHit;
    }
    protected ProjectileModel ProjectileData;

    protected void Start() {
        if (BulletPrefab == null) {
            //throw new UnassignedReferenceException("BaddieLifeform is missing ProjectileData.BulletPrefab");
            print("BaddieLifeform is missing ProjectileData.BulletPrefab");
        }

        ProjectileData.FirePoint = transform.Find(GameConstants.ObjectName_FirePoint);
        if (ProjectileData.FirePoint == null) {
            print("BaddieLifeform is missing ProjectileData.FirePoint");
        }

        // Assign the layermask for WhatToHit to be the Player(8) and Obstacle(10)
        AssignLayermask(PLAYER_LAYER, OBSTACLE_LAYER);

        FindPlayers();
        if (Target == null) {
            ChooseTarget();
        }
    }

    protected void AssignLayermask(params int[] layers) {
        LayerMask layerMask = 0;
        // This should grow the layermask "or"ing as many times as we need for example
        // layerMask = 0 | 8
        // layerMask = (0 | 8) | 12
        // layerMask = (0 | 8 | 12) | 17
        foreach (var layer in layers) {
            layerMask = layerMask | (1 << layer);
        }
        // Set the layermasks
        ProjectileData.WhatToHit = layerMask;
    }

    /// <summary>
    /// Check if we are out of targets.
    /// Find new targets if necessary.
    /// Assign specific target if necessary :
    ///     Previous target died but there is another player in the workd
    /// </summary>
    protected bool CheckTargetsExist() {
        if (Targets == null || Targets.Where(t => t != null).ToList().Count == 0) {
            FindPlayers();
            return false;
        } else if (Targets.Contains(null)) {
            Targets = Targets.Where(target => target != null).ToList();
            // The max spawn time is 10 seconds so in 10 seconds search again for a player
            Invoke("FindPlayers", TARGET_FIND_DELAY);
        }

        if (Target == null) {
            ChooseTarget();
        }
        return true;
    }

    /// <summary>
    /// Find all players and store them as target references 
    /// </summary>
    protected void FindPlayers() {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(GameConstants.Tag_Player);
        if (targets == null || targets.Where(t => t != null).ToList().Count == 0) {
            return;
        }
        foreach (var target in targets) {
            // Only add the player if its not already in the list
            if (!Targets.Contains(target.transform)) {
                Targets.Add(target.transform);
            }
        }
    }

    /// <summary>
    /// Choose a target from the list.
    /// Number in list = Action
    /// 0 = Short Circuit
    /// 1 = Choose it
    /// > 1 = Choose based off random index in array < array.length
    /// </summary>
    protected void ChooseTarget() {
        if (Targets == null || Targets.Where(t => t != null).ToList().Count == 0) {
            return;
        } else if (Targets.Count == 1) {
            Target = Targets.FirstOrDefault();
        } else {
            ChooseRandomTarget();
        }
        print("New Target found. Adding target reference : " + Target.gameObject.name);
    }

    private void ChooseRandomTarget() {
        var targets = Targets.Where(t => t != null).ToArray();
        var index = Random.Range(0, targets.Length - 1);
        Target = targets[index];
    }

    /// <summary>
    /// Based off where the player is in relation to this baddie
    /// we can tell which direction we should be facing.
    /// </summary>
    protected void CalculateFacingDirection(float directionToTarget) {
        if (directionToTarget == 0 || Mathf.Sign(transform.localScale.x) == Mathf.Sign(directionToTarget)) { return; }

        // Switch the way the lifeform is labelled as facing.
        FacingRight = Mathf.Sign(directionToTarget) <= 0;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    protected void InitiateAttack() {
        Animator.SetBool("Attack", true);
    }

    protected void ResetAttack() {
        Animator.SetBool("Attack", false);
    }

    protected override void Move() {
        print("MOVING");
        Animator.SetFloat("Velocity", Mathf.Abs(Velocity.x));
        Controller2d.Move(Velocity * Time.deltaTime);
    }

    public void DestroyBaddie(bool invokeDelegate, float deathOffset = 0f) {
        if (invokeDelegate) {
            InvokeHordeUpdate.Invoke(gameObject.name);
        }
        Invoke("InvokeDestroy", deathOffset);
    }

    protected override void PreDestroy() {
        DestroyBaddie(PartOfHorde);
    }

    protected override void InvokeDestroy() {
        var increment = CalculateDeathScore();
        GameMasterV2.Instance.Score += increment;
        base.InvokeDestroy();
    }

    private int CalculateDeathScore() {
        var increment = 0;
        if (gameObject.name.Contains("GL1")) {
            increment = 1;
        } else if (gameObject.name.Contains("GL2")) {
            increment = 2;
        } else if (gameObject.name.Contains("FL1")) {
            increment = 1;
        } else if (gameObject.name.Contains("FL2")) {
            increment = 2;
        } else if (gameObject.name.Contains("FL3")) {
            increment = 3;
        }
        return increment;
    }
}
