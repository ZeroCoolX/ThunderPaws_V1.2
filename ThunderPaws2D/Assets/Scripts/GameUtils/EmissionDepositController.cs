using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionDepositController : MonoBehaviour {
    [Header("Payload Properties")]
    public Transform PayloadContent;
    public int PayloadItemCount = 10;

    public Transform EmptyShell;

    /// <summary>
    /// Min and Max values for the range to which the items in the payload can explode.
    /// </summary>
    private Vector2[] _explodeRanges = new[]{
        new Vector2(1f, 7f),
        new Vector2(2f, 15f)
    };

    private Animator _animator;
    private SimpleCollider _depositCollider;
    private const int PLAYER_LAYER = 8;

    // Use this for initialization
    void Start() {
        AddCollisionDelegate();
    }

    //private void OnDrawGizmos() {
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawSphere(transform.position, 2);
    //}

    private void AddCollisionDelegate(){ 
        _depositCollider = GetComponent<SimpleCollider>();
        if(_depositCollider == null) {
            throw new MissingComponentException("Cannot find SimpleCollider on EmissionDepositcontroller");
        }
        _depositCollider.InvokeCollision += Apply;
        _depositCollider.Initialize(1 << PLAYER_LAYER, 2);

        _animator = GetComponent<Animator>();
        if(_animator == null) {
            throw new MissingComponentException("Missing animator on the deposit!");
        }
    }

    private void Apply(Vector3 v, Collider2D c) {
        // once the player hits this we need to stop them, flash animate, and ad to their emission cache
        var player = c.transform.GetComponent<Player>();
        player.PickupEmissionDeposit(100);//TODO: remove hard coded
        StopGameMomentarily();
    }

    private void StopGameMomentarily() {
        // play animation
        _animator.SetBool("Throb", true);

        // after 2 seconds 
        Invoke("PreDestroy", 2f);
    }

    private void PreDestroy() {
        GeneratePayload();
        GenerateEmptyShell();
        Destroy(gameObject);
    }

    private void GenerateEmptyShell() {
        Instantiate(EmptyShell, transform.position, transform.rotation);
    }

    private void GeneratePayload() {
        List<Transform> payload = new List<Transform>();
        // Generate the items to be offloaded and store them in the list
        for (var i = 0; i < PayloadItemCount; ++i) {
            var pItem = Instantiate(PayloadContent, transform.position, Quaternion.identity) as Transform;
            pItem.gameObject.SetActive(false);
            var coinController = pItem.GetComponent<CoinController>();
            if (coinController != null) {
                coinController.Initialize(GenerateRandomExplosionDirection());
                coinController.enabled = false;
            }
            payload.Add(pItem);
        }

        // Now offload in an explosive manor
        Explode(payload);
    }

    private Vector2 GenerateRandomExplosionDirection() {
        var explodeX = UnityEngine.Random.Range(_explodeRanges[0].x, _explodeRanges[1].x);
        explodeX *= Mathf.Sign(UnityEngine.Random.Range(-1, 2));
        var explodeY = UnityEngine.Random.Range(_explodeRanges[0].y, _explodeRanges[1].y);
        var explosionDir = transform.rotation * new Vector2(explodeX, explodeY);
        return explosionDir;
    }

    /// <summary>
    /// Generate explosion effect by activating the script on each Transform
    /// in the payload.
    /// </summary>
    private void Explode(List<Transform> payload) {
        foreach (var pItem in payload) {
            if (pItem.GetComponent<CoinController>() != null) {
                pItem.GetComponent<CoinController>().enabled = true;
            }
            pItem.gameObject.SetActive(true);
        }
    }
}