using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeController : MonoBehaviour {
    /// <summary>
    /// Indicates if we should start spawning enemies or not
    /// </summary>
    private bool _spawningAllowed = false;
    
    /// <summary>
    /// Necessary for collisions
    /// </summary>
    private SimpleCollider Collider;
    /// <summary>
    /// Reference to the main camera
    /// </summary>
    public Transform Camera;
    
    /// <summary>
    /// Total baddies left to kill before player can move on
    /// </summary>
    public int BaddiesLeftToKill = 20;
    
    // Reference to the baddie prefab - DEFINITELY TODO: add these to the runtime gamemaster
    public Transform GL1BaddiePrefab;
    // Indicates how many of each type of baddie is allowed on screen at any one point
    public int MaxGL1Count = 10;
    // Keeps a count of how many baddies of this type on on screen
    private int _activeGL1Count;
    
    // Reference to the baddie prefab - DEFINITELY TODO: add these to the runtime gamemaster
    public Transform GL2BaddiePrefab;
    public int MaxGL2Count = 3;
    private int _activeGL2Count;
    
    // Reference to the baddie prefab - DEFINITELY TODO: add these to the runtime gamemaster
    public Transform FL1BaddiePrefab;
    public int MaxFL1Count = 5;
    private int _activeFL1Count;
    
    // Reference to the baddie prefab - DEFINITELY TODO: add these to the runtime gamemaster
    public Transform FL2BaddiePrefab;
    public int MaxFL2Count = 4;
    private int _activeFL2Count;
    
    // Reference to the baddie prefab - DEFINITELY TODO: add these to the runtime gamemaster
    public Transform FL3BaddiePrefab;
    public int MaxFL3Count = 1;
    private int _activeFL3Count;
    
    /// <summary>
    /// Holds the flying spawn points
    /// </summary>
    public Transform[] FlyingSpawns;
    /// <summary>
    /// Holds the ground spawn points
    /// </summary>
    public Transform[] GroundSpawns;

    // Use this for initialization
    void Start() {
        //Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << 8, 12);
        
        if(FlyingSpawns == null){
            throw new MissingComponentException("No Flying Spawns specified")
        }
        if(GroundSpawns == null){
            throw new MissingComponentException("No Flying Spawns specified")
        }
    }
    
    void Update(){
        if(_spawningAllowed){
            SpawnBaddies();
        }
    }
    
    private void SpawnBaddies(){
        SpawnFlyingBaddies();
        SpawnGroundBaddies();
    }
    
    private void InstantiateBaddies(int numBaddies, int maxBaddies, Transform baddiePrefab, Vector3 position){
        // This just stops the baddies from spawning literally on top of one another
        var offset = 2;
            // We need to spawn the difference of Max and active
            for(var i = numBaddies; i <= maxBaddies; ++i){
                var cleanPosition = new Vector3(position.x, position.y + offset, position.z);
                Transform baddieTransform = Instantiate(baddiePrefab, cleanPosition, transform.rotation) as Transform;
                var damageableLifeform = baddieTransform.GetComponent<DamageableLifeform>();
                if(damageableLifeform == null){
                    throw new MissingComponentException("Somehow the baddie: " + baddieTransform.gameObject.name + " does not have a DamageableLifeform script attached");
                }
                damageableLifeform.HordeNotificationDelegate += UpdateBaddieCount
            }
    }
    
    private void UpdateBaddieCount(string baddieName){
        --BaddiesLeftToKill;
        switch(baddieName){
            case: "G1"
                --_activeGL1Count;
                break;
            case: "G2"
                --_activeGL2Count;
                break;
            case: "F1"
                --_activeFL1Count;
                break;
            case: "F2"
                --_activeFL2Count;
                break;
            case: "F3"
                --_activeFL3Count;
                break;
        }
    }
    
    private void SpawnFlyingBaddies(){
        var rand = (int)Random.Range(0, FlyingSpawns.Length);
        if(_activeFL1Count < MaxFL1Count){
            InstantiateBaddies((MaxFl1Count - _activeFL1Count), MaxFl1Count, FL1BaddiePrefab, FlyingSpawns[rand].position);
        }
        if(_activeFL2Count < MaxFL2Count){
            rand = Random.Range(0, FlyingSpawns.Length);
            InstantiateBaddies((MaxFl2Count - _activeFL2Count), MaxFl2Count, FL2BaddiePrefab, FlyingSpawns[rand].position);
            _activeFL2Count = MaxFl2Count;
        }
        if(_activeFL3Count < MaxFL3Count){
            rand = Random.Range(0, FlyingSpawns.Length);
            InstantiateBaddies((MaxFl3Count - _activeFL3Count), MaxFl3Count, FL3BaddiePrefab, FlyingSpawns[rand].position);
            _activeFL3Count = MaxFl3Count;
        }
    }
    
    private void SpawnGroundBaddies(){
    
    }
    

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 12);
    }

    /// <summary>
    /// The Player has walked into the horde zone.
    /// Active the baddie spawners!
    /// </summary>
    /// <param name="v"></param>
    /// <param name="c"></param>
    private void Apply(Vector3 v, Collider2D c) {
        SetCameraTarget(transform, false);
        _spawningAllowed = true;
    }
        
    private void SetCameraTarget(Transform target, bool activator){
        var camFollow = Camera.GetComponent<Camera2DFollow>();
        camFollow.Target = target;
        // Disable the simple collider and activator as well. Not that this is necessary but why leave it running.
        Camera.GetComponent<BaddieActivator>().enabled = activator;
        Camera.GetComponent<SimpleCollider>().enabled = activator;
    }
}
