using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxTiling : MonoBehaviour {

    // deadzone between end of parallax and camera
    private float _buddyIndicatorZone = 3;

    // These are used as instantiation checks
    private bool _hasARightBuddy = false;
    private bool _hasALeftBuddy = false;

    private float _spriteWidth;
    private Camera _mainCam;
    private Transform _myTransform; // Is this really a performance boost?

    private void Awake() {
        _mainCam = Camera.main;
        _myTransform = transform;

    }

    // Use this for initialization
    void Start () {
        SpriteRenderer sRenderer = GetComponent<SpriteRenderer>();
        _spriteWidth = sRenderer.sprite.bounds.size.x;
	}
	
	// Update is called once per frame
	void Update () {
        // This will be the length from the center of the camera to its right bar
        float camHorizontalExtent = _mainCam.orthographicSize * Screen.width / Screen.height;

        // Calculate the x position where the camera can see the edge fo the sprite
        float edgeVisiblePositionRight = _myTransform.position.x + (_spriteWidth / 2) - camHorizontalExtent; // B
        float edgeVisiblePositionLeft = _myTransform.position.x - (_spriteWidth / 2) + camHorizontalExtent; // A
        

        if (!_hasALeftBuddy || !_hasARightBuddy) {
            // Checking is we can see the edge of the element
            if((_mainCam.transform.position.x >= edgeVisiblePositionRight - _buddyIndicatorZone) && !_hasARightBuddy){
                GenerateBuddy(1);
                _hasARightBuddy = true;
            }else if (_mainCam.transform.position.x <= edgeVisiblePositionLeft + _buddyIndicatorZone && !_hasALeftBuddy) {
                GenerateBuddy(-1);
                _hasALeftBuddy = true;
            }
        }/*else if(_hasALeftBuddy && _hasARightBuddy){
            // Checking is we can see the edge of the element
            if (_mainCam.transform.position.x - camHorizontalExtent >= _myTransform.position.x + (_spriteWidth / 2)) {
                Destroy(gameObject);
            } else if (_mainCam.transform.position.x + camHorizontalExtent <= _myTransform.position.x - (_spriteWidth / 2)) {
                Destroy(gameObject);
            }
        }*/
	}

    private void GenerateBuddy(int onRightOrLeft) {
        Vector3 newPostion = new Vector3(_myTransform.position.x + _spriteWidth * onRightOrLeft, _myTransform.position.y, _myTransform.position.z);
        Transform buddy = Instantiate(_myTransform, newPostion, _myTransform.rotation) as Transform;

        buddy.parent = _myTransform.parent;
        if(onRightOrLeft > 0) {
            buddy.GetComponent<ParallaxTiling>()._hasALeftBuddy = true;
        }else {
            buddy.GetComponent<ParallaxTiling>()._hasARightBuddy = true;
        }
    }
}
