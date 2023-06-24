using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Internal;
using WeaverCore.Settings;

namespace WeaverCore.Components
{

    public class WeaverBossStatue : BossStatue, ISerializationCallbackReceiver
    {
        [SerializeField]
        [Tooltip("The settings object used for retaining the statue state")]
        SaveSpecificSettings settings;

        [SerializeField]
        [Tooltip("The name of the field within the SaveSpecificSettings object that contains the completion state of the statue")]
        string normalStatueStateField;

        [SerializeField]
        [Tooltip("The name of the field within the SaveSpecificSettings object that contains the completion state of the statue (dream version)")]
        string dreamStatueStateField;

        [SerializeField]
        string bossNameKey;

        [SerializeField]
        string bossNameSheet;

        [SerializeField]
        string bossDescKey;

        [SerializeField]
        string bossDescSheet;

        [SerializeField]
        string dreamBossNameKey;

        [SerializeField]
        string dreamBossNameSheet;

        [SerializeField]
        string dreamBossDescKey;

        [SerializeField]
        string dreamBossDescSheet;

        [Space]
        [Header("Overrides")]
        [SerializeField]
        [Tooltip("If specified, will override the name of the boss that is normally retrieved via boss name key and sheet")]
        string nameOverride;

        [SerializeField]
        [Tooltip("If specified, will override the description of the boss that is normally retrieved via boss name key and sheet")]
        string descOverride;

        [SerializeField]
        [Tooltip("If specified, will override the name of the dream boss that is normally retrieved via boss name key and sheet")]
        string dreamNameOverride;

        [SerializeField]
        [Tooltip("If specified, will override the description of the dream boss that is normally retrieved via boss name key and sheet")]
        string dreamDescOverride;


        //[NonSerialized]
        //bool hooked = false;

        static Dictionary<string, bool> playerDataHooks = new Dictionary<string, bool>();


        string HookKey => settings?.name + "_WEAVER_SETTINGS_" + statueStatePD + "_SPACE_" + dreamStatueStatePD;

        static bool Weaver_Awake_Prefix(BossStatue __instance)
        {
            //WeaverLog.Log("BOSS STATUE PREFIX");
            if (__instance is WeaverBossStatue wbs)
            {
                if (!playerDataHooks.ContainsKey(wbs.HookKey))
                {
                    playerDataHooks.Add(wbs.HookKey, true);
                    AddGetterHook(wbs.settings, wbs.normalStatueStateField, wbs.dreamStatueStateField);
                    AddSetterHook(wbs.settings, wbs.normalStatueStateField, wbs.dreamStatueStateField);
                    //ModHooks.GetPlayerVariableHook += wbs.ModHooks_GetPlayerVariableHook;
                    //ModHooks.SetPlayerVariableHook += wbs.ModHooks_SetPlayerVariableHook;
                }
                //WeaverLog.Log("PREFIX WBS");
                /*if (!wbs.hooked)
                {
                    wbs.hooked = true;
                    
                }*/

                wbs.bossDetails = new BossUIDetails
                {
                    nameKey = wbs.bossNameKey,
                    nameSheet = wbs.bossNameSheet,
                    descriptionKey = wbs.bossDescKey,
                    descriptionSheet = wbs.bossDescSheet
                };

                wbs.dreamBossDetails = new BossUIDetails
                {
                    nameKey = wbs.dreamBossNameKey,
                    nameSheet = wbs.dreamBossNameSheet,
                    descriptionKey = wbs.dreamBossDescKey,
                    descriptionSheet = wbs.dreamBossDescSheet
                };

                if (Initialization.Environment == Enums.RunningState.Game)
                {
                    wbs.statueDownSound = GG_Internal.statueDownSound;
                    wbs.statueUpSound = GG_Internal.statueUpSound;

                    var lever = wbs.GetComponentInChildren<BossStatueLever>(true);

                    if (lever != null)
                    {
                        lever.audioPlayerPrefab = GG_Internal.AudioPlayerPrefab;
                        lever.switchSound = GG_Internal.BossLeverSwitchSound;
                        lever.strikeNailPrefab = GG_Internal.StrikeNailR;
                    }

                    var dreamLever = wbs.GetComponentInChildren<BossStatueDreamToggle>(true);

                    if (dreamLever != null)
                    {
                        dreamLever.dreamImpactPrefab = GG_Internal.dreamImpactPrefab;
                        dreamLever.dreamBurstEffectPrefab = GG_Internal.dreamBurstEffectPrefab;
                        dreamLever.dreamBurstEffectOffPrefab = GG_Internal.dreamBurstEffectOffPrefab;
                    }
                }

                //WeaverLog.Log("BOSS DETAILS = " + JsonUtility.ToJson(wbs.bossDetails, true));
                //WeaverLog.Log("DREAM BOSS DETAILS = " + JsonUtility.ToJson(wbs.dreamBossDetails, true));

                if (GG_Internal.AudioPlayerPrefab != null)
                {
                    foreach (var glow in wbs.GetComponentsInChildren<GlowResponse>(true))
                    {
                        glow.audioPlayerPrefab = GG_Internal.AudioPlayerPrefab;
                    }
                }
            }

            return true;
        }

