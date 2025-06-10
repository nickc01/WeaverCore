using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Utilities;

namespace WeaverCore
{
    /// <summary>
    /// An advanced music cue that allows for custom behavior when being applied.
    /// Inherits from WeaverMusicCue and provides a virtual BeginApplyMusicCue method that can be overridden.
    /// </summary>
    [CreateAssetMenu(fileName = "AdvancedMusicCue", menuName = "WeaverCore/Advanced Music Cue")]
    public class AdvancedMusicCue : WeaverMusicCue
    {
        /// <summary>
        /// Delegate for getting the music sources array
        /// </summary>
        public delegate AudioSource[] GetMusicSourcesDelegate();

        /// <summary>
        /// Delegate for updating music sync
        /// </summary>
        public delegate void UpdateMusicSyncDelegate(MusicChannels channel, bool isSyncRequired);

        static MethodInfo originalApplyMusicCue;
        static object baseAudioObject;

        /// <summary>
        /// Virtual method that gets called when the music cue is applied.
        /// Override this method to provide custom behavior.
        /// Call base.BeginApplyMusicCue() to execute the original behavior.
        /// </summary>
        /// <param name="musicCue">The music cue being applied</param>
        /// <param name="delayTime">The delay before applying the cue</param>
        /// <param name="transitionTime">The transition time</param>
        /// <param name="applySnapshot">Whether to apply the snapshot</param>
        /// <param name="getMusicSources">Delegate to get the music sources array</param>
        /// <param name="updateMusicSync">Delegate to update music sync</param>
        /// <returns>IEnumerator for the coroutine</returns>
        public virtual IEnumerator BeginApplyMusicCue(
            MusicCue musicCue,
            float delayTime,
            float transitionTime,
            bool applySnapshot,
            GetMusicSourcesDelegate getMusicSources,
            UpdateMusicSyncDelegate updateMusicSync)
        {
#if UNITY_EDITOR
            if (baseAudioObject == null)
            {
                baseAudioObject = Music.InternalAudioObject;
            }
            if (baseAudioObject != null)
            {
                if (originalApplyMusicCue == null)
                {
                    originalApplyMusicCue = baseAudioObject.GetType().GetMethod("BeginApplyMusicCue", BindingFlags.NonPublic | BindingFlags.Instance);
                }

                return (IEnumerator)originalApplyMusicCue.Invoke(baseAudioObject, new object[] { musicCue, delayTime, transitionTime, applySnapshot });
            }

            return null;
#else
            if (baseAudioObject == null)
            {
                baseAudioObject = Music.InternalAudioObject;
            }
            if (baseAudioObject != null)
            {
                if (originalApplyMusicCue == null)
                {
                    originalApplyMusicCue = baseAudioObject.GetType().GetMethod("ApplyMusicCue", BindingFlags.NonPublic | BindingFlags.Instance);
                }

                return (IEnumerator)originalApplyMusicCue.Invoke(baseAudioObject, new object[] { musicCue, delayTime, transitionTime, applySnapshot });
            }
#endif
        }
    }
}