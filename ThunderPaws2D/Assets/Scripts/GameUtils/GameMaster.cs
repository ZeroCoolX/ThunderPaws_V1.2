using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Overseer of the game.
/// Handles static mappings needed throughout and persists data which needs to be carried over from level to level/ life to life...etc
/// </summary>
public class GameMaster : MonoBehaviour {
    /// <summary>
    /// Static reference for anyone who needs it
    /// </summary>
    public static GameMaster Instance;

    /// <summary>
    /// Compile time collection of any sprites that need to be swapped out during the course of the game
    /// </summary>
    public Sprite[] PlayerSprites = new Sprite[6];
    /// <summary>
    /// Mapping from angle of rotation to sprite
    /// </summary>
    private Dictionary<int, Sprite> _playerSpiteMap = new Dictionary<int, Sprite>();

    public Transform[] WeaponList = new Transform[3];
    private Dictionary<string, Transform> _weaponMap = new Dictionary<string, Transform>();

    /// <summary>
    /// Delegate for switching weapons
    /// </summary>
    /// <param name="choice"></param>
    public delegate void WeaponSwitchCallback();
    public WeaponSwitchCallback OnWeaponSwitch;

    /// <summary>
    /// CameraShake instance so we know we can shake the screen
    /// </summary>
    public CameraShake CamShake { get; private set; }

    /// <summary>
    /// Reference to the UI layer
    /// </summary>
    public GameObject UIOverlay;
    /// <summary>
    /// Player 1 stats UI
    /// </summary>
    private PlayerStatsUIController _player1StatsUi;
    /// <summary>
    /// Player 2 stats UI
    /// </summary>
    private PlayerStatsUIController _player2StatsUi;

    /// <summary>
    /// index 0 = player 1 coins per level
    /// index 1 = player 2 coins per level
    /// </summary>
    private int[] _playerCoinCounts = new int[2];

    /// <summary>
    /// Max lives per game
    /// </summary>
    [SerializeField]
    private int _maxLives = 3;

    /// <summary>
    /// Determines when we have ended the game
    /// </summary>
    [SerializeField]
    private static int _remainingLives;

    /// <summary>
    /// Player reference for respawning
    /// </summary>
    public Transform Player;
    /// <summary>
    /// Collection of all possible places of where to respawn the player 
    /// </summary>
    public Transform[] SpawnPoints;
    /// <summary>
    /// Indicates which spawn point the player should spawn from
    /// Used for checkpoints
    /// </summary>
    public int SpawnPointIndex;
    /// <summary>
    /// How long to wait from player death to respawn
    /// </summary>
    public int SpawnDelay = 3;

    /// <summary>
    /// Remaining lives counter must persist through player deaths
    /// </summary>
    public int RemainingLives { get { return _remainingLives; } set { _remainingLives = value; } }

    /// <summary>
    /// This is the world to screen point where any collected coin should go
    /// </summary>
    public Vector3 CoinCollectionOrigin {
        get {
            var collectionPoint = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
            //We dont want the exact corner, only relatively
            //collectionPoint.x += 3;
            return collectionPoint;
        }
        private set {
            CoinCollectionOrigin = value;
        }
    }

    // Use this for initialization
    void Awake () {
        if (Instance == null) {
            Instance = GameObject.FindGameObjectWithTag(GameConstants.Tag_GameMaster).GetComponent<GameMaster>();
        }

        //Load sprites for player animation map
        _playerSpiteMap.Add(0, PlayerSprites[0]);
        _playerSpiteMap.Add(45, PlayerSprites[1]);
        _playerSpiteMap.Add(90, PlayerSprites[2]);
        //Load sprites for player health UI state
        _playerSpiteMap.Add(100, PlayerSprites[3]);
        _playerSpiteMap.Add(50, PlayerSprites[4]);
        _playerSpiteMap.Add(25, PlayerSprites[5]);

        //Load weapon map
        _weaponMap.Add(WeaponList[0].gameObject.name, WeaponList[0]);
        _weaponMap.Add(WeaponList[1].gameObject.name, WeaponList[1]);
        _weaponMap.Add(WeaponList[2].gameObject.name, WeaponList[2]);
    }

    private void Start() {
        CamShake = transform.GetComponent<CameraShake>();
        if (CamShake == null) {
            throw new MissingReferenceException("No CameraShake found on gamemaster");
        }
        //Set player stats UI reference
        _player1StatsUi = GetPlayerStatsUi(1);

        //Double check that there is at least one spawn point in this level
        if (SpawnPoints.Length <= 0) {
            throw new MissingReferenceException("No spawn points for this level");
        }
        SpawnPointIndex = -1;
        //var spawn = SpawnPoints[0];
        //var controller = spawn.GetComponent<CheckpointController>();
        //if (controller == null) {
        //    throw new MissingComponentException("No Checkpoint controller");
        //}
        //controller.SpawnFreshBaddiesForCheckpoint();
        //Set remaining lives
        _remainingLives = _maxLives;
    }

