﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialKeyActionController : MonoBehaviour {

    public string InputListener;
    private int successIndicator = 0;

    private SpriteRenderer _spriteRenderer;

    private bool _complete = false;

    public Sprite[] Sprites = new Sprite[2];

    public TutorialControllerBase Controller;


    // Use this for initialization
    void Start () {
        _spriteRenderer = transform.GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null) {
            throw new MissingComponentException("Missing sprite on tutorial controller");
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (_complete) {
            return;
        }
        switch (InputListener) {
            case "Melee":
                if (Input.GetKeyDown(InputManager.Instance.Melee)) {
                    ++successIndicator;
                }
                break;
            case "Roll":
                if (Input.GetKeyDown(InputManager.Instance.Roll)) {
                    ++successIndicator;
                }
                break;
            case "ChangeWeapon":
                if (Input.GetKeyDown(InputManager.Instance.ChangeWeapon)) {
                    ++successIndicator;
                }
                break;
        }
	    if(successIndicator == 3) {
            _spriteRenderer.sprite = Sprites[1];
            Controller.IncrementProgress();
            _complete = true;
        }
    }
}