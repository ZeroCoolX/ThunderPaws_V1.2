using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class persists throughout the entire game.
/// It handles the various controllers who delegate work per genre.
/// </summary>
public class GameMasterV2 : MonoBehaviour {
    /// <summary>
    /// Static reference for anyone who needs it
    /// </summary>
    public static GameMasterV2 Instance;

    [Header("Mapping")]
    /// <summary>
    /// Compile time collection of any sprites that need to be swapped out during the course of the game.
    /// These include angle specific sprites for the player, health stages...etc
    /// Sprites set on the GameMaster object
    /// </summary>
    public Sprite[] PlayerSpriteList;
    /// <summary>
    /// Compile time list of all weapon prefabs.
    /// </summary>
    public Transform[] WeaponList;
    /// <summary>
    /// Compile time list of player prefabs for Player 1 and (optional) Player 2
    /// Indiex corresponds as such (0 = player 1, 1= player 2)
    /// </summary>
    public Transform[] PlayerPrefabList;
    /// <summary>
    /// Struct which holds all mappings defined above
    /// </summary>
    private MappingProfiles Maps;

    [Header("Life Data")]
    /// <summary>
    /// Remaining lives counter must persist through player deaths
    /// </summary>
    public int RemainingLives;
    /// <summary>
    /// Keeps track of how many players are actively alive in the world.
    /// Useful for determining if we should decrement a life and respawn them.
    /// The integer value is irrelevent.
    /// Needs to be public because it needs to be accessed by the singleton
    /// </summary>
    public Stack<int> PlayersCurrentlyAlive = new Stack<int>();

    [Header("Managers")]
    /// <summary>
    /// Used to shake the screen when needed
    /// </summary>
    private CameraShake _cameraShakeManager;
    /// <summary>
    /// Used to handle where to spawn the player
    /// </summary>
    private SpawnPointManager _spawnManager;
    /// <summary>
    /// Used to access and modify the players HUD UI elements
    /// </summary>
    private PlayerHudManager _playerHudManager;
    /// <summary>
    /// Used to access all the UI elements that might exist in a scene
    /// </summary>
    private UIManager _uiManager;

    /// <summary>
    /// Indicates while one player was dead and waiting to respawn - the other player died
    /// and when that coroutine ends so it should do nothing
    /// </summary>
    private bool _fullRespawnOccurred;
    /// <summary>
    /// How long to wait from player death to respawn
    /// if this is single payer - respawn them almost immeddiately.
    /// Otherwise, if this is Co-op wait a significantly longer time since one player dying doesn't subtract lives
    /// </summary>
    public int SpawnDelay { get { return SinglePlayer || _fullRespawnOccurred ? 3 : 10; } }
    /// <summary>
    /// Indicates if this is single player by checking the Players list count
    /// </summary>
    public bool SinglePlayer { get { return Maps.PlayersPrefabMap != null && Maps.PlayersPrefabMap.Count == 1; } }


    //TODO: Must remove everything below because its terrible
    /// <summary>
    /// Delegate for snotifying the horde the player died, so reset the camera and kill themselves
    /// </summary>
    /// <param name="choice"></param>
    public delegate void PlayerDeadResetHordeCallback();
    public PlayerDeadResetHordeCallback OnHordeKilledPlayer;
    public bool LastSeenInHorde = false;



    private void Awake() {
        // Ensure the object persists through the lifetime of the game
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }

        // Transfer all the compile time lists into maps
        Maps.PlayerSpiteMap = new Dictionary<int, Sprite>();
        Maps.PlayersPrefabMap = new Dictionary<int, Transform>();
        Maps.WeaponMap = new Dictionary<string, Transform>();
        BuildMaps();

