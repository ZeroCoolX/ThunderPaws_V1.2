using System;
using System.Collections.Generic;

[Serializable]
public class ProfileModel {
    public int EmissionCache;
    public string SelectedWeapon;
    public string SelectedUltimate;
    public List<string> Weapons;
    public List<string> Ultimates;
    public List<string> Levels; 
}
