using System.Reflection;
using UnityEngine.Audio;
using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{
    static class TESTING_PATCHES
    {
        /*[OnHarmonyPatch]
        static void OnHarmonyPatch(HarmonyPatcher patcher)
        {
            {
                var orig = typeof(AudioMixer).GetMethod("TransitionToSnapshot", BindingFlags.NonPublic | BindingFlags.Instance);

                var prefix = typeof(TESTING_PATCHES).GetMethod(nameof(TransitionToSnapshot));

                patcher.Patch(orig, prefix, null);
            }

            On.AudioManager.ApplyMusicCue += AudioManager_ApplyMusicCue;
        }

        private static void AudioManager_ApplyMusicCue(On.AudioManager.orig_ApplyMusicCue orig, AudioManager self, MusicCue musicCue, float delayTime, float transitionTime, bool applySnapshot)
        {
            WeaverLog.Log("APPLYING MUSIC CUE = " + musicCue);
            WeaverLog.Log(new System.Diagnostics.StackTrace());
            orig(self, musicCue, delayTime, transitionTime, applySnapshot);
        }

        public static bool TransitionToSnapshot(AudioMixer __instance, AudioMixerSnapshot snapshot, float timeToReach)
        {
            WeaverLog.Log("TRANSITIONING TO SNAPSHOT = " + snapshot + " to mixer = " + __instance.name);
            //WeaverLog.Log(new System.Diagnostics.StackTrace());
            return true;
        }*/
    }
}
