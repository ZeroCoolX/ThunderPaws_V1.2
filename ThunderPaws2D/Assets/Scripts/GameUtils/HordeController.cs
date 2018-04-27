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
    /// Reference to the left wall we activate to lock the player in duing a horde session
    /// </summary>
    public Transform LeftBarrier;
    /// <summary>
    /// Reference to the right wall we activate to lock the player in duing a horde session
    /// </summary>
    public Transform RightBarrier;
    
    /// <summary>
    /// Necessary for collisions
    /// </summary>
    private SimpleCollider Collider;
    /// <summary>
    /// Reference to the main camera
    /// </summary>
    public Transform Camera;

    private float GL1SpawnRate;
    private float FL1SpawnRate;
    private float FL2SpawnRate;
    private float FL3SpawnRate;

    // Reference to all the currently active baddies so we can stop them all, kill them all
    private Dictionary<string, Transform> ActiveHordeBaddieCache = new Dictionary<string, Transform>();
    
    /// <summary>
    /// Total baddies left to kill before player can move on
    /// </summary>
    public int BaddiesLeftToKill = 25;

    private int _baddieKillNumBackup;

    public bool EndGameAfter = false;
    
    // Reference to the baddie prefab - DEFINITELY TODO: add these to the runtime gamemaster
    public Transform GL1BaddiePrefab;
    // Indicates how many of each type of baddie is allowed on screen at any one point
    public int MaxGL1Count;
    // Keeps a count of how many baddies of this type on on screen
    private int _activeGL1Count = 0;
    
    // Reference to the baddie prefab - DEFINITELY TODO: add these to the runtime gamemaster
    public Transform GL2BaddiePrefab;
    public int MaxGL2Count;
    private int _activeGL2Count = 0;
    
    // Reference to the baddie prefab - DEFINITELY TODO: add these to the runtime gamemaster
    public Transform FL1BaddiePrefab;
    public int MaxFL1Count;
    private int _activeFL1Count = 0;
    
    // Reference to the baddie prefab - DEFINITELY TODO: add these to the runtime gamemaster
    public Transform FL2BaddiePrefab;
    public int MaxFL2Count;
    private int _activeFL2Count = 0;
    
    // Reference to the baddie prefab - DEFINITELY TODO: add these to the runtime gamemaster
    public Transform FL3BaddiePrefab;
    public int MaxFL3Count;
    private int _activeFL3Count = 0;
    
    /// <summary>
    /// Holds the flying spawn points
    /// </summary>
    public Transform[] FlyingSpawns;
    /// <summary>
    /// Holds the ground spawn points
    /// </summary>
    public Transform[] GroundSpawns;

    public int RadiusOfTrigger = 12;

    private bool PlayerDiedHack = false;

    // Use this for initialization
    void Start() {
        //Add delegate for collision detection
        Collider = GetComponent<SimpleCollider>();
        if (Collider == null) {
            throw new MissingComponentException("No collider for this object");
        }
        Collider.InvokeCollision += Apply;
        Collider.Initialize(1 << 8, RadiusOfTrigger);
        
        if(FlyingSpawns == null){
            throw new MissingComponentException("No Flying Spawns specified");
        }
        if(GroundSpawns == null){
            throw new MissingComponentException("No Flying Spawns specified");
        }
        GameMaster.Instance.OnHordeKilledPlayer += PlayerDiedReset;

        _baddieKillNumBackup = BaddiesLeftToKill;
    }

    void Update(){
        if (PlayerDiedHack) {
            var player = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);
            if(player != null) {
                SetCameraTarget(player.transform, true, 5f);
                PlayerDiedHack = false;
            }
        }

        if(!_spawningAllowed){
            return;
        }
        
        if(BaddiesLeftToKill <= 0){
            _spawningAllowed = false;
            KillAllBaddies();
        }
        if(Time.time > _spawnWaitTime){
            print("SPAWN BADDIES!");
            _spawnWaitTime = Time.time + _spawnDelay;
            SpawnBaddies();
        }
    }
    
    private void SpawnBaddies(){
        SpawnFlyingBaddies();
        if (Time.time > GL1SpawnRate) {
            // Wait betweeen 5 and 10 seconds to spawn some more ground types
            GL1SpawnRate = Time.time + Random.Range(5, 10);
            SpawnGroundBaddies();
        }
    }

    private void PlayerDiedReset() {
        GameMaster.Instance.AudioManager.stopSound("Music_Boss");
        GameMaster.Instance.AudioManager.playSound("Music_Main");
        PlayerDiedHack = true;
        // Inform collider to reset iteself
        Collider.Initialize(1 << 8, RadiusOfTrigger);

        // Clear cache
        ActiveHordeBaddieCache = new Dictionary<string, Transform>();
        var baddies = GameObject.FindGameObjectsWithTag(GameConstants.Tag_HordeBaddie);

        // DEstroy every baddie
        foreach (var baddie in baddies) {
            try {
                Destroy(baddie);
            } catch (System.Exception e) {
                print("Trying to destroy already destroyed object, no worries move on");
            }
        }
        _activeFL1Count = 0;
        _activeFL2Count = 0;
        _activeFL3Count = 0;
        _activeGL1Count = 0;
        _activeGL2Count = 0;
        _spawnWaitTime = Time.time;
        BaddiesLeftToKill = _baddieKillNumBackup;
        _spawningAllowed = false;
        if (LeftBarrier != null) {
            LeftBarrier.gameObject.SetActive(false);
        }
    }
    
    // Goes through all the baddies still alive on screen and kills them 0.1 second from eachother
    private void KillAllBaddies(){
        var deathOffset = 0.1f;
        foreach(var baddie in ActiveHordeBaddieCache.ToArray()){
            try {
                baddie.Value.GetComponent<CollisionController2D>().enabled = false;
                baddie.Value.GetComponent<DamageableLifeform>().DestroyBaddie(false, deathOffset);
                deathOffset += 0.1f;
            }catch(System.Exception e) {
                print("Trying to disable already destroyed object, no worries move on");
            }
        }
        EndHorde();
    }
    
    private void EndHorde(){
        var player = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);
         SetCameraTarget(player.transform, true, 5f);

        // Failsafe just so the player doesn't die after beatig the horde
        player.GetComponent<Player>().RegenerateAllHealth();

        // Here we should also open the path to let the player out
        GameMaster.Instance.LastSeenInHorde = false;

        // Then destroy the spawns, and destroy ourself
        foreach (var spawn in FlyingSpawns){
            GameObject.Destroy(spawn.gameObject);
        }
        foreach(var spawn in GroundSpawns){
            GameObject.Destroy(spawn.gameObject);
        }
        Invoke("ActivateExit", 5f);
    }

    private void ActivateExit() {
        GameMaster.Instance.AudioManager.stopSound("Music_Boss");
        if (RightBarrier != null) {
            RightBarrier.gameObject.SetActive(false);
        }
        if (!EndGameAfter) {
            // Was Horde 1
            GameMaster.Instance.CalculateHordeScore(1);
            GameMaster.Instance.AudioManager.playSound("Music_Main");
            Destroy(gameObject);
        }else {
            GameMaster.Instance.CalculateHordeScore(2);
            print("OMG YOU COMPLETED THE GAME!!!!");
            GameMaster.Instance.GameOver();
        }
    }

    private void InstantiateBaddies(string baddieCachePrefix, int numBaddies, int maxBaddies, Transform baddiePrefab, Vector3 position, bool yOffset, int invertFactor = 1){
        // This just stops the baddies from spawning literally on top of one another
        var size = baddiePrefab.GetComponent<Renderer>().bounds.size;
        var offset = size;
            // We need to spawn the difference of Max and active
            for (var i = 0; i <= numBaddies; ++i){
                var cleanPosition = new Vector3((!yOffset ? position.x + offset.x : position.x), (yOffset ? position.y + offset.y : position.y), position.z);
                Transform baddieTransform = Instantiate(baddiePrefab, cleanPosition, transform.rotation) as Transform;
                baddieTransform.gameObject.name = baddieCachePrefix+baddieTransform.gameObject.GetInstanceID();
                baddieTransform.tag = GameConstants.Tag_HordeBaddie;
                var damageableLifeform = baddieTransform.GetComponent<DamageableLifeform>();
                if(damageableLifeform == null){
                    throw new MissingComponentException("Somehow the baddie: " + baddieTransform.gameObject.name + " does not have a DamageableLifeform script attached");
                }
                damageableLifeform.enabled = true;
                damageableLifeform.PartOfHorde = true;
                // Add delegate onto the created baddie so when it dies it can inform the hordecontroller to update the counts
                damageableLifeform.InvokeHordeUpdate += UpdateBaddieCount;
                print("Adding baddie: " + baddieTransform.gameObject.name);
                // Add this baddie to the cache
                // Key = prefix-gameobject.name
                // Value = actual transform
                ActiveHordeBaddieCache.Add(baddieTransform.gameObject.name, baddieTransform);
                offset += (size * 1.5f * invertFactor);
            }
    }
    
    private void UpdateBaddieCount(string baddieName){
        var baddieNameKey = baddieName.Substring(0, 3);
        // Try to get the baddie from the cache, and remove it
        Transform outBaddie;
        if(ActiveHordeBaddieCache.TryGetValue(baddieName, out outBaddie)){
            print("Removing baddie: " + baddieName);
            ActiveHordeBaddieCache.Remove(baddieName);
        }
        print("Update baddie count - decrement : " + baddieNameKey);
        // Decrement the appropriate count 
        switch(baddieNameKey) {
            case "GL1":
                _activeGL1Count = _activeGL1Count - 1;
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
        var rand = (int)Random.Range(0, 10);
        if (Time.time > FL1SpawnRate && _activeFL1Count < MaxFL1Count){
            // WAit between 1 and 4 seconds to spawn new badddies
            FL1SpawnRate = Time.time + Random.Range(1, 5);
            InstantiateBaddies("FL1-", (MaxFL1Count - _activeFL1Count), MaxFL1Count, FL1BaddiePrefab, FlyingSpawns[rand <= 3 ? 0 : 1].position, false);
            _activeFL1Count = MaxFL1Count;
        }
        if (Time.time > FL2SpawnRate && _activeFL2Count < MaxFL2Count) {
            FL2SpawnRate = Time.time + Random.Range(3, 6);
            InstantiateBaddies("FL2-", (MaxFL2Count - _activeFL2Count), MaxFL2Count, FL2BaddiePrefab, FlyingSpawns[rand > 3 && rand <= 5 ? 1 : 2].position, false);
            _activeFL2Count = MaxFL2Count;
        }
        if (Time.time > FL1SpawnRate && _activeFL3Count < MaxFL3Count) {
            FL3SpawnRate = Time.time + Random.Range(3, 10);
            InstantiateBaddies("FL3-", (MaxFL3Count - _activeFL3Count), MaxFL3Count, FL3BaddiePrefab, FlyingSpawns[rand > 6 ? 2 : 0].position, false);
            _activeFL3Count = MaxFL3Count;
        }
    }
    
    private void SpawnGroundBaddies(){
         var rand = (int)Random.Range(0, 9);
        if (_activeGL1Count < MaxGL1Count) {
            InstantiateBaddies("GL1-", (MaxGL1Count - _activeGL1Count), MaxGL1Count, GL1BaddiePrefab, GroundSpawns[rand % 2 == 0 ? 0 : 1].position, false, (rand % 2 != 0) ? -1 : 1);
            _activeGL1Count = MaxGL1Count;
        }
        //if (_activeGL2Count < MaxGL2Count) {
        //    InstantiateBaddies("GL2-", (MaxGL2Count - _activeGL2Count), MaxGL2Count, GL2BaddiePrefab, GroundSpawns[rand % 2 == 0 ? 1 : 0].position, false);
        //    _activeGL2Count = MaxGL2Count;
        //}
    }


    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 5);
    }

    /// <summary>
    /// The Player has walked into the horde zone.
    /// Active the baddie spawners!
    /// </summary>
    /// <param name="v"></param>
    /// <param name="c"></param>
    private void Apply(Vector3 v, Collider2D c) {
        // Here we should also close the path on either side of the horde section locking the player in
        GameMaster.Instance.LastSeenInHorde = true;

        SetCameraTarget(transform, false, 0f);
        _spawningAllowed = true;
        if (LeftBarrier != null) {
            LeftBarrier.gameObject.SetActive(true);
        }

        GameMaster.Instance.AudioManager.stopSound("Music_Main");
        GameMaster.Instance.AudioManager.playSound("Music_Boss");
    }
        
    private void SetCameraTarget(Transform target, bool activator, float yOffset){
        var camFollow = Camera.GetComponent<Camera2DFollow>();
        camFollow.Target = target;
        camFollow.YOffset = yOffset;
        // Disable the simple collider and activator as well. Not that this is necessary but why leave it running.
        if(target.gameObject.tag == GameConstants.Tag_Player) {
            target.GetComponent<BaddieActivator>().enabled = activator;
            target.GetComponent<SimpleCollider>().enabled = activator;
        }
    }
}
