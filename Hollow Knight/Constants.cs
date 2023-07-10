using System.Reflection;
using GlobalEnums;
using UnityEngine;

public static class Constants
{
    public const string GAME_VERSION = "1.5.75.11827";

    public const float DEFAULT_TIMESCALE = 1f;

    public const float HALF_TIMESCALE = 0.5f;

    public const float PAUSED_TIMESCALE = 0f;

    public const float FRAME_WAIT = 0.165f;

    public const float TIME_SCALE_CHANGE_RATE = 1E-05f;

    public const float SCENE_TRANSITION_WAIT = 0.5f;

    public const float HERO_DEFAULT_GRAVITY = 0.79f;

    public const float HERO_UNDERWATER_GRAVITY = 0.225f;

    public const float RAYCAST_EXTENTS = 0.16f;

    public const float MIN_WALL_HEIGHT = 0.2f;

    public const float INPUT_LOWER_SNAP_V = 0.5f;

    public const float INPUT_LOWER_SNAP_H = 0.3f;

    public const float INPUT_UPPER_SNAP = 0.9f;

    public const float INPUT_DEADZONE_L = 0.15f;

    public const float INPUT_DEADZONE_U = 0.95f;

    public const float CAM_Z_DEFAULT = -38.1f;

    public const float CAM_BOUND_X = 14.6f;

    public const float CAM_BOUND_Y = 8.3f;

    public const float CAM_HOR_OFFSET_AMOUNT = 1f;

    public const float CAM_FALL_VELOCITY = -20f;

    public const float CAM_FALL_OFFSET = -4f;

    public const float CAM_LOOK_OFFSET = 6f;

    public const float CAM_START_LOCKED_TIMER = 0.5f;

    public const float CAM_HAZARD_RESPAWN_FROZEN = 0.5f;

    public const float CAM_MENU_X = 14.6f;

    public const float CAM_MENU_Y = 8.5f;

    public const float CAM_CIN_X = 14.6f;

    public const float CAM_CIN_Y = 8.5f;

    public const float CAM_CUT_X = 14.6f;

    public const float CAM_CUT_Y = 8.5f;

    public const float CAM_STAG_PRE_FADEOUT = 0.6f;

    public const float CAM_FADE_TIME_JUST_FADE = 0.5f;

    public const float CAM_FADE_TIME_START_FADE = 2.3f;

    public const float CAM_DEFAULT_BLUR_DEPTH = 6.62f;

    public const float CAM_DEFAULT_SATURATION = 0.7f;

    public const float CAM_DEFAULT_INTENSITY = 0.7f;

    public const float MIN_VIEW_DEPTH = 10f;

    public const float MAX_VIEW_DEPTH = 1000f;

    public const float CAM_OVERLAP = 1E-05f;

    public const float CAM_ORTHOSIZE = 8.710664f;

    public const float CAM_GAME_ASPECT = 1.77777779f;

    public const float CAM_CANVAS_MOVE_WAIT = 0.5f;

    public const string CAM_SHAKE_ENEMYKILL = "EnemyKillShake";

    public const float SCENE_POSITION_LIMIT = 60f;

    public const float SCENE_BORDER_THICKNESS = 20f;

    public const string MENU_SCENE = "Menu_Title";

    public const string FIRST_LEVEL_NAME = "Tutorial_01";

    public const string STARTING_SCENE = "Opening_Sequence";

    public const string INTRO_PROLOGUE = "Intro_Cutscene_Prologue";

    public const string OPENING_CUTSCENE = "Intro_Cutscene";

    public const string STAG_CINEMATIC = "Cinematic_Stag_travel";

    public const string PERMADEATH_LEVEL = "PermaDeath";

    public const string PERMADEATH_UNLOCK = "PermaDeath_Unlock";

    public const string MRMUSHROOM_CINEMATIC = "Cinematic_MrMushroom";

    public const string ENDING_A_CINEMATIC = "Cinematic_Ending_A";

    public const string ENDING_B_CINEMATIC = "Cinematic_Ending_B";

    public const string ENDING_C_CINEMATIC = "Cinematic_Ending_C";

    public const string ENDING_D_CINEMATIC = "Cinematic_Ending_D";

    public const string ENDING_E_CINEMATIC = "Cinematic_Ending_E";

    public const string END_CREDITS = "End_Credits";

    public const string MENU_CREDITS = "Menu_Credits";

    public const string TITLE_SCREEN_LEVEL = "Title_Screens";

    public const string TUTORIAL_LEVEL = "Tutorial_01";

    public const string BOSS_DOOR_CUTSCENE = "Cutscene_Boss_Door";

    public const string GAME_COMPLETION_SCREEN = "End_Game_Completion";

    public const string BOSSRUSH_END_SCENE = "GG_End_Sequence";

    public const string GG_ENTRANCE_SCENE = "GG_Entrance_Cutscene";

    public const string GG_DOOR_ENTRANCE_SCENE = "GG_Boss_Door_Entrance";

    public const string GG_RETURN_SCENE = "GG_Waterways";

    public const string SAVE_ICON_START_EVENT = "GAME SAVING";

    public const string SAVE_ICON_END_EVENT = "GAME SAVED";

    public const float HERO_Z = 0.004f;

    public const float HAZARD_DEATH_WAIT = 0f;

    public const float RESPAWN_FADEOUT_WAIT = 0.8f;

    public const float HAZ_RESPAWN_FADEIN_WAIT = 0.1f;

    public const float SCENE_ENTER_WAIT = 0.33f;

    public const float HERO_MIN_FALL_VEL = -1E-06f;

