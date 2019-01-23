using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class Ultimate : MonoBehaviour {
    public PlayerStats PlayerStats;
    public int PlayerNum;

    public delegate void DeactivateUltimateDelegate();
    public DeactivateUltimateDelegate DeactivateDelegate;

    public abstract void Activate();
}