    private void Update() {
        if (Input.GetButtonUp(GameConstants.Input_Xbox_LBumper) || Input.GetKeyUp(KeyCode.UpArrow)) {
            OnWeaponSwitch.Invoke();
        }
    }

    public void UpdateHealthUI(int player, int current, int max) {
        if (player == 1) {
            _player1StatsUi.SetHealthStatus(current, max);
        } else {
            throw new System.Exception("This is bad because there is only one player...");
        }
    }

    public void UpdateUltimateUI(int player, int current, int max) {
        if(player == 1) {
            _player1StatsUi.SetUltimateStatus(current, max);
        }else {
            throw new System.Exception("This is bad because there is only one player...");
        }
    }

    /// <summary>
    /// Add to the global coin count for a player
    /// </summary>
    /// <param name="index"></param>
    public void AddCoins(int index) {
        ++_playerCoinCounts[index];
    }

    /// <summary>
    /// Return the current number of coins
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public int GetCoinCount(int index) {
        return _playerCoinCounts[index];
    }

    /// <summary>
    /// Based off the key supplied return the corresponding weapon from the map
    /// </summary>
    /// <param name="weaponKey"></param>
    /// <returns></returns>
    public Transform GetWeaponFromMap(string weaponKey) {
        Transform weapon;
        _weaponMap.TryGetValue(weaponKey, out weapon);
        if(weapon == null) {
            weapon = WeaponList[0];
        }
        return weapon;
    }

    /// <summary>
    /// Based off a positive angle value, get the corresponding sprite from the map
    /// </summary>
    /// <param name="degreeKey"></param>
    /// <returns></returns>
    public Sprite GetSpriteFromMap(int degreeKey) {
        Sprite sprite;
        _playerSpiteMap.TryGetValue(degreeKey, out sprite);
        if(sprite == null) {
            sprite = PlayerSprites[0];
            //throw new KeyNotFoundException("degreeKey: " + degreeKey + " did not exist in the mapping");
        }
        return sprite;
    }

    /// <summary>
    /// Helper to extract the player ui objects
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public PlayerStatsUIController GetPlayerStatsUi(int player) {
        var statsName = "Player" + player + "Stats";
        var stats = UIOverlay.transform.Find(statsName).GetComponent<PlayerStatsUIController>();
        if (stats == null) {
            throw new MissingComponentException("Attempted to extract " + statsName + " but found none");
        }
        return stats;
    }

    /// <summary>
    /// Decrement lives, generate particles, shake camera and destroy current player reference
    /// </summary>
    /// <param name="player"></param>
    public static void KillPlayer(Player player) {
        //decrement lives
        --_remainingLives;

        //Generate death particles
        //Transform clone = Instantiate(player.DeathParticles, player.transform.position, Quaternion.identity) as Transform;
        //Destroy(clone.gameObject, 3f);

        //Generate camera shake
        //Instance.CamShake.Shake(player.ShakeAmount, player.ShakeLength);

        //kill the player if necessary
        Instance.KillDashNine(player.gameObject, _remainingLives > 0);
    }

    /// <summary>
    /// Actual destruction of optional respawn
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="respawn"></param>
    private void KillDashNine(GameObject obj, bool respawn) {
        Destroy(obj);
        if (respawn) {
            Instance.StartCoroutine(Instance.RespawnPlayer());
        } else {
            if (RemainingLives <= 0) {
                print("GAME OVER DUDE");
                //_audioManager.playSound(GameOverSoundName);
                //GameOverUI.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Respawn player
    /// </summary>
    /// <returns></returns>
    private IEnumerator RespawnPlayer() {
        //play sound and wait for delay
        //_audioManager.playSound(RespawnCountdownSoundName);
        yield return new WaitForSeconds(SpawnDelay);

        var spawn = SpawnPoints[SpawnPointIndex];
        var controller = spawn.GetComponent<CheckpointController>();
        if(controller == null) {
            throw new MissingComponentException("No Checkpoint controller");
        }
        controller.DeactivateBaddiesInCheckpoint();
        controller.SpawnFreshBaddiesForCheckpoint();

        //_audioManager.playSound(SpawnSoundName);
        Instantiate(Player, SpawnPoints[SpawnPointIndex].position, SpawnPoints[SpawnPointIndex].rotation);
    }

}
