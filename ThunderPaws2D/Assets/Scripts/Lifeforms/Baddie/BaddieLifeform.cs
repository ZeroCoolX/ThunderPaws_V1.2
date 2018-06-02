using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class specifically for Baddie lifeform types.
/// </summary>
public class BaddieLifeform : DamageableLifeform {
    /// <summary>
    /// Set on the prefab in Unity - Cannot be within struct
    /// </summary>
    public Transform BulletPrefab;
    /// <summary>
    /// Holds all necessary data for what to shoot, where to fire from, and what to hit
    /// </summary>
    protected ProjectileModel ProjectileData;

    /// <summary>
    /// List of all possible players this baddie could potentially target.
    /// </summary>
    // Most often this will be 0-1 (dead or alive single player).
    // However for co-op if one player dies that was this target, it needs 
    // shift to the next available target
    protected List<Transform> Targets = new List<Transform>();
    /// <summary>
    /// The specific player this baddie is targeting
    /// </summary>
    protected Transform Target;

    /// <summary>
    /// Indicates this baddie is part of the horde and should inform 
    /// the HordeController of its death
    /// </summary>
    public bool PartOfHorde = false;

    /// <summary>
    /// Indicates if the lifeform is facing right.
    /// Used to determine the correct sprite showing
    /// </summary>
    protected bool FacingRight = false;

    // Delegates
    /// <summary>
    /// Delegate for informing the HordeController of our death
    /// so it can appropriately update its counts
    /// </summary>
    /// <param name="baddie">
    /// GameObject.name of the baddie that died
    /// </param>
    public delegate void InvokeHordeUpdateDelegate(string baddie);
    public InvokeHordeUpdateDelegate InvokeHordeUpdate;

    protected void Start() {
        // Ensure there is a BulletPrefab attached
        if (BulletPrefab == null) {
            throw new UnassignedReferenceException("BaddieLifeform is missing ProjectileData.BulletPrefab");
        }

        ProjectileData.FirePoint = transform.Find(GameConstants.ObjectName_FirePoint);
        // Ensure we found a firepoint
        if (ProjectileData.FirePoint == null) {
            throw new UnassignedReferenceException("BaddieLifeform is missing ProjectileData.FirePoint");
        }

        FindPlayers();
        if (Target == null) {
            ChooseTarget();
        }
    }

    /// <summary>
    /// Assigns the layermask of what this Baddie should collide with
    /// </summary>
    /// <param name="layers">
    /// 1-many int values representing layermasks which get "or"ed together
    /// </param>
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
        //var playerLayer = 1 << 8;
        //var obstacleLayer = 1 << 10;
        ProjectileData.WhatToHit = layerMask;
    }

    /// <summary>
    /// Check if we are out of targets.
    /// Find new tragets if necessary.
    /// Assign specific target if necessary :
    ///     Previous target died but there is another player in the workd
    /// </summary>
    protected void CheckTargetsExist() {
        if (Targets == null || Targets.Where(t => t != null).ToList().Count == 0) {
            FindPlayers();
            return;
        } else if (Targets.Contains(null)) {
            Targets = Targets.Where(target => target != null).ToList();
            // The max spawn time is 10 seconds so in 10 seconds search again for a player
            Invoke("FindPlayers", 10f);
        }

        if (Target == null) {
            ChooseTarget();
        }
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
            // Choose a random player
            var targets = Targets.Where(t => t != null).ToArray();
            var index = Random.Range(0, targets.Length - 1);
            Target = targets[index];
        }
        print("New Target found. Adding target reference : " + Target.gameObject.name);
    }

    /// <summary>
    /// based off where the player is in relation to this baddie
    /// we can tell which direction we should be facing.
    /// </summary>
    /// <param name="directionToTarget"></param>
    protected void CalculateFacingDirection(float directionToTarget) {
        if (directionToTarget == 0 || Mathf.Sign(transform.localScale.x) == Mathf.Sign(directionToTarget)) { return; }

        // Switch the way the lifeform is labelled as facing.
        FacingRight = Mathf.Sign(directionToTarget) <= 0;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    /// <summary>
    /// Overridden Method.
    /// Move the baddie based off velocity.
    /// </summary>
    protected override void Move() {
        Controller2d.Move(Velocity * Time.deltaTime);
    }

    /// <summary>
    /// Destroy the baddie.
    /// </summary>
    /// <param name="invokeDelegate">
    /// Determines whether we inform anyone else of our death.
    /// Used primarily for baddies in a Horde to inform the HordeManager
    /// of the death
    /// </param>
    /// <param name="deathOffset">
    /// Allows optional delayed destruction
    /// </param>
    public void DestroyBaddie(bool invokeDelegate, float deathOffset = 0f) {
        if (invokeDelegate) {
            InvokeHordeUpdate.Invoke(gameObject.name);
        }
        Invoke("InvokeDestroy", deathOffset);
    }

    /// <summary>
    /// Overridden method.
    /// Destroy the baddie based off if they're apart of the horde or not.
    /// </summary>
    protected override void PreDestroy() {
        DestroyBaddie(PartOfHorde);
    }

    /// <summary>
    /// Overridden method. 
    /// Before calling the base destruction logic calculate the score
    /// of the player killing this baddie
    /// </summary>
    protected override void InvokeDestroy() {
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
        GameMaster.Instance.Score += increment;
        base.InvokeDestroy();
    }

    /// <summary>
    /// Contains all properties needed to use projectiles.
    /// Where to shoot it, and what to hit
    /// </summary>
    protected struct ProjectileModel {
        public Transform FirePoint;
        public LayerMask WhatToHit;
    }
}
