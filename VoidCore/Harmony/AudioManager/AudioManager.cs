using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace VoidCore.Harmony
{
    [HarmonyPatch(typeof(AudioManager))]
    [HarmonyPatch("BeginApplyMusicCue")]
    [HarmonyPatch(new Type[] { typeof(MusicCue),typeof(float),typeof(float),typeof(bool) })]
    internal class AudioManagerBeginApplyMusicCue
    {
        static bool Prefix(AudioManager __instance,ref MusicCue musicCue,ref AudioSource[] ___musicSources)
        {
            try
            {
                foreach (var music in ___musicSources)
                {
                    VoidModLog.Log("MUSIC = " + music);
                }

                var channels = typeof(MusicCue).GetField("channelInfos", BindingFlags.NonPublic | BindingFlags.Instance);
                var info = (MusicCue.MusicChannelInfo[])channels.GetValue(musicCue);
                foreach (var i in info)
                {
                    VoidModLog.Log("CLIP = " + i.Clip?.name);
                }
                return true;
            }
            catch (Exception e)
            {
                VoidModLog.LogError(e);
            }
            return true;
        }
    }
}
