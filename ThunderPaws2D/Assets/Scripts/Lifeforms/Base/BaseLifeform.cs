using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseLifeform : MonoBehaviour {
    protected float Health { get; set; }

    public abstract void Damage(float dmg);
}
