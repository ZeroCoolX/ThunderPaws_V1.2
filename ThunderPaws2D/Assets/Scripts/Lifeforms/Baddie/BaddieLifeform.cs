using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class specifically for Baddie Lifeforms.
/// Has Baddie specific logic for dying.
/// </summary>
public class BaddieLifeform : DamageableLifeform {
    /// <summary>
    /// Indicates this baddie is part of the horde and should inform 
    /// the HordeController of its death
    /// </summary>
    public bool PartOfHorde = false;

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
}
