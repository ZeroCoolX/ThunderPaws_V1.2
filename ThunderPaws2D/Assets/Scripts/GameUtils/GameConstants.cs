using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConstants {

    // Audio file names
	public static string Audio_JumpLanding = "JumpLanding";
    public static string Audio_MenuMusic = "Music_Menu";
    public static string Audio_MainMusic = "Music_Main";
    public static string Audio_BossMusic = "Music_Boss";
    public static string Audio_BackstoryMusic = "Music_Backstory";
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
    public static string ObjectName_DefaultWeapon = "FuzzBuster";
    public static string ObjectName_WeaponAnchor = "WeaponAnchor";
    public static string ObjectName_UltimateIndicator = "UltIndicator";
    public static string ObjectName_PlayerImage = "PlayerImage";
    public static string ObjectName_WeaponImage = "WeaponImage";
    public static string ObjectName_AmmoText = "AmmoText";
    public static string ObjectName_LivesText = "LivesText";
    public static string ObjectName_BarContainer = "BarContainer";
    public static string ObjectName_UltimateBar = "UltimateBar";
    public static string ObjectName_FirePoint = "FirePoint";
    public static string ObjectName_MainCamera = "MainCamera";
    public static string ObjectName_ScoreText = "ScoreText";

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
    public static string Scene_LevelName_1 = "AlphaDemoLevel1";
    public static string Scene_LevelName_Menu = "AlphaDemoMainMenu";
    public static string Scene_Backstory_Menu = "AlphaDemoBackstory";

    // Difficulties
    public static string Difficulty_Easy = "easy";
    public static string Difficulty_Normal = "normal";
    public static string Difficulty_Hard = "hard";

}
