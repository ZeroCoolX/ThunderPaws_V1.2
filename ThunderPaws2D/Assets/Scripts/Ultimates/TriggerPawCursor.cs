using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriggerPawCursor : MonoBehaviour {
    public delegate void InvokeBaddiesTaggedDelegate(List<GameObject> baddies);
    public InvokeBaddiesTaggedDelegate InvokeTaggedBaddies;

    public Sprite[] ClickSprites;

    public float TagSize = 2.5f;
    private float TagDurationTime = 3f;
    public Player Player;

    private SimpleCollider _collider;
    private Dictionary<int, GameObject> _baddiesTagged;
    private float _cursorMoveSpeed = 15f;
    private Vector3 _velocity;
    private float _velocityXSmoothing;
    private float _velocityYSmoothing;
    private bool _active;

    private SpriteRenderer _renderer;
    private int _spriteIndex = 0;

    void Start () {
        _renderer = GetComponent<SpriteRenderer>();
        if(_renderer == null) {
            throw new MissingComponentException("SpriteRenderer is null");
        }
    }

    private void ResetMap() {
        _baddiesTagged = new Dictionary<int, GameObject>();
    }

    public void Activate() {
        _active = true;
        _renderer.enabled = true;

        ResetMap();

        _collider = GetComponent<SimpleCollider>();
        _collider.Initialize(1 << 14, TagSize, true);
        _collider.InvokeCollision += TagBaddie;

        Invoke("StopTagging", TagDurationTime);
    }

    private void StopTagging() {
        _active = false;
        _collider.enabled = false;
        transform.position = Player.transform.position;
        InvokeTaggedBaddies.Invoke(_baddiesTagged.Select(kvp => kvp.Value).ToList());
        enabled = false;
        _renderer.enabled = false;
    }

    private void TagBaddie(Vector3 v, Collider2D c) {
        if(c.gameObject.tag != GameConstants.Tag_Baddie) {
            return;
        }

        GameObject outObj;
        if(!_baddiesTagged.TryGetValue(c.gameObject.GetInstanceID(), out outObj)) {
            if(Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown(Player.JoystickId + GameConstants.Input_Jump)) {
                print("Adding baddie : " + c.gameObject.GetInstanceID() + " to map");
                _baddiesTagged.Add(c.gameObject.GetInstanceID(), c.gameObject);
                c.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                c.gameObject.GetComponent<BaddieLifeform>().ForceHalt = true;
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, TagSize);
    }

    private void Update() {
        if (!_active) {
            return;
        }

        if(Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown(Player.JoystickId + GameConstants.Input_Jump)) {
            _renderer.sprite = ClickSprites[1];
        }else {
            _renderer.sprite = ClickSprites[0];
        }

        var directionalInput = GetDirectionBasedOffInputType();
        print("dir: " + directionalInput + " vel: " + _velocity);
        _velocity.x = Mathf.SmoothDamp(_velocity.x, directionalInput.x * _cursorMoveSpeed, ref _velocityXSmoothing, 0f);
        if (!Player.FacingRight) {
            _velocity *= -1;
        }
        _velocity.y = Mathf.SmoothDamp(_velocity.y, directionalInput.y * _cursorMoveSpeed, ref _velocityYSmoothing, 0f);
        transform.Translate(_velocity * Time.deltaTime);
        print("LOCAL SCALE: " + transform.localScale);
    }

    private Vector2 GetDirectionBasedOffInputType() {
        return (JoystickManagerController.Instance.ConnectedControllers() > 0)
            ? new Vector2(Input.GetAxisRaw(Player.JoystickId + GameConstants.Input_Horizontal), Input.GetAxisRaw(Player.JoystickId + GameConstants.Input_Vertical))
            : new Vector2(Input.GetAxisRaw(GameConstants.Input_Horizontal), Input.GetAxisRaw(GameConstants.Input_Vertical));
    }
}
