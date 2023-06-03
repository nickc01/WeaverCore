using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Internal;

namespace WeaverCore.Components
{
    public class WeaverBossStatue : BossStatue
    {
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


        static bool Weaver_Awake_Prefix(BossStatue __instance)
        {
            if (__instance is WeaverBossStatue wbs)
            {
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
            }
        }


        [OnHarmonyPatch]
        void OnPatch(HarmonyPatcher patcher)
        {
            var bossStatueType = typeof(BossStatue);

            var awakeMethod = bossStatueType.GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);

            var awake_prefix = typeof(WeaverBossStatue).GetMethod(nameof(Weaver_Awake_Prefix), BindingFlags.Instance | BindingFlags.NonPublic);
            var awake_postfix = typeof(WeaverBossStatue).GetMethod(nameof(Weaver_Awake_Postfix), BindingFlags.Instance | BindingFlags.NonPublic);

            patcher.Patch(awakeMethod, awake_prefix, awake_postfix);
        }
    }
}
