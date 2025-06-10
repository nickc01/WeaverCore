using System.Collections;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore
{
    /// <summary>
    /// Harmony patches for AdvancedMusicCue functionality
    /// </summary>
    public static class AdvancedMusicCue_Patches
    {
        [OnHarmonyPatch]
        static void OnHarmonyPatch(HarmonyPatcher patcher)
        {
            if (Initialization.Environment == Enums.RunningState.Editor)
            {
                return;
            }
            var originalMethod = typeof(GameManager).Assembly.GetType("AudioManager").GetMethod("BeginApplyMusicCue", BindingFlags.NonPublic | BindingFlags.Instance);
            var prefixMethod = typeof(AdvancedMusicCue_Patches).GetMethod(nameof(BeginApplyMusicCuePrefix), BindingFlags.NonPublic | BindingFlags.Static);

            patcher.Patch(originalMethod, prefixMethod, null);
        }

        /// <summary>
        /// Prefix method that intercepts BeginApplyMusicCue calls
        /// </summary>
        /// <param name="__instance">The AudioManager instance</param>
        /// <param name="musicCue">The music cue being applied</param>
        /// <param name="delayTime">The delay time</param>
        /// <param name="transitionTime">The transition time</param>
        /// <param name="applySnapshot">Whether to apply snapshot</param>
        /// <param name="__result">The result to return</param>
        /// <returns>True to continue with original method, false to skip it</returns>
        static bool BeginApplyMusicCuePrefix(object __instance, MusicCue musicCue, float delayTime, float transitionTime, bool applySnapshot, ref IEnumerator __result)
        {
            if (musicCue is AdvancedMusicCue advancedCue)
            {
                var getMusicSources = CreateGetMusicSourcesDelegate(__instance);
                var updateMusicSync = CreateUpdateMusicSyncDelegate(__instance);

                __instance.GetType().GetField("currentMusicCue", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, musicCue);

                __result = advancedCue.BeginApplyMusicCue(musicCue, delayTime, transitionTime, applySnapshot, getMusicSources, updateMusicSync);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a delegate for getting music sources from AudioManager
        /// </summary>
        static AdvancedMusicCue.GetMusicSourcesDelegate CreateGetMusicSourcesDelegate(object audioManagerInstance)
        {
            var musicSourcesField = audioManagerInstance.GetType().GetField("musicSources", BindingFlags.NonPublic | BindingFlags.Instance);
            return () => (AudioSource[])musicSourcesField?.GetValue(audioManagerInstance);
        }

        /// <summary>
        /// Creates a delegate for updating music sync
        /// </summary>
        static AdvancedMusicCue.UpdateMusicSyncDelegate CreateUpdateMusicSyncDelegate(object audioManagerInstance)
        {
            var updateMusicSyncMethod = audioManagerInstance.GetType().GetMethod("UpdateMusicSync", BindingFlags.NonPublic | BindingFlags.Instance);
            return (channel, isSyncRequired) => updateMusicSyncMethod?.Invoke(audioManagerInstance, new object[] { channel, isSyncRequired });
        }
    }
}