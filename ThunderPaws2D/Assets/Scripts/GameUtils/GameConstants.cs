using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConstants {

    // Audio file names
	public static string Audio_JumpLanding = "JumpLanding";
    public static string Audio_MenuMusic = "Music_Menu";
    public static string Audio_MainMusic = "S1L2";//"Music_Main";
    public static string Audio_BossMusic = "Music_Boss";
    public static string Audio_BackstoryMusic = "Music_Backstory";
    public static string Audio_LevelClear = "LevelClear";
    public static string Audio_WeaponSwitch = "WeaponSwitch";
    public static string Audio_FuzzBuster = "FuzzBusterShot";
    public static string Audio_Shotgun = "ShotgunShot";
    public static string Audio_WeaponPickup = "WeaponPickup";
    public static string Audio_Melee = "FirePunch";
    public static string Audio_MenuButtonClick = "MenuButton_Click";
    public static string Audi_MenuButtonNavigate = "MenuButton_Navigate";
    public static string Audio_GaussShot = "GaussShot";
    public static string Audio_GaussShotCharged = "GaussShot_Charged";
    public static string Audio_FatCatShot = "FatCatShot";
    public static string Audio_EmissionIndexShot = "EmissionIndexShot";
    public static string Audio_EmissionIndexImpact = "EmissionIndex_Impact";
    public static string Audio_GaussShotChargedImpact = "GaussShot_ChargedImpact";
    public static string Audio_GaussShotImpact = "GaussShot_Impact";
    public static string Audio_GaussShotUltImpact = "GaussShot_UltImpact";
    public static string Audio_FatCatImpact = "FatCat_Impact";
    public static string Audio_AccessDenied = "AccessDenied";

    // Level + (H)orde  audio
    public static string S1L1 = "S1L1";
    public static string S1L1H = "S1L1H";
    public static string S1L2 = "S1L2";
    public static string S1L2H = "S1L2H";
    public static string S1L3 = "S1L3";
    public static string S1L3H = "S1L3H";
    public static string S1L4 = "S1L4";

    // GameTags
    public static string Tag_Player = "Player";
    public static string Tag_GameMasterV2 = "GameMasterV2";
    public static string Tag_ObstacleThrough = "OBSTACLE-THROUGH";
    public static string Tag_Baddie = "Baddie";
    public static string Tag_HordeBaddie = "HordeBaddie";
    public static string Tag_Tutorial = "Tutorial";
    public static string Tag_StartSpawn = "StartSpawn";

    // Game object names
    public static string ObjectName_Pickup = "Pickup";
    public static string ObjectName_WeaponAnchor = "WeaponAnchor";
    public static string ObjectName_UltimateIndicator = "UltIndicator";
    public static string ObjectName_PlayerImage = "PlayerImage";
    public static string ObjectName_WeaponImage = "WeaponImage";
    public static string ObjectName_UltimateImage = "UltimateImage";
    public static string ObjectName_AmmoText = "AmmoText";
    public static string ObjectName_LivesText = "LivesText";
    public static string ObjectName_EmissionCachesFoundText = "CachesFoundText";
    public static string ObjectName_BarContainer = "BarContainer";
    public static string ObjectName_UltimateBar = "UltimateBar";
    public static string ObjectName_FirePoint = "FirePoint";
    public static string ObjectName_MainCamera = "MainCamera";
    public static string ObjectName_ScoreText = "ScoreText";

    //Weapons
    public static string ObjectName_DefaultWeapon = "fuzzbuster";
    public static string ObjectName_GaussWeapon = "gaussrifle";
    public static string ObjectName_ShotgunWeapon = "shotgun";
    public static string ObjectName_EmissionIndexWeapon = "emissionindex";
    public static string ObjectName_FatCatWeapon = "fatcat";
    //Ultimates
    public const string ObjectName_TriggerPaw = "triggerpaw";
    public const string ObjectName_LighteningClaw = "lighteningclaw";
    public const string ObjectName_ThunderPounce = "thunderpounce";
    public const string ObjectName_ReflectFurball = "reflectfurball";



    // Input names
    public static string Input_LBumper = "LBumper";
    public static string Input_LTrigger = "LTrigger";
    public static string Input_RTrigger = "RTrigger";
    public static string Input_Horizontal = "Horizontal";
    public static string Input_Vertical = "Vertical";
    public static string Input_Fire = "Fire1";
    public static string Input_Jump = "Jump";
    public static string Input_Melee = "Melee";
    public static string Input_Roll = "Roll";
    public static string Input_Ultimate = "Ultimate";
    public static string Input_Back = "Back";
    public static string Input_Start = "Start";

    // Layer names
    public static string Layer_PlayerProjectile = "PLAYER_PROJECTILE";
    public static string Layer_ObstacleThrough = "OBSTACLE-THROUGH";

    // Layer values
    public static int Layer_Player = 8;

    // Player Data
    public static float Data_PlayerCrouchY = -0.49f;
    public static float Data_PlayerCrouchSize = 0.78303f;
    public static float Data_PlayerY = -0.1063671f;
    public static float Data_PlayerSize = 1.536606f;

    // FL1 Baddie Data
    public static float Data_VerticalPrecision = 0.1f;

    // FL2 Baddie Data
    /// <summary>
    /// Indicates how far the player can move away from the enemy until it stops firing
    /// </summary>
    public static float Data_SightDistance = 15f;

    // Scene Name Data
    public static string Scene_LevelName_1 = "AlphaArtDemoLevel1";
    public static string Scene_LevelName_Menu = "MainMenu";
    public static string Scene_Backstory_Menu = "IntroBackstory";

    // Difficulties
    public static string Difficulty_Easy = "easy";
    public static string Difficulty_Normal = "normal";
    public static string Difficulty_Hard = "hard";

    public struct Level {
        public string Name { get; set; }
        public string PublicName { get; set; }
    }

    // Levels
    private static Dictionary<int, Level> _stageLevels = new Dictionary<int, Level>() {
        {11, new Level { Name = "S1L1", PublicName =  "City Outskirts"} },
        {12, new Level { Name = "S1L2", PublicName =  "City Streets"} },
        {13, new Level { Name = "S1L3", PublicName =  "Rooftops"} },
        {14, new Level { Name = "S1L4", PublicName =  "Capital"} },
        {21, new Level { Name = "S2L1", PublicName =  "..."} },
        {22, new Level { Name = "S2L2", PublicName =  "..."} },
        {23, new Level { Name = "S2L3", PublicName =  "..."} },
        {24, new Level { Name = "S2L4", PublicName =  "..."} },
        {31, new Level { Name = "S3L1", PublicName =  "..."} },
        {32, new Level { Name = "S3L2", PublicName =  "..."} },
        {33, new Level { Name = "S3L3", PublicName =  "..."} },
        {34, new Level { Name = "S3L4", PublicName =  "..."} },
        {41, new Level { Name = "S4L1", PublicName =  "..."} },
        {42, new Level { Name = "S4L2", PublicName =  "..."} }
    };

    private static Dictionary<int, string> _stages = new Dictionary<int, string> {
        { 1 ,"Nipton City"},
        { 2, "Neko Forest"},
        { 3, "Mausu Ops Base"},
        { 4, "Mausu Space Station"},
        { 0, "CLASSIFIED"}
    };

    public static string GetLevelPublicName(string key) {
        int realKey = Convert.ToInt32(key);
        if(realKey % 10 == 0) {
            // This indicates the level was not unlocked so return default
            return "UN-LIBERATED";
        }
        Level level;
        _stageLevels.TryGetValue(realKey, out level);
        return level.PublicName;
    }


    public static string GetStage(int key) {
        string stage = "";
        _stages.TryGetValue(key, out stage);
        if (string.IsNullOrEmpty(stage)) {
            Debug.LogError("Error retrieving stage : " + key + " returning stage 1 regardless");
            stage = "Nipton City";
        }
        return stage;
    }

    public static string GetLevel(int key) {
        Level level;
        _stageLevels.TryGetValue(key, out level);
        return level.Name;
    }

}
