using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBase : MonoBehaviour {
    public Transform Target { get; set; }
    /// <summary>
    /// Threshold of camera movement down
    /// </summary>
    public float YCameraPosClamp = -19;
    /// <summary>
    /// Buffer for position dampeneing so movment is not sudden and jerky
    /// </summary>
    protected float Dampening = 0.25f;
    /// <summary>
    /// How far to look ahead from our current position
    /// </summary>
    protected float LookAheadFactor = 1.25f;
    /// <summary>
    /// How fast we get to the desired position
    /// </summary>
    protected float LookAheadReturnSpeed = 0.80f;
    /// <summary>
    /// Determines if we should be looking for the target or wheather we're in a close enough range
    /// </summary>
    protected float LookAheadMoveThreshold = 0.1f;
    protected float OffsetZ;
    /// <summary>
    /// Indicates offset from the moddle based on facing/moving direction
    /// </summary>
    protected float OffsetX = 0.15f;
    protected Vector3 LastTargetPosition;
    protected Vector3 CurrentVelocity;
    protected Vector3 LookAheadPos;

    private float nextTimeToSearch = 0f;
    private float searchDelay = 0.25f;
    private string _searchName;



    protected void InitializeSearchName(string target) {
        _searchName = target;
    }

    protected void Start() {
        if(Target == null) {
            FindPlayer();
            return;
        }
        LastTargetPosition = Target.position;
        OffsetZ = (transform.position - Target.position).z;
        transform.parent = null;
    }

    protected void FindPlayer() {
        if (nextTimeToSearch <= Time.time) {
            GameObject searchResult = GameObject.FindGameObjectWithTag(_searchName);
            if (searchResult != null) {
                if (!searchResult.GetComponent<BaddieActivator>().enabled) {
                    // Activate in 1 second
                    Invoke("DelayedActivate", 1);
                }
                Target = searchResult.transform;
                nextTimeToSearch = Time.time + searchDelay;
            }
        }
    }

    private void DelayedActivate() {
        Target.gameObject.GetComponent<BaddieActivator>().enabled = true;
        Target.gameObject.GetComponent<SimpleCollider>().enabled = true;
    }
}
