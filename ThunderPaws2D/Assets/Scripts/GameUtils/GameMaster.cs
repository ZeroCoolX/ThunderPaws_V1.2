using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Overseer of the game.
/// Handles static mappings needed throughout and persists data which needs to be carried over from level to level/ life to life...etc
/// </summary>
public class GameMaster : MonoBehaviour {
    /// <summary>
    /// Static reference for anyone who needs it
    /// </summary>
    public static GameMaster Instance;

    public int BaddiesKilled;
    public int FuzzBusterAmmo;
    public int ShotgunAmmo;

    public bool PopulatePlayers;

    /// <summary>
    /// Compile time collection of any sprites that need to be swapped out during the course of the game
    /// </summary>
    public Sprite[] PlayerSprites = new Sprite[6];
    /// <summary>
    /// Mapping from angle of rotation to sprite
    /// </summary>
    private Dictionary<int, Sprite> _playerSpiteMap = new Dictionary<int, Sprite>();

    public Transform[] WeaponList = new Transform[5];
    private Dictionary<string, Transform> _weaponMap = new Dictionary<string, Transform>();

    /// <summary>
    /// Delegate for switching weapons
    /// </summary>
    /// <param name="choice"></param>
    public delegate void WeaponSwitchCallback();
    public WeaponSwitchCallback OnWeaponSwitch;

    /// <summary>
    /// Delegate for snotifying the horde the player died, so reset the camera and kill themselves
    /// </summary>
    /// <param name="choice"></param>
    public delegate void PlayerDeadResetHordeCallback();
    public PlayerDeadResetHordeCallback OnHordeKilledPlayer;

    public bool LastSeenInHorde = false;

    /// <summary>
    /// CameraShake instance so we know we can shake the screen
    /// </summary>
    public CameraShake CamShake { get; private set; }

    /// <summary>
    /// Reference to the UI layer
    /// </summary>
    public GameObject UIOverlay;
    /// <summary>
    /// Reference to the game over screen
    /// </summary>
    public Transform GameOverUi;
    public Transform ControlScreen;
    public Transform DifficultyScreen;
    public Transform DifficultyExit;
    public Transform GameOverLostScreen;
    public Transform InputBindingScreen;
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
    /// Allows the user to select between 3 difficulties
    /// </summary>
    public int SpecialOverrideHealth;

    private Dictionary<string, int[]> _difficulties = new Dictionary<string, int[]>();
    public Transform[] DifficultyObjects = new Transform[3];
    /// <summary>
    /// Set by the player in the menu. Default is easy
    /// </summary>
    public string Difficulty = GameConstants.Difficulty_Easy;

    /// <summary>
    /// Determines when we have ended the game
    /// </summary>
    [SerializeField]
    private static int _remainingLives;

    /// <summary>
    /// Player array holding 1-2 players depending on how many are playing
    /// </summary>
    public List<Transform> Players;
    public Transform BarneyPrefab;
    public Transform RupertPrefab;
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
    /// if this is single payer - respawn them almost immeddiately.
    /// Otherwise, if this is Co-op wait a significantly longer time since one player dying doesn't subtract lives
    /// </summary>
    public int SpawnDelay { get { return SinglePlayer || FullRespawnOccurred ? 3 : 10; } }

    public bool ShowDifficultyScreen = false;

    /// <summary>
    /// Super rudimentary score system
    /// 
    /// </summary>
    public int Score { get; set; }
    // Indicates if this is single player by checking the Players list count
    public bool SinglePlayer { get { return Players != null && Players.Count == 1; } }

    // Indicates while one player was dead and waiting to respawn - the other player died
    // and when that coroutine ends it should do nothing.
    private bool FullRespawnOccurred;

    // Keeps track of how many players are actively alive in the world.
    // Useful for determining if we should decrement a life and respawn them
    // The integer value is irrelevent
    public Stack<int> PlayersCurrentlyAlive = new Stack<int>();

    private int MaxScorePossible;

    /// <summary>
    /// Remaining lives counter must persist through player deaths
    /// </summary>
    public int RemainingLives { get { return _remainingLives; } set { _remainingLives = value; } }

