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

    public Transform LeftBuddy;
    public Transform RightBuddy;

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
                RightBuddy = GenerateBuddy(1);
                _hasARightBuddy = true;
            } else if (_mainCam.transform.position.x <= edgeVisiblePositionLeft + _buddyIndicatorZone && !_hasALeftBuddy) {
                LeftBuddy = GenerateBuddy(-1);
                _hasALeftBuddy = true;
            }
        }

        var camLeftEdge = _mainCam.transform.position.x - camHorizontalExtent; // C
        var camRightEdge = _mainCam.transform.position.x + camHorizontalExtent; // D
        float parallaxLeftEdge = _myTransform.position.x - (_spriteWidth / 2);// + camHorizontalExtent; // A
        float parallaxRightEdge = _myTransform.position.x + (_spriteWidth / 2);// - camHorizontalExtent; // B

        if(parallaxRightEdge + _buddyIndicatorZone < camLeftEdge) {
            // Delete ittself and all the left buddies
            DeleteBuddyThenSelf(transform, -1);
        } else if(parallaxLeftEdge - _buddyIndicatorZone > camRightEdge) {
            // Delete itself and all the right baddies
            DeleteBuddyThenSelf(transform, 1);
        }        
	}

    private void DeleteBuddyThenSelf(Transform buddy, int direction) {
        if(buddy == null) {
            return;
        }
        if(direction < 0) {
            // Left buddies
            DeleteBuddyThenSelf(buddy.GetComponent<ParallaxTiling>().LeftBuddy, -1);
            buddy.GetComponent<ParallaxTiling>().RightBuddy.GetComponent<ParallaxTiling>()._hasALeftBuddy = false;
            Destroy(buddy.gameObject);
            return;
        } else {
            // Right buddies
            DeleteBuddyThenSelf(buddy.GetComponent<ParallaxTiling>().RightBuddy, 1);
            buddy.GetComponent<ParallaxTiling>().LeftBuddy.GetComponent<ParallaxTiling>()._hasARightBuddy = false;
            Destroy(buddy.gameObject);
            return;
        }
    }

    private Transform GenerateBuddy(int onRightOrLeft) {
        Vector3 newPostion = new Vector3(_myTransform.position.x + _spriteWidth * onRightOrLeft, _myTransform.position.y, _myTransform.position.z);
        Transform buddy = Instantiate(_myTransform, newPostion, _myTransform.rotation) as Transform;

        buddy.parent = _myTransform.parent;
        if(onRightOrLeft > 0) {
            buddy.GetComponent<ParallaxTiling>()._hasALeftBuddy = true;
            buddy.GetComponent<ParallaxTiling>().LeftBuddy = _myTransform;
        }else {
            buddy.GetComponent<ParallaxTiling>()._hasARightBuddy = true;
            buddy.GetComponent<ParallaxTiling>().RightBuddy = _myTransform;
        }

        return buddy;
    }
}