        static void Weaver_Awake_Postfix(BossStatue __instance)
        {
            if (__instance is WeaverBossStatue wbs)
            {
                if (wbs.settings == null)
                {
                    WeaverLog.LogError($"The statue {wbs.gameObject.name} doesn't have it's settings field properly configured.");
                }
                if (Initialization.Environment == Enums.RunningState.Game)
                {
                    if (GG_Internal.attuned_award_prefab == null)
                    {
                        throw new System.Exception("Could not find attuned reward prefab");
                    }

                    if (GG_Internal.AudioPlayerPrefab == null)
                    {
                        throw new System.Exception("Could not find audio player prefab");
                    }
                }
                if (GG_Internal.attuned_award_prefab != null)
                {
                    foreach (var plaque in wbs.GetComponentsInChildren<BossStatueTrophyPlaque>())
                    {
                        plaque.tierCompleteEffectPrefabs = new GameObject[]
                        {
                            GG_Internal.attuned_award_prefab,
                            GG_Internal.ascended_award_prefab,
                            GG_Internal.radiant_award_prefab
                        };
                    }
                }

                if (GG_Internal.AudioPlayerPrefab != null)
                {
                    wbs.audioSourcePrefab = GG_Internal.AudioPlayerPrefab;

                    if (GG_Internal.StrikeNailR != null)
                    {
                        foreach (var lever in wbs.GetComponentsInChildren<BossStatueLever>(true))
                        {
                            lever.audioPlayerPrefab = GG_Internal.AudioPlayerPrefab;
                            lever.strikeNailPrefab = GG_Internal.StrikeNailR;
                        }
                    }
                }

                if (GG_Internal.statueAnimators != null)
                {
                    foreach (var animator in wbs.GetComponentsInChildren<Animator>(true))
                    {
                        animator.runtimeAnimatorController = GG_Internal.statueAnimators.FirstOrDefault(r => r.name == animator.runtimeAnimatorController.name);
                    }
                }
            }
        }

        static void AddGetterHook(SaveSpecificSettings settings, string normalField, string dreamField)
        {
            if (settings == null)
            {
                return;
            }

            ModHooks.GetPlayerVariableHook += (type, name, value) =>
            {
                if (type == typeof(Completion))
                {
                    if (!string.IsNullOrEmpty(normalField) && name == settings.name + "___WEAVER_BOSS_STATUE___" + normalField)
                    {
                        if (settings.HasField<Completion>(normalField))
                        {
                            //WeaverLog.Log("GET SETTINGS = " + JsonUtility.ToJson(settings.GetFieldValue<Completion>(normalField), true));
                            return settings.GetFieldValue<Completion>(normalField);
                        }
                    }
                    else if (!string.IsNullOrEmpty(dreamField) && name == settings.name + "___WEAVER_BOSS_STATUE___" + dreamField)
                    {
                        if (settings.HasField<Completion>(dreamField))
                        {
                            return settings.GetFieldValue<Completion>(dreamField);
                        }
                    }
                }

                return value;
            };
        }

        static void AddSetterHook(SaveSpecificSettings settings, string normalField, string dreamField)
        {
            if (settings == null)
            {
                return;
            }

            ModHooks.SetPlayerVariableHook += (type, name, value) =>
            {
                /*WeaverLog.Log("SET PLAYER VARIABLE WORKING");
                bool print = name.Contains("___WEAVER_BOSS_STATUE___");
                if (print)
                {
                    WeaverLog.Log("Name = " + name);
                    WeaverLog.Log("Type = " + type.FullName);
                    WeaverLog.Log("Value = " + JsonUtility.ToJson((Completion)value, true));
                }*/
                if (type == typeof(Completion))
                {
                    if (!string.IsNullOrEmpty(normalField) && name == settings.name + "___WEAVER_BOSS_STATUE___" + normalField)
                    {
                        if (settings.HasField<Completion>(normalField))
                        {
                            //return settings.GetFieldValue<Completion>(normalStatueStateField);
                            settings.SetFieldValue(normalField, (Completion)value);
                            //WeaverLog.Log("NEW NORMAL VALUE = " + JsonUtility.ToJson((Completion)value, true));
                        }
                    }
                    else if (!string.IsNullOrEmpty(dreamField) && name == settings.name + "___WEAVER_BOSS_STATUE___" + dreamField)
                    {
                        if (settings.HasField<Completion>(dreamField))
                        {
                            //return settings.GetFieldValue<Completion>(dreamStatueStateField);
                            settings.SetFieldValue(dreamField, (Completion)value);
                            //WeaverLog.Log("NEW DREAM VALUE = " + JsonUtility.ToJson((Completion)value, true));
                        }
                    }
                }

                return value;
            };
        }