    public AudioManager AudioManager;

    private bool _pauseHackIndicator = false;

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

        if (PopulatePlayers) {
            PopulatePlayerArray();
        }
        SpawnPlayers();

        if (ShowDifficultyScreen) {
            DifficultyScreen.gameObject.SetActive(true);
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
        _weaponMap.Add(WeaponList[3].gameObject.name, WeaponList[3]);
        _weaponMap.Add(WeaponList[4].gameObject.name, WeaponList[4]);

        // Difficulty, [lives, max health]
        _difficulties.Add(GameConstants.Difficulty_Easy, new int[] { 10, 500 });
        _difficulties.Add(GameConstants.Difficulty_Normal, new int[] { 5, 250 });
        _difficulties.Add(GameConstants.Difficulty_Hard, new int[] { 3, 100 });

        _remainingLives = LivesManager.Lives;
        print("lives = " + LivesManager.Lives);
    }

    // Based off how many joysticks are connected populate the player array
    public void PopulatePlayerArray() {
        print("Populating Player Array!");
        // check if there is only 1 player or we're using a KB
        var prefix = "";
        if (JoystickManagerController.Instance.ControllerMap.Count < 2) {
            print("there was only 0-1 players in map so set player 1 and leave");
            JoystickManagerController.Instance.ControllerMap.TryGetValue(1, out prefix);
            BarneyPrefab.GetComponent<Player>().JoystickId = prefix;
            print("Set BarneyPrefab.JoystickId to " + prefix);
            Players.Add(BarneyPrefab);
            return;
        }
        // Assign Player 1
        prefix = "";
        JoystickManagerController.Instance.ControllerMap.TryGetValue(1, out prefix);
        BarneyPrefab.GetComponent<Player>().JoystickId = prefix;
        print("Set BarneyPrefab.JoystickId to " + prefix);
        Players.Add(BarneyPrefab);

        // Assign Player 2
        prefix = "";
        JoystickManagerController.Instance.ControllerMap.TryGetValue(2, out prefix);
        RupertPrefab.GetComponent<Player>().JoystickId = prefix;
        print("Set RupertPrefab.JoystickId to " + prefix);
        Players.Add(RupertPrefab);

    }

    // Spawn as many players that live in the array
    public void SpawnPlayers() {
        // Spawn the allotted number of players into the room
        var startSpawns = GameObject.FindGameObjectsWithTag(GameConstants.Tag_StartSpawn);
        var spawnIndex = 0;
        foreach(var player in Players) {
            Instantiate(player, startSpawns[spawnIndex].transform.position, startSpawns[spawnIndex].transform.rotation);
            print("Setting lives for player " + player.GetComponent<Player>().PlayerNumber);
            GetPlayerStatsUi(player.GetComponent<Player>().PlayerNumber).SetLives(_remainingLives);
            print("success");
            // Push a player onto the stack indicating he is alive in the scene
            PlayersCurrentlyAlive.Push(0);
            ++spawnIndex;
        }
    }

    public void SetDifficulty() {
        int[] values;
        var livesAndHealth = _difficulties.TryGetValue(Difficulty.ToLower(), out values);
        if (values == null) {
            print("Something went wrong - using the default difficulty");
            livesAndHealth = _difficulties.TryGetValue("easy", out values);
        }
        LivesManager.Lives = values[0];
        print("Settiung heath to " + values[1]);
        LivesManager.Health = values[1];

        // Based on the difficulty selected set theit starting score
        MaxScorePossible = values[0] == 10 ? 200 : values[0] == 5 ? 400 : 600;

        var player = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player).GetComponent<Player>();
        if(player == null) {
            throw new MissingComponentException("The player is missing at the time of selecting a difficulty?!");
        }
        var playerStats = player.PlayerStats;
        if(playerStats == null) {
            throw new MissingComponentException("There are no playterstats on the player");
        }
        // Set the max health
        playerStats.MaxHealth = LivesManager.Health;
        // Set the max lives
        _maxLives = LivesManager.Lives;
        DifficultyScreen.gameObject.SetActive(false);
        DifficultyExit.gameObject.SetActive(false);
        foreach(var difficulty in DifficultyObjects) {
            difficulty.gameObject.SetActive(false);
        }

