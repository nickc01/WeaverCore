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
    /// <summary>
    /// WeaverCore's implementation of a Godhome boss statue
    /// </summary>
    public class WeaverBossStatue : BossStatue, ISerializationCallbackReceiver
    {
        [SerializeField]
        [Tooltip("The settings object used for retaining the statue state")]
        SaveSpecificSettings settings;

        [SerializeField]
        [Tooltip("The name of the field within the SaveSpecificSettings object that contains the completion state of the statue")]
        [SaveSpecificFieldName(typeof(Completion), nameof(settings))]
        string normalStatueStateField;

        [SerializeField]
        [Tooltip("The name of the field within the SaveSpecificSettings object that contains the completion state of the statue (dream version)")]
        [SaveSpecificFieldName(typeof(Completion), nameof(settings))]
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

        static Dictionary<string, bool> playerDataHooks = new Dictionary<string, bool>();


        string HookKey => settings?.name + "_WEAVER_SETTINGS_" + statueStatePD + "_SPACE_" + dreamStatueStatePD;

        static bool Weaver_Awake_Prefix(BossStatue __instance)
        {
            if (__instance is WeaverBossStatue wbs)
            {
                if (!playerDataHooks.ContainsKey(wbs.HookKey))
                {
                    playerDataHooks.Add(wbs.HookKey, true);
                    AddGetterHook(wbs.settings, wbs.normalStatueStateField, wbs.dreamStatueStateField);
                    AddSetterHook(wbs.settings, wbs.normalStatueStateField, wbs.dreamStatueStateField);
                }

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
                if (type == typeof(Completion))
                {
                    if (!string.IsNullOrEmpty(normalField) && name == settings.name + "___WEAVER_BOSS_STATUE___" + normalField)
                    {
                        if (settings.HasField<Completion>(normalField))
                        {
                            settings.SetFieldValue(normalField, (Completion)value);
                        }
                    }
                    else if (!string.IsNullOrEmpty(dreamField) && name == settings.name + "___WEAVER_BOSS_STATUE___" + dreamField)
                    {
                        if (settings.HasField<Completion>(dreamField))
                        {
                            settings.SetFieldValue(dreamField, (Completion)value);
                        }
                    }
                }

                return value;
            };
        }

        [OnHarmonyPatch]
        static void OnPatch(HarmonyPatcher patcher)
        {
            var bossStatueType = typeof(BossStatue);

            var awakeMethod = bossStatueType.GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);

            var awake_prefix = typeof(WeaverBossStatue).GetMethod(nameof(Weaver_Awake_Prefix), BindingFlags.Static | BindingFlags.NonPublic);
            var awake_postfix = typeof(WeaverBossStatue).GetMethod(nameof(Weaver_Awake_Postfix), BindingFlags.Static | BindingFlags.NonPublic);

            patcher.Patch(awakeMethod, awake_prefix, awake_postfix);
        }

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