        /*private static object ModHooks_GetPlayerVariableHook1()
        {
            throw new NotImplementedException();
        }

        private object ModHooks_SetPlayerVariableHook(Type type, string name, object value)
        {
            if (type == typeof(Completion))
            {
                if (name == settings.name + "___WEAVER_BOSS_STATUE___" + normalStatueStateField)
                {
                    if (settings.HasField<Completion>(normalStatueStateField))
                    {
                        //return settings.GetFieldValue<Completion>(normalStatueStateField);
                        settings.SetFieldValue(normalStatueStateField, (Completion)value);
                        //WeaverLog.Log("NEW NORMAL VALUE = " + JsonUtility.ToJson((Completion)value,true));
                    }
                }
                else if (name == settings.name + "___WEAVER_BOSS_STATUE___" + dreamStatueStateField)
                {
                    if (settings.HasField<Completion>(dreamStatueStateField))
                    {
                        //return settings.GetFieldValue<Completion>(dreamStatueStateField);
                        settings.SetFieldValue(dreamStatueStateField, (Completion)value);
                        //WeaverLog.Log("NEW DREAM VALUE = " + JsonUtility.ToJson((Completion)value, true));
                    }
                }
            }

            return value;
        }

        private object ModHooks_GetPlayerVariableHook(Type type, string name, object value)
        {
            if (type == typeof(Completion))
            {
                if (name == settings.name + "___WEAVER_BOSS_STATUE___" + normalStatueStateField)
                {
                    if (settings.HasField<Completion>(normalStatueStateField))
                    {
                        //WeaverLog.Log("GET SETTINGS = " + JsonUtility.ToJson(settings.GetFieldValue<Completion>(normalStatueStateField), true));
                        return settings.GetFieldValue<Completion>(normalStatueStateField);
                    }
                }
                else if (name == settings.name + "___WEAVER_BOSS_STATUE___" + dreamStatueStateField)
                {
                    if (settings.HasField<Completion>(dreamStatueStateField))
                    {
                        return settings.GetFieldValue<Completion>(dreamStatueStateField);
                    }
                }
            }

            return value;
        }*/

        [OnHarmonyPatch]
        static void OnPatch(HarmonyPatcher patcher)
        {
            //WeaverLog.Log("RUNNING WEAVERBOSSSTATUE PATCH");

            var bossStatueType = typeof(BossStatue);

            var awakeMethod = bossStatueType.GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);

            var awake_prefix = typeof(WeaverBossStatue).GetMethod(nameof(Weaver_Awake_Prefix), BindingFlags.Static | BindingFlags.NonPublic);
            var awake_postfix = typeof(WeaverBossStatue).GetMethod(nameof(Weaver_Awake_Postfix), BindingFlags.Static | BindingFlags.NonPublic);

            patcher.Patch(awakeMethod, awake_prefix, awake_postfix);
        }

        /*private void OnEnable()
        {
            if (!hooked)
            {
                hooked = true;
                ModHooks.GetPlayerVariableHook += ModHooks_GetPlayerVariableHook;
                ModHooks.SetPlayerVariableHook += ModHooks_SetPlayerVariableHook;
            }
        }

        private void OnDisable()
        {
            if (hooked)
            {
                hooked = false;
                ModHooks.GetPlayerVariableHook -= ModHooks_GetPlayerVariableHook;
                ModHooks.SetPlayerVariableHook -= ModHooks_SetPlayerVariableHook;
            }
        }

        private void OnDestroy()
        {
            if (hooked)
            {
                hooked = false;
                ModHooks.GetPlayerVariableHook -= ModHooks_GetPlayerVariableHook;
                ModHooks.SetPlayerVariableHook -= ModHooks_SetPlayerVariableHook;
            }
        }*/

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (settings != null && !string.IsNullOrEmpty(normalStatueStateField))
            {
                statueStatePD = settings.name + "___WEAVER_BOSS_STATUE___" + normalStatueStateField;
            }
            else
            {
                statueStatePD = "";
            }

            if (settings != null && !string.IsNullOrEmpty(dreamStatueStateField))
            {
                dreamStatueStatePD = settings.name + "___WEAVER_BOSS_STATUE___" + dreamStatueStateField;
            }
            else
            {
                dreamStatueStatePD = "";
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            
        }
    }
}
