using UnityEngine;

public class BetterCameraFollow : MonoBehaviour {

    public Transform Target;
    public Vector3 HordePosition = Vector3.zero;

    public Vector2 FocusAreaSize;
    public float VerticalOffset;
    public float LookaheadDistanceX;
    public float LookSmoothTimeX;
    public float VerticalSmoothTime;

    private float _currentLookaheadX;
    private float _targetLookaheadX;
    private float _lookaheadDirectionX;
    private float _smoothLookVelocityX;
    private float _smoothVelocityY;

    private bool _lookaheadStopped;

    private CollisionController2D _targetCollider;
    private FocusArea _focusArea;

    private float nextTimeToSearch = 0f;
    private float searchDelay = 0.25f;
    private string _searchName;

    private void Start() {
        if (Target == null || _targetCollider == null) {
            FindPlayer();
            return;
        }
        _targetCollider = Target.GetComponent<CollisionController2D>();
        _focusArea = new FocusArea(_targetCollider.BoxCollider.bounds, FocusAreaSize);
    }

    protected void FindPlayer() {
        if (nextTimeToSearch <= Time.time) {
            GameObject searchResult = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);
            if (searchResult != null) {
                Target = searchResult.transform;
                _targetCollider = Target.GetComponent<CollisionController2D>();
                _focusArea = new FocusArea(_targetCollider.BoxCollider.bounds, FocusAreaSize);
                nextTimeToSearch = Time.time + searchDelay;
            }
        }
    }

    private void LateUpdate() {
        // Update for everything unless we're in a horde section
        if (HordePosition != Vector3.zero) {
            transform.position = HordePosition + Vector3.forward * -10;
            return;
        }

        if (Target == null || _targetCollider == null) {
            FindPlayer();
            return;
        }

        _focusArea.Update(_targetCollider.BoxCollider.bounds);

        Vector2 focusPosition = _focusArea.Center + Vector2.up * VerticalOffset;

        if(_focusArea.Velocity.x != 0) {
            _lookaheadDirectionX = Mathf.Sign(_focusArea.Velocity.x);
            if(Mathf.Sign(_targetCollider.PlayerInput.x) == Mathf.Sign(_focusArea.Velocity.x) && _targetCollider.PlayerInput.x != 0) {
                _lookaheadStopped = false;
                _targetLookaheadX = _lookaheadDirectionX * LookaheadDistanceX;
            }else {
                if (!_lookaheadStopped) {
                    _lookaheadStopped = true;
                    _targetLookaheadX = _currentLookaheadX + (_lookaheadDirectionX * LookaheadDistanceX - _currentLookaheadX) / 4; // just random 4...
                }
            }
        }

        _currentLookaheadX = Mathf.SmoothDamp(_currentLookaheadX, _targetLookaheadX, ref _smoothLookVelocityX, LookSmoothTimeX);

        focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref _smoothVelocityY, VerticalSmoothTime);
        focusPosition += Vector2.right * _currentLookaheadX;
        transform.position = (Vector3)focusPosition + Vector3.forward * -10; // making sure the camera is in front of the level always
    }

    private void OnDrawGizmos() {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(_focusArea.Center, FocusAreaSize);
    }

    struct FocusArea {
        public Vector2 Center;
        public Vector2 Velocity;
        float Left, Right;
        float Top, Bottom;

        public FocusArea(Bounds targetBounds, Vector2 size) {
            Left = targetBounds.center.x - (size.x / 2);
            Right = targetBounds.center.x + (size.x / 2);
            Bottom = targetBounds.min.y;
            Top = targetBounds.min.y + size.y;

            Velocity = Vector2.zero;
            Center = new Vector2((Left + Right) / 2, (Top + Bottom) / 2);
        }

        public void Update(Bounds targetBounds) {
            // check if the player is moving up against either the left or right edge
            float amountToShiftX = 0;
            if(targetBounds.min.x < Left) {
                amountToShiftX = targetBounds.min.x - Left;
            }else if(targetBounds.max.x > Right) {
                amountToShiftX = targetBounds.max.x - Right;
            }
            Left += amountToShiftX;
            Right += amountToShiftX;

            // check if the player is moving up against either the top or bottom edge
            float amountToShiftY = 0;
            if (targetBounds.min.y < Bottom) {
                amountToShiftY = targetBounds.min.y - Bottom;
            } else if (targetBounds.max.y > Top) {
                amountToShiftY = targetBounds.max.y - Top;
            }
            Top += amountToShiftY;
            Bottom += amountToShiftY;

            //copy center position
            Center = new Vector2((Left + Right) / 2, (Top + Bottom) / 2);
            Velocity = new Vector2(amountToShiftX, amountToShiftY);
        }
    }
}
