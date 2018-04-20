using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HordeController : MonoBehaviour {
    /// <summary>
    /// Indicates if we should start spawning enemies or not
    /// </summary>
    private bool _spawningAllowed = false;
    
    /// <summary>
    /// How long we should wait in between spawning more baddies.
    /// We don't want to immedately spawn replacement baddies everytime one dies. (once a frame)
    /// Instead check once every second
    /// </summary>
    private float _spawnDelay = 1f;
    /// <summary>
    /// INnicates the time that needs to pass before we can spawn baddies back in.
    /// </summary>
    private float _spawnWaitTime;
    
    /// <summary>
    /// Necessary for collisions
    /// </summary>
    private SimpleCollider Collider;
    /// <summary>
    /// Reference to the main camera
    /// </summary>
    public Transform Camera;

    // Reference to all the currently active baddies so we can stop them all, kill them all
    private Dictionary<string, Transform> ActiveHordeBaddieCache = new Dictionary<string, Transform>();
    
    /// <summary>
    /// Total baddies left to kill before player can move on
    /// </summary>
    public int BaddiesLeftToKill = 20;
    
    // Reference to the baddie prefab - DEFINITELY TODO: add these to the runtime gamemaster
    public Transform GL1BaddiePrefab;
    // Indicates how many of each type of baddie is allowed on screen at any one point
    public int MaxGL1Count = 10;
    // Keeps a count of how many baddies of this type on on screen
    private int _activeGL1Count = 0;
    
    // Reference to the baddie prefab - DEFINITELY TODO: add these to the runtime gamemaster
    public Transform GL2BaddiePrefab;
    public int MaxGL2Count = 3;
    private int _activeGL2Count = 0;
    
    // Reference to the baddie prefab - DEFINITELY TODO: add these to the runtime gamemaster
    public Transform FL1BaddiePrefab;
    public int MaxFL1Count = 5;
    private int _activeFL1Count = 0;
    
    // Reference to the baddie prefab - DEFINITELY TODO: add these to the runtime gamemaster
    public Transform FL2BaddiePrefab;
    public int MaxFL2Count = 4;
    private int _activeFL2Count = 0;
    
    // Reference to the baddie prefab - DEFINITELY TODO: add these to the runtime gamemaster
    public Transform FL3BaddiePrefab;
    public int MaxFL3Count = 1;
    private int _activeFL3Count = 0;
    
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
            throw new MissingComponentException("No Flying Spawns specified");
        }
        if(GroundSpawns == null){
            throw new MissingComponentException("No Flying Spawns specified");
        }
    }
    
    void Update(){
        if(!_spawningAllowed){
            return;
        }
        
        if(BaddiesLeftToKill <= 0){
            _spawningAllowed = false;
            KillAllBaddies();
        }
        if(Time.time > _spawnWaitTime){
            _spawnWaitTime = Time.time + _spawnDelay;
            SpawnBaddies();
        }
    }
    
    private void SpawnBaddies(){
        SpawnFlyingBaddies();
        //SpawnGroundBaddies();
    }
    
    // Goes through all the baddies still alive on screen and kills them 0.1 second from eachother
    private void KillAllBaddies(){
        var deathOffset = 0.1f;
        foreach(var baddie in ActiveHordeBaddieCache.ToArray()){
            baddie.Value.GetComponent<DamageableLifeform>().DestroyBaddie(false, deathOffset);
            deathOffset += 0.1f;
        }
        EndHorde();
    }
    
    private void EndHorde(){
        var player = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);
         SetCameraTarget(player.transform, true, 5f);
        
        // Here we should also open the path to let the player out
        
        // Then destroy the spawns, and destroy ourself
        foreach(var spawn in FlyingSpawns){
            GameObject.Destroy(spawn);
        }
        foreach(var spawn in GroundSpawns){
            GameObject.Destroy(spawn);
        }
        Destroy(gameObject);
    }
    
    private void InstantiateBaddies(string baddieCachePrefix, int numBaddies, int maxBaddies, Transform baddiePrefab, Vector3 position, bool yOffset){
        // This just stops the baddies from spawning literally on top of one another
        var offset = baddiePrefab.GetComponent<Renderer>().bounds.size;
            // We need to spawn the difference of Max and active
            for (var i = 0; i <= numBaddies; ++i){
                var cleanPosition = new Vector3((!yOffset ? position.x + offset.x : position.x), (yOffset ? position.y + offset.y : position.y), position.z);
                Transform baddieTransform = Instantiate(baddiePrefab, cleanPosition, transform.rotation) as Transform;
                baddieTransform.gameObject.name = baddieCachePrefix+baddieTransform.gameObject.GetInstanceID();
                
                var damageableLifeform = baddieTransform.GetComponent<DamageableLifeform>();
                if(damageableLifeform == null){
                    throw new MissingComponentException("Somehow the baddie: " + baddieTransform.gameObject.name + " does not have a DamageableLifeform script attached");
                }
                damageableLifeform.enabled = true;
                // Add delegate onto the created baddie so when it dies it can inform the hordecontroller to update the counts
                damageableLifeform.InvokeHordeUpdate += UpdateBaddieCount;
                print("Adding baddie: " + baddieTransform.gameObject.name);
                // Add this baddie to the cache
                // Key = prefix-gameobject.name
                // Value = actual transform
                ActiveHordeBaddieCache.Add(baddieTransform.gameObject.name, baddieTransform);
                offset *= 1.5f;
            }
    }
    
    private void UpdateBaddieCount(string baddieName){
        var baddieNameKey = baddieName.Substring(0, 3);
        // Try to get the baddie from the cache, and remove it
        Transform outBaddie;
        if(ActiveHordeBaddieCache.TryGetValue(baddieName, out outBaddie)){
            ActiveHordeBaddieCache.Remove(baddieName);
        }
        // Decrement the appropriate count 
        switch(baddieName){
            case "GL1":
                --_activeGL1Count;
                break;
            case "GL2":
                --_activeGL2Count;
                break;
            case "FL1":
                --_activeFL1Count;
                break;
            case "FL2":
                --_activeFL2Count;
                break;
            case "FL3":
                --_activeFL3Count;
                break;
        }
        
        // Decrement the baddies left to kill
        --BaddiesLeftToKill;
    }
    
    private void SpawnFlyingBaddies(){
        var rand = Random.Range(0, 2);
        if(_activeFL1Count < MaxFL1Count){
            InstantiateBaddies("FL1-", 1, MaxFL1Count, FL1BaddiePrefab, FlyingSpawns[0].position, true);
            _activeFL1Count = MaxFL1Count;
        }
        //if (_activeFL2Count < MaxFL2Count) {
        //    rand = Random.Range(1, 3);
        //    InstantiateBaddies("FL2-", (MaxFL2Count - _activeFL2Count), MaxFL2Count, FL2BaddiePrefab, FlyingSpawns[1].position, true);
        //    _activeFL2Count = MaxFL2Count;
        //}
        //if (_activeFL3Count < MaxFL3Count) {
        //    rand = Random.Range(0, 3);
        //    InstantiateBaddies("FL3-", (MaxFL3Count - _activeFL3Count), MaxFL3Count, FL3BaddiePrefab, FlyingSpawns[2].position, true);
        //    _activeFL3Count = MaxFL3Count;
        //}
    }
    
    private void SpawnGroundBaddies(){
         var rand = (int)Random.Range(0, GroundSpawns.Length);
        if(_activeGL1Count < MaxGL1Count){
            InstantiateBaddies("GL1-", (MaxGL1Count - _activeGL1Count), MaxGL1Count, GL1BaddiePrefab, GroundSpawns[0].position, false);
            _activeGL1Count = MaxGL1Count;
        }
        if(_activeFL2Count < MaxFL2Count){
            rand = Random.Range(0, GroundSpawns.Length);
            InstantiateBaddies("GL2-", (MaxGL2Count - _activeGL2Count), MaxGL2Count, GL2BaddiePrefab, GroundSpawns[1].position, false);
            _activeGL2Count = MaxGL2Count;
        }
    }
    

    //void OnDrawGizmosSelected() {
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawSphere(transform.position, 12);
    //}

    /// <summary>
    /// The Player has walked into the horde zone.
    /// Active the baddie spawners!
    /// </summary>
    /// <param name="v"></param>
    /// <param name="c"></param>
    private void Apply(Vector3 v, Collider2D c) {
        // Here we should also close the path on either side of the horde section locking the player in

        SetCameraTarget(transform, false, 0f);
        _spawningAllowed = true;
    }
        
    private void SetCameraTarget(Transform target, bool activator, float yOffset){
        var camFollow = Camera.GetComponent<Camera2DFollow>();
        camFollow.Target = target;
        camFollow.YOffset = yOffset;
        // Disable the simple collider and activator as well. Not that this is necessary but why leave it running.
        Camera.GetComponent<BaddieActivator>().enabled = activator;
        Camera.GetComponent<SimpleCollider>().enabled = activator;
    }
}