    public const float ASPECT_16_9 = 1.777778f;

    public const float ASPECT_16_10 = 1.777778f;

    public const float CUTSCENE_PROMPT_TIMEOUT = 5f;

    public const float CUTSCENE_PROMPT_SKIP_COOLDOWN = 0.3f;

    public const float SAVE_FLEUR_PAUSE = 0.2f;

    public const string RECORD_PERMADEATH_MODE = "RecPermadeathMode";

    public const string RECORD_BOSSRUSH_MODE = "RecBossRushMode";

    public const SupportedLanguages DEFAULT_LANGUAGE = SupportedLanguages.EN;

    public const int DEFAULT_BACKERCREDITS = 0;

    public const int DEFAULT_NATIVEPOPUPS = 0;

    public const bool DEFAULT_NATIVEINPUT = true;

    public const float MM_AUDIO_MASTER_VOL = 10f;

    public const float MM_AUDIO_MUSIC_VOL = 10f;

    public const float MM_AUDIO_SOUND_VOL = 10f;

    public const int MM_VIDEO_RESX = 1920;

    public const int MM_VIDEO_RESY = 1080;

    public const int MM_VIDEO_REFRESH = 60;

    public const int MM_VIDEO_FULLSCREEN = 2;

    public const int MM_VIDEO_VSYNC = 0;

    public const int DEFAULT_VIDEO_FRAMECAP = 400;

    public const bool DEFAULT_VIDEO_FRAMECAP_ON = true;

    public const int DEFAULT_DISPLAY = 0;

    public const int DEFAULT_VIDEO_PARTICLES = 1;

    public const float MM_OS_MAINCAM = 1f;

    public const float MM_OS_HUDCAM = 8.710664f;

    public const float MM_OS_DEFAULT = 0f;

    public const float DEFAULT_BRIGHTNESS = 20f;

    public const string GSKEY_GAME_LANGUAGE = "GameLang";

    public const string GSKEY_GAME_BACKERS = "GameBackers";

    public const string GSKEY_GAME_POPUPS = "GameNativePopups";

    public const string GSKEY_RUMBLE_MUTED = "RumbleMuted";

    public const string GSKEY_VIDEO_RESX = "VidResX";

    public const string GSKEY_VIDEO_RESY = "VidResY";

    public const string GSKEY_VIDEO_REFRESH = "VidResRefresh";

    public const string GSKEY_VIDEO_FULLSCREEN = "VidFullscreen";

    public const string GSKEY_VIDEO_VSYNC = "VidVSync";

    public const string GSKEY_VIDEO_DISPLAY = "VidDisplay";

    public const string GSKEY_VIDEO_TFR = "VidTFR";

    public const string GSKEY_VIDEO_FRAMECAP = "VidFC";

    public const string GSKEY_VIDEO_PARTICLES = "VidParticles";

    public const string GSKEY_VIDEO_SHADER_QUALITY = "ShaderQuality";

    public const string GSKEY_OS_VALUE = "VidOSValue";

    public const string GSKEY_OS_SET = "VidOSSet";

    public const string GSKEY_BRIGHT_VALUE = "VidBrightValue";

    public const string GSKEY_BRIGHT_SET = "VidBrightSet";

    public const string GSKEY_AUDIO_MASTER = "MasterVolume";

    public const string GSKEY_AUDIO_MUSIC = "MusicVolume";

    public const string GSKEY_AUDIO_SOUND = "SoundVolume";

    public const string GSKEY_KEY_JUMP = "KeyJump";

    public const string GSKEY_KEY_ATTACK = "KeyAttack";

    public const string GSKEY_KEY_DASH = "KeyDash";

    public const string GSKEY_KEY_CAST = "KeyCast";

    public const string GSKEY_KEY_SUPERDASH = "KeySupDash";

    public const string GSKEY_KEY_DREAMNAIL = "KeyDreamnail";

    public const string GSKEY_KEY_QUICKMAP = "KeyQuickMap";

    public const string GSKEY_KEY_QUICKCAST = "KeyQuickCast";

    public const string GSKEY_KEY_INVENTORY = "KeyInventory";

    public const string GSKEY_KEY_UP = "KeyUp";

    public const string GSKEY_KEY_DOWN = "KeyDown";

    public const string GSKEY_KEY_LEFT = "KeyLeft";

    public const string GSKEY_KEY_RIGHT = "KeyRight";

    public const string GSKEY_CONTROLLER_PREFIX = "Controller";

    public const string GSKEY_LANG_SET = "GameLangSet";

    public const string GSKEY_NATIVE_INPUT = "NativeInput";

    public const string COMM_ARG_RESETALL = "-resetall";

    public const string COMM_ARG_RESETRES = "-resetres";

    public const string COMM_ARG_ALLOWLANG = "-forcelang";

    public const string COMM_ARG_DEBUGKEYS = "-allowdebug";

    public const string COMM_ARG_NATIVEINPUT = "-nativeinput";

    public static T GetConstantValue<T>(string variableName)
    {
        FieldInfo[] fields = typeof(Constants).GetFields(BindingFlags.Static | BindingFlags.Public);
        foreach (FieldInfo fieldInfo in fields)
        {
            if (fieldInfo.Name == variableName)
            {
                if (fieldInfo.FieldType == typeof(T))
                {
                    return (T)fieldInfo.GetRawConstantValue();
                }
                Debug.LogError($"Constants value was of type \"{fieldInfo.FieldType.ToString()}\", expected type \"{typeof(T).ToString()}\".");
            }
        }
        Debug.LogError("Couldn't find constant with name: " + variableName);
        return default(T);
    }
}
