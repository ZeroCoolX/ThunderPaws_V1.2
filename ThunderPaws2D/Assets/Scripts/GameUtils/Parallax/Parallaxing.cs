using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Parallaxing : MonoBehaviour {

    public Transform[] Backgrounds;
    protected float[] ParallaxScales;

    //private int _numberOfBackgrounds;
    public float Smoothing = 1f;            // How smooth the parallax is going to be. Make sure to set this above 0

    protected void Awake() {
        ParallaxScales = new float[Backgrounds.Length];
    }

    protected abstract void ApplyRelativeParallax();
}