        // Set the remaining lives
        RemainingLives = LivesManager.Lives == 0 ? 99 : LivesManager.Lives;
        print("Remaining lives : " + RemainingLives);
    }

    private void Start() {
        // Ensure the CameraShake manager exists
        _cameraShakeManager = transform.GetComponent<CameraShake>();
        if (_cameraShakeManager == null) {throw new MissingReferenceException("No CameraShake found");}

        // Ensure SpawnPointManager exists
        _spawnManager = GameObject.FindGameObjectWithTag("SpawnPointManager").GetComponent<SpawnPointManager>();
        if (_spawnManager == null) {throw new MissingReferenceException("No Spawn manager and thus no spawn points for this scene");}

        _playerHudManager = GameObject.FindGameObjectWithTag("PlayerHudManager").GetComponent<PlayerHudManager>();
        if (_spawnManager == null) { throw new MissingReferenceException("No PlayerHudManager found in the scene"); }

        _uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
        if (_uiManager == null) { throw new MissingReferenceException("No UIManager found in the scene"); }

        // Set the mood
        SelectMusic();
    }

    /// <summary>
    /// Select the music based off what scene is loaded.
    /// This is very basic ATM but it is being built for the future
    /// </summary>
    private void SelectMusic() {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName(GameConstants.Scene_LevelName_Menu) || SceneManager.GetActiveScene() == SceneManager.GetSceneByName("PreAlphaDemoTutorial1")) {
            AudioManager.instance.playSound(GameConstants.Audio_MenuMusic);
        }
    }

    /// <summary>
    /// For each compile time list of sprites, prefabs, whatever needs mapping
    /// populate the maps
    /// </summary>
    private void BuildMaps() {
        // Load sprites for player animation map
        Maps.PlayerSpiteMap.Add(0, PlayerSpriteList[0]);
        Maps.PlayerSpiteMap.Add(45, PlayerSpriteList[1]);
        Maps.PlayerSpiteMap.Add(90, PlayerSpriteList[2]);
        // Load sprites for player health UI state
        Maps.PlayerSpiteMap.Add(100, PlayerSpriteList[3]);
        Maps.PlayerSpiteMap.Add(50, PlayerSpriteList[4]);
        Maps.PlayerSpiteMap.Add(25, PlayerSpriteList[5]);

        // Load weapon map
        foreach (var weaponPrefab in WeaponList) {
            Maps.WeaponMap.Add(weaponPrefab.gameObject.name, weaponPrefab);
        }

        // Load Player prefab map
        MapPlayerWithInput();
    }

    /// <summary>
    /// Generate a mapping between player number and player prefab
    /// </summary>
    private void MapPlayerWithInput() {
        print("Populating Players");
        for(var i = 0; i < JoystickManagerController.Instance.ControllerMap.Count; ++i) {
            var prefix = "";

            JoystickManagerController.Instance.ControllerMap.TryGetValue(i+1, out prefix);
            PlayerPrefabList[i].GetComponent<Player>().JoystickId = prefix;
            print("Set Player "+ PlayerPrefabList[i].GetComponent<Player>().PlayerNumber + " to prefix : " + prefix);

            Maps.PlayersPrefabMap.Add(PlayerPrefabList[i].GetComponent<Player>().PlayerNumber, PlayerPrefabList[i]);
        }
    }

    /// <summary>
    /// Spawn all the players in the array
    /// </summary>
    private void SpawnPlayers() {
        // Spawn the allotted number of players into the room
        foreach (var player in Maps.PlayersPrefabMap) {
            var currentSpawn = _spawnManager.GetCurrentSpawn();
            var playerNum = player.Value.GetComponent<Player>().PlayerNumber;

            Instantiate(player.Value, currentSpawn.position, currentSpawn.rotation);

            print("Setting lives for player " + playerNum);
            _playerHudManager.ActivateStatsHud(playerNum);
            _playerHudManager.GetPlayerHud(playerNum).SetLives(RemainingLives);

            // Push a player onto the stack indicating he is alive in the scene
            PlayersCurrentlyAlive.Push(0);
        }
    }

    /// <summary>
    /// Based off a positive angle value, get the corresponding sprite from the map
    /// </summary>
    /// <param name="degreeKey"></param>
    /// <returns></returns>
    public Sprite GetSpriteFromMap(int degreeKey) {
        Sprite sprite;
        Maps.PlayerSpiteMap.TryGetValue(degreeKey, out sprite);
        if (sprite == null) {
            print("Just informing that GetSpriteFromMap("+degreeKey+") was called and we didn't have that sprite");
            sprite = PlayerSpriteList[0];
        }
        return sprite;
    }

    /// <summary>
    /// Decrement lives, generate particles, shake camera and destroy current player reference
    /// </summary>
    /// <param name="player"></param>
    public static void KillPlayer(Player player) {
        // Pop off a player from the stack
        Instance.PlayersCurrentlyAlive.Pop();

        var fullRespawn = false;
        // We should only decrement lives if there are no more alive players on the field (co-op check)
        if (Instance.PlayersCurrentlyAlive.Count == 0) {
            print("Decrement lives");
            fullRespawn = true;
            //decrement lives
            --Instance.RemainingLives;
        } else {
            print("There is still someone fighting for their life!");
        }

        // Kill the player if necessary
        Instance.KillDashNine(player.gameObject, Instance.RemainingLives > 0, fullRespawn, player.PlayerNumber);
    }

    /// <summary>
    /// Actual destruction with optional respawn
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="respawn"></param>
    private void KillDashNine(GameObject obj, bool respawn, bool fullRespawn, int player) {
        Destroy(obj);
        if (respawn) {
            Instance.StartCoroutine(Instance.RespawnPlayer(player, fullRespawn));
        } else {
            if (RemainingLives <= 0) {
                OnHordeKilledPlayer.Invoke();
                print("GAME OVER DUDE");
                UIManager.Instance.GetUi("GameLost").gameObject.SetActive(true);
            }
        }
    }

    // TODO: This needs to be wildly refactored because it has logic speicifc to literally the demo
    /// <summary>
    /// Respawn player
    /// </summary>
    /// <returns></returns>
    // fullRespawn = true indicates there are no more players on the field
    // fullRespawn = false indicates we're in co-op mode and only one of the players has died
    private IEnumerator RespawnPlayer(int playerToRespawn, bool fullRespawn) {
        yield return new WaitForSeconds(SpawnDelay);
        if (fullRespawn) {
            _fullRespawnOccurred = true;
            var spawn = _spawnManager.GetCurrentSpawn();
            var controller = spawn.GetComponent<CheckpointController>();
            if (controller == null) {
                throw new MissingComponentException("No Checkpoint controller");
            }
            if (_spawnManager.GetSpawnIndex() != 2) {
                controller.DeactivateBaddiesInCheckpoint();
                controller.SpawnFreshBaddiesForCheckpoint(1.5f);
                if (LastSeenInHorde) {
                    OnHordeKilledPlayer.Invoke();
                }
            } else {
                OnHordeKilledPlayer.Invoke();
            }
            // If this is a full respawn we are respawning 1-2 players exactly at the last checkpoint
            foreach (var player in Maps.PlayersPrefabMap) {
                // Have to subtract 1 from the player we want because 0 indexes
                Instantiate(player.Value, spawn.position, spawn.rotation);
                _playerHudManager.GetPlayerHud(player.Value.GetComponent<Player>().PlayerNumber).SetLives(RemainingLives);
                // Push a player onto the stack indicating he is alive in the scene
                PlayersCurrentlyAlive.Push(0);
            }
        } else {
            // Short circuit if while we were dead the other player died
            if (_fullRespawnOccurred) {
                print("Full Respawn occurred");
                _fullRespawnOccurred = false;
                yield break;
            }
            // There is still a teamate on the battle field!
            // Get their location and spawn on their position
            try {
                var playerStillAlive = GameObject.FindGameObjectWithTag(GameConstants.Tag_Player);
                // Have to subtract 1 from the player we want because 0 indexes
                Transform player;
                Maps.PlayersPrefabMap.TryGetValue(playerToRespawn, out player);
                Instantiate(player, playerStillAlive.transform.position, playerStillAlive.transform.rotation);
                // Since both players call respawn this should be pushing 1-2 values
                Instance.PlayersCurrentlyAlive.Push(0);
                _playerHudManager.GetPlayerHud(player.GetComponent<Player>().PlayerNumber).SetLives(RemainingLives);
            } catch (System.Exception e) {
                print("Hit the one in a million exception. Congratulations");
                // Its possible like the EXACT moment we try and get the player that's alive, he might die. if that happens and this throws an error just chill because
                // as soon as he hits his "KillPlayer" logic the GameMaster will see no players left and full respawn them
                yield break;
            }
        }
    }

    // TODO: REFACTOR THIS OUT - IT SHOULD NOT BE IN HERE I THINK 
    public void CalculateHordeScore(int horde) {
        //var inc = horde == 1 ? 25 : 50;
        //if (Difficulty.ToLower().Equals(GameConstants.Difficulty_Easy)) {
        //    inc = inc * 1;
        //} else if (Difficulty.ToLower().Equals(GameConstants.Difficulty_Normal)) {
        //    inc = inc * 2;
        //} else {
        //    inc = inc * 3;
        //}
        //Score += inc;
    }
    // TODO: THIS AS WELL NEEDS REFACTORING
    public void GameOver() {
        //UIManager.Instance.GetUi("GameOver").Find(GameConstants.ObjectName_ScoreText).GetComponent<Text>().text = "Your Score: " + Score;
        UIManager.Instance.GetUi("GameOver").gameObject.SetActive(true);
    }


    private struct MappingProfiles {
        /// <summary>
        /// Mapped by angle of rotation of sprite.
        /// UI Images are mapped by health stages (100, 50, 25) - subject to abstracting out
        /// </summary>
        public Dictionary<int, Sprite> PlayerSpiteMap;
        /// <summary>
        /// Mapped by name to weapon prefab.
        /// Actual storage since unity cannot handle public Dictionaries....
        /// </summary>
        public Dictionary<string, Transform> WeaponMap;
        /// <summary>
        /// Map holding player prefabs mapped by player number to prefab
        /// </summary>
        public Dictionary<int, Transform> PlayersPrefabMap;
    }


    // Hacks for ease of development
    private void Update() {
        // Testing hack for music on and off
        if (Input.GetKeyDown(KeyCode.F)) {
            try {
                //SetDifficulty();
            } catch (System.Exception e) {
                print("Couldn't set difficulty");
            }
        }

        if (Input.GetKeyDown(KeyCode.B)) {
            UIManager.Instance.GetUi("GameOver").gameObject.SetActive(!UIManager.Instance.GetUi("GameOver").gameObject.activeSelf);
        }

        // Testing hack for pause
        if (Input.GetKeyDown(KeyCode.Escape)) {
            UIManager.Instance.GetUi("Controls").gameObject.SetActive(!UIManager.Instance.GetUi("Controls").gameObject.activeSelf);
        }
    }
}
