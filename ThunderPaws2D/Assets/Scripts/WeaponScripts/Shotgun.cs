using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : AbstractWeapon {


    protected override void ApplyRecoil() {
        throw new NotImplementedException();
    }

    protected override void CalculateShot() {
        throw new NotImplementedException();
    }

    protected override void GenerateShot(Vector3 shotPos, Vector3 shotNormal, LayerMask whatToHit, string layer, bool ultMode, float freeFlyDelay = 0.5F) {
        throw new NotImplementedException();
    }
}