        GetPlayerStatsUi(1).SetLives(_maxLives);
    }

    private void Start() {
        AudioManager = AudioManager.instance;
        if (AudioManager == null) {
            throw new MissingComponentException("No AudioManager was found");
        }

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName(GameConstants.Scene_LevelName_Menu) || SceneManager.GetActiveScene() == SceneManager.GetSceneByName("PreAlphaDemoTutorial1")) {
            AudioManager.playSound(GameConstants.Audio_MenuMusic);
        }

        CamShake = transform.GetComponent<CameraShake>();
        if (CamShake == null) {
            throw new MissingReferenceException("No CameraShake found on gamemaster");
        }
        //Set player stats UI reference
        _player1StatsUi = GetPlayerStatsUi(1);
        if (Players.Count == 2) {
            _player2StatsUi = GetPlayerStatsUi(2);
        }

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
        _remainingLives = LivesManager.Lives;
        print("lives = " + LivesManager.Lives);
        GetPlayerStatsUi(1).SetLives(_remainingLives);
    }

    private void Update() {
        // Testing hack for music on and off
        if (Input.GetKeyDown(KeyCode.F)) {
            try {
                SetDifficulty();
            }catch(System.Exception e){
                print("Couldn't set difficulty");
            }
        }

        if (Input.GetKeyDown(KeyCode.B)) {
            InputBindingScreen.gameObject.SetActive(!InputBindingScreen.gameObject.activeSelf);
        }

        // Testing hack for pause
        if (Input.GetKeyDown(KeyCode.Escape)) {
            _pauseHackIndicator = !_pauseHackIndicator;
            ControlScreen.gameObject.SetActive(_pauseHackIndicator);
        }

        //// TODO - this needs to be changed anyways right now just hardcode it
        //if (Input.GetButtonUp("J1-" + GameConstants.Input_LBumper) || Input.GetKeyUp(InputManager.Instance.ChangeWeapon)) {
        //    OnWeaponSwitch.Invoke();
        //    AudioManager.playSound(GameConstants.Audio_WeaponSwitch);
        //}
    }

    public void UpdateHealthUI(int player, int current, int max) {
        print("Max health is " + max);
        if (player == 1) {
            if(_player1StatsUi == null) {
                _player1StatsUi = GetPlayerStatsUi(1);
            }
            _player1StatsUi.SetHealthStatus(current, max);
        } else {
            if (_player2StatsUi == null) {
                _player2StatsUi = GetPlayerStatsUi(2);
            }
            _player2StatsUi.SetHealthStatus(current, max);
        }
    }

    public void UpdateUltimateUI(int player, int current, int max) {
        if(player == 1) {
            if (_player1StatsUi == null) {
                _player1StatsUi = GetPlayerStatsUi(1);
            }
            _player1StatsUi.SetUltimateStatus(current, max);
        }else {
            if (_player2StatsUi == null) {
                _player2StatsUi = GetPlayerStatsUi(2);
            }
            _player2StatsUi.SetUltimateStatus(current, max);
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
        var statsUi = UIOverlay.transform.Find(statsName);
        if (!statsUi.gameObject.activeSelf) {
            statsUi.gameObject.SetActive(true);
        }
        var stats = statsUi.GetComponent<PlayerStatsUIController>();
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
        // Pop off a player from the stack
        Instance.PlayersCurrentlyAlive.Pop();
        var fullRespawn = false;
        // We should only decrement lives if there are no more alive players on the field!
        if (Instance.PlayersCurrentlyAlive.Count == 0) {
            print("Decrement lives");
            fullRespawn = true;
            //decrement lives
            --_remainingLives;
        }else {
            print("There is still someone fighting for their life!");
        }

        //Generate death particles
        //Transform clone = Instantiate(player.DeathParticles, player.transform.position, Quaternion.identity) as Transform;
        //Destroy(clone.gameObject, 3f);

        //Generate camera shake
        //Instance.CamShake.Shake(player.ShakeAmount, player.ShakeLength);

        //kill the player if necessary
        Instance.KillDashNine(player.gameObject, _remainingLives > 0, fullRespawn, player.PlayerNumber);
    }

    /// <summary>
    /// Actual destruction of optional respawn
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="respawn"></param>
    private void KillDashNine(GameObject obj, bool respawn,bool fullRespawn, int player) {
        Destroy(obj);
        if (respawn) {
            Instance.StartCoroutine(Instance.RespawnPlayer(player, fullRespawn));
        } else {
            if (RemainingLives <= 0) {
                OnHordeKilledPlayer.Invoke();
                print("GAME OVER DUDE");
                GameOverLostScreen.gameObject.SetActive(true);
            }
        }
    }

    public void CalculateHordeScore(int horde) {
        var inc = horde == 1 ? 25 : 50;
        if (Difficulty.ToLower().Equals(GameConstants.Difficulty_Easy)) {
            inc = inc * 1;
        }else if (Difficulty.ToLower().Equals(GameConstants.Difficulty_Normal)) {
            inc = inc * 2;
        }else {
            inc = inc * 3;
        }
        Score += inc;
    }

    public void GameOver() {
        GameOverUi.Find(GameConstants.ObjectName_ScoreText).GetComponent<Text>().text = "Your Score: " + Score;
        GameOverUi.gameObject.SetActive(true);
    }

    /// <summary>
    /// Respawn player
    /// </summary>
    /// <returns></returns>
    // fullRespawn = true indicates there are no more players on the field
    // fullRespawn = false indicates we're in co-op mode and only one of the players has died
    private IEnumerator RespawnPlayer(int playerToRespawn, bool fullRespawn) {
        yield return new WaitForSeconds(SpawnDelay);
        if (fullRespawn) {
            FullRespawnOccurred = true;
            var spawn = SpawnPoints[SpawnPointIndex];
            var controller = spawn.GetComponent<CheckpointController>();
            if (controller == null) {
                throw new MissingComponentException("No Checkpoint controller");
            }
            if (SpawnPointIndex != 2) {
                controller.DeactivateBaddiesInCheckpoint();
                controller.SpawnFreshBaddiesForCheckpoint(1.5f);
                if (LastSeenInHorde) {
                    OnHordeKilledPlayer.Invoke();
                }
            } else {
                OnHordeKilledPlayer.Invoke();
            }
            // If this is a full respawn we are respawning 1-2 players exactly at the last checkpoint
            foreach (var player in Players) {
                // Have to subtract 1 from the player we want because 0 indexes
                Instantiate(player, SpawnPoints[SpawnPointIndex].position, SpawnPoints[SpawnPointIndex].rotation);
                GetPlayerStatsUi(player.GetComponent<Player>().PlayerNumber).SetLives(_remainingLives);
                // Push a player onto the stack indicating he is alive in the scene
                PlayersCurrentlyAlive.Push(0);
            }
        }else {
            // Short circuit if while we were dead the other player died
            if (FullRespawnOccurred) {
                print("Full Respawn occurred");
                FullRespawnOccurred = false;
                yield break;
            }
            // There is still a teamate on the battle field!
            // Get their location and spawn on their position
            try {
                var playerStillAlive = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);
                // Have to subtract 1 from the player we want because 0 indexes
                Instantiate(Players[playerToRespawn - 1], playerStillAlive.transform.position, playerStillAlive.transform.rotation);
                // Since both players call respawn this should be pushing 1-2 values
                Instance.PlayersCurrentlyAlive.Push(0);
                GetPlayerStatsUi(playerToRespawn).SetLives(_remainingLives);
            } catch (Exception e) {
                print("Hit the one in a million exception. Congratulations");
                // Its possible like the EXACT moment we try and get the player that's alive, he might die. if that happens and this throws an error just chill because
                // as soon as he hits his "KillPlayer" logic the GameMaster will see no players left and full respawn them
                yield break;
            }
        }
    }


}
