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
    /// Sprites set on the GameMasterV2 object
    /// </summary>
    public Sprite[] PlayerSpriteList;
    /// <summary>
    /// Compile time collection of any weapon sprites
    /// These are for indicating to the player what weapon they currently have picked u
    /// </summary>
    public Sprite[] WeaponSpriteList;
    /// <summary>
    /// Compile time collection of any ultimate sprites
    /// These are for indicating to the player what ultimate they currently have
    /// </summary>
    public Sprite[] UltimateSpriteList;
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


    // ************************************************************ TODO: this is just for now ************************************************************ //
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
    private int MaxScorePossible;
    /// <summary>
    /// index 0 = player 1 coins per level
    /// index 1 = player 2 coins per level
    /// </summary>
    private int[] _playerCoinCounts = new int[2];
    public int Score { get; set; }

    /// <summary>
    /// Delegate for snotifying the horde the player died, so reset the camera and kill themselves
    /// </summary>
    /// <param name="choice"></param>
    public delegate void PlayerDeadResetHordeCallback();
    public PlayerDeadResetHordeCallback OnHordeKilledPlayer;
    public bool LastSeenInHorde = false;
    // ************************************************************ TODO: this is just for now ************************************************************ //



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




    private void Awake() {
        // Ensure the object persists through the lifetime of the game
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    private void Start() {
        // Transfer all the compile time lists into maps
        Maps.PlayerSpiteMap = new Dictionary<int, Sprite>();
        Maps.PlayersPrefabMap = new Dictionary<int, Transform>();
        Maps.WeaponSpriteMap = new Dictionary<string, Sprite>();
        Maps.UltimateSpriteMap = new Dictionary<string, Sprite>();
        Maps.WeaponPrefabMap = new Dictionary<string, Transform>();
        BuildMaps();

        // Set the remaining lives
        RemainingLives = LivesManager.Lives == 0 ? 99 : LivesManager.Lives;
        print("Remaining lives : " + RemainingLives);

        // Ensure the CameraShake manager exists
        _cameraShakeManager = transform.GetComponent<CameraShake>();
        if (_cameraShakeManager == null) {throw new MissingReferenceException("No CameraShake found");}

        // Set the mood
        SelectMusic();

        SpawnPlayers();
    }

    /// <summary>
    /// Select the music based off what scene is loaded.
    /// This is very basic ATM but it is being built for the future
    /// </summary>
    private void SelectMusic() {
        try {
            AudioManager.Instance.StopSound(GameConstants.Audio_MenuMusic);
        }catch(System.Exception e) {
            print("There was no menu music to stop");
        }
        // Audio files correspond exactly to level names for ease
        if(DifficultyManager.Instance != null) {
            print("playing music " + DifficultyManager.Instance.LevelToPlay);
            AudioManager.Instance.PlaySound(GameConstants.GetLevel(DifficultyManager.Instance.LevelToPlay));
        }else {
            print("DifficultyManager is null?!");
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

        // Load weapon sprite map
        foreach (var ultSprite in UltimateSpriteList) {
            Maps.UltimateSpriteMap.Add(ultSprite.name.ToLower(), ultSprite);
        }

        // Load weapon sprite map
        foreach (var weaponSprite in WeaponSpriteList) {
            Maps.WeaponSpriteMap.Add(weaponSprite.name.Substring(weaponSprite.name.IndexOf("_")+1).ToLower(), weaponSprite);
        }

        // Load weapon prefab map
        foreach (var weaponPrefab in WeaponList) {
            Maps.WeaponPrefabMap.Add(weaponPrefab.gameObject.name.ToLower(), weaponPrefab);
        }

        // Load Player prefab map
        MapPlayerWithInput();
    }

    /// <summary>
    /// Generate a mapping between player number and player prefab
    /// </summary>
    private void MapPlayerWithInput() {
        print("Populating Players");
        var prefix = "";
        if (JoystickManagerController.Instance.ControllerMap.Count < 2) {
            print("there was only 0-1 players in map so set player 1 and leave");
            JoystickManagerController.Instance.ControllerMap.TryGetValue(1, out prefix);
            PlayerPrefabList[0].GetComponent<Player>().JoystickId = prefix;
            print("Set BarneyPrefab.JoystickId to " + prefix);
            Maps.PlayersPrefabMap.Add(PlayerPrefabList[0].GetComponent<Player>().PlayerNumber, PlayerPrefabList[0]);
            return;
        }
        for (var i = 0; i < JoystickManagerController.Instance.ControllerMap.Count; ++i) {
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
            var currentSpawn = SpawnPointManagerV2.Instance.GetCurrentSpawn();
            var playerNum = player.Value.GetComponent<Player>().PlayerNumber;

            var clone = Instantiate(player.Value, currentSpawn.position, currentSpawn.rotation);

            print("Setting lives for player " + playerNum);
            PlayerHudManager.Instance.ActivateStatsHud(playerNum);
            if (!ProfilePool.Instance.Debug) {
                PlayerHudManager.Instance.UpdateUltimateUI2(playerNum, ProfilePool.Instance.GetPlayerProfile(playerNum).GetSelectedUltimate());
            }
            PlayerHudManager.Instance.GetPlayerHud(playerNum).SetLives(RemainingLives);

            // Push a player onto the stack indicating he is alive in the scene
            PlayersCurrentlyAlive.Push(0);

            // Add the player to the game stats map
            GameStatsManager.Instance.AddPlayerToMap(playerNum);
            GameStatsManager.Instance.StartTimer(playerNum);
        }
    }

    /// <summary>
    /// Apply the difficulty by calcualting the max score possible right now (super basic and probably incorrect)
    /// based off the difficulty which is just Health and Lives.
    /// These are set by the DifficultyManager sometime before the level
    /// </summary>
    private void ApplyDifficulty() {
        // Based on the difficulty selected set theit starting score
        MaxScorePossible = LivesManager.Lives == 10 ? 200 : LivesManager.Lives == 5 ? 400 : 600;

        foreach (var player in Maps.PlayersPrefabMap) {
            PlayerHudManager.Instance.GetPlayerHud(player.Value.GetComponent<Player>().PlayerNumber).SetLives(LivesManager.Lives);
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
    /// Based off the key supplied return the corresponding weapon from the map
    /// </summary>
    /// <param name="weaponKey"></param>
    /// <returns></returns>
    public Transform GetWeaponFromMap(string weaponKey) {
        Transform weapon;
        Maps.WeaponPrefabMap.TryGetValue(weaponKey, out weapon);
        if (weapon == null) {
            weapon = WeaponList[0];
        }
        return weapon;
    }

    /// <summary>
    /// Based off a positive angle value, get the corresponding sprite from the map
    /// </summary>
    /// <param name="degreeKey"></param>
    /// <returns></returns>
    public Sprite GetWeaponSpriteFromMap(string weaponKey) {
        Sprite sprite;
        Maps.WeaponSpriteMap.TryGetValue(weaponKey, out sprite);
        if (sprite == null) {
            print("Just informing that GetWeaponSpriteFromMap(" + weaponKey + ") was called and we didn't have that sprite");
            sprite = WeaponSpriteList[0];
        }
        return sprite;
    }

    public Sprite GetUltimateSpriteFromMap(string ultimateKey) {
        Sprite sprite;
        Maps.UltimateSpriteMap.TryGetValue(ultimateKey, out sprite);
        if (sprite == null) {
            print("Just informing that GetUltimateSpriteFromMap(" + ultimateKey + ") was called and we didn't have that sprite");
            sprite = UltimateSpriteList[0];
        }
        return sprite;
    }

    private string currentTune = "";
    private void SoundCheckDebug() {
        if (Input.GetKeyDown(KeyCode.Alpha0)) {
            AudioManager.Instance.StopSound(currentTune);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            currentTune = "S1L1";
            AudioManager.Instance.PlaySound(currentTune);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            currentTune = "S1L2";
            AudioManager.Instance.PlaySound(currentTune);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            currentTune = "S1L3";
            AudioManager.Instance.PlaySound(currentTune);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            currentTune = "S1L4";
            AudioManager.Instance.PlaySound(currentTune);
        }
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
        // increment the stats for whoever died
        GameStatsManager.Instance.AddDeath(player.PlayerNumber);
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
                if(OnHordeKilledPlayer != null) {
                    OnHordeKilledPlayer.Invoke();
                }
                print("GAME OVER DUDE");
                // add optional delay in here so its not so sudden when you lose
                UIManager.Instance.GetUi("GameLost").gameObject.SetActive(true);
            }
        }
    }

    private IEnumerator RespawnPlayer(int playerToRespawn, bool fullRespawn) {
        yield return new WaitForSeconds(SpawnDelay);
        if (fullRespawn) {
            _fullRespawnOccurred = true;
            var spawn = SpawnPointManagerV2.Instance.GetCurrentSpawn();
            var controller = spawn.GetComponent<CheckpointControllerV2>();
            if (controller == null) {
                throw new MissingComponentException("No Checkpoint controller");
            }
            if (SpawnPointManagerV2.Instance.GetSpawnIndex() != 3) {
                controller.ResetBaddieSpawnGroup(1.5f);
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
                PlayerHudManager.Instance.GetPlayerHud(player.Value.GetComponent<Player>().PlayerNumber).SetLives(RemainingLives);
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
                PlayerHudManager.Instance.GetPlayerHud(player.GetComponent<Player>().PlayerNumber).SetLives(RemainingLives);
            } catch (System.Exception e) {
                print("Hit the one in a million exception. Congratulations");
                // Its possible like the EXACT moment we try and get the player that's alive, he might die. if that happens and this throws an error just chill because
                // as soon as he hits his "KillPlayer" logic the GameMasterV2 will see no players left and full respawn them
                yield break;
            }
        }
    }

    // TODO: REFACTOR THIS OUT - IT SHOULD NOT BE IN HERE I THINK 
    public void CalculateHordeScore(int horde) {
        var inc = horde == 1 ? 25 : 50;
        if (DifficultyManager.Instance.Difficulty.ToLower().Equals(GameConstants.Difficulty_Easy)) {
            inc = inc * 1;
        } else if (DifficultyManager.Instance.Difficulty.ToLower().Equals(GameConstants.Difficulty_Normal)) {
            inc = inc * 2;
        } else {
            inc = inc * 3;
        }
        Score += inc;
    }
    // TODO: THIS AS WELL NEEDS REFACTORING
    public void GameOver() {
        AudioManager.Instance.StopSound(GameConstants.GetLevel(DifficultyManager.Instance.LevelToPlay));
        UIManager.Instance.GetUi("GameOver").gameObject.SetActive(true);
        AudioManager.Instance.PlaySound(GameConstants.Audio_LevelClear);
    }


    private struct MappingProfiles {
        /// <summary>
        /// Mapped by angle of rotation of sprite.
        /// UI Images are mapped by health stages (100, 50, 25) - subject to abstracting out
        /// </summary>
        public Dictionary<int, Sprite> PlayerSpiteMap;
        /// <summary>
        /// Mapped by weapon name.
        /// </summary>
        public Dictionary<string, Sprite> WeaponSpriteMap;
        /// <summary>
        /// Mapped by ultimate name
        /// </summary>
        public Dictionary<string, Sprite> UltimateSpriteMap;
        /// <summary>
        /// Mapped by name to weapon prefab.
        /// Actual storage since unity cannot handle public Dictionaries....
        /// </summary>
        public Dictionary<string, Transform> WeaponPrefabMap;
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

                ApplyDifficulty();
            } catch (System.Exception e) {
                print("Couldn't set difficulty : " + e.Message);
            }
        }
        SoundCheckDebug();
    }
}
