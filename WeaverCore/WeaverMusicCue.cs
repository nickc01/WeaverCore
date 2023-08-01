using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore
{
	[CreateAssetMenu(fileName = "WeaverMusicCue", menuName = "WeaverCore/Weaver Music Cue", order = 1000)]
    public class WeaverMusicCue : MusicCue, ISerializationCallbackReceiver
	{
        static Func<MusicCue, MusicChannelInfo[]> channelInfosGetter;
        static Func<MusicCue, Alternative[]> alternativesGetter;

        static Action<MusicCue, MusicChannelInfo[]> channelInfosSetter;
        static Action<MusicCue, Alternative[]> alternativesSetter;

        [Serializable]
        struct DataContainer
        {
            public MusicChannelInfo[] channelInfos;
            public Alternative[] alternatives;
        }

		[SerializeField]
        [HideInInspector]
        string cueData;

        [SerializeField]
        [Tooltip("If set to true, then the audio snapshot specified below will be used when the music cue is applied")]
        bool useSnapshot = true;

        [SerializeField]
        Music.SnapshotType m_snapshot;

        public Music.SnapshotType SnapshotType => m_snapshot;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            //cueData = "";

            cueData = JsonUtility.ToJson(new DataContainer
            {
                channelInfos = channelInfosGetter(this),
                alternatives = alternativesGetter(this)
            }, false);
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#if !UNITY_EDITOR
            var container = JsonUtility.FromJson<DataContainer>(cueData);

            channelInfosSetter(this, container.channelInfos);
            alternativesSetter(this, container.alternatives);
#endif
        }

        private void Reset()
        {
            channelInfosSetter(this,new MusicChannelInfo[6]);
        }

        [OnInit]
        static void Init()
        {
            channelInfosGetter = ReflectionUtilities.CreateFieldGetter<MusicCue, MusicChannelInfo[]>("channelInfos");
            alternativesGetter = ReflectionUtilities.CreateFieldGetter<MusicCue, Alternative[]>("alternatives");

            channelInfosSetter = ReflectionUtilities.CreateFieldSetter<MusicCue, MusicChannelInfo[]>("channelInfos");
            alternativesSetter = ReflectionUtilities.CreateFieldSetter<MusicCue, Alternative[]>("alternatives");
        }

        [OnHarmonyPatch]
        static void Patch(HarmonyPatcher patcher)
        {
            var snapshotGetter = typeof(MusicCue).GetProperty(nameof(Snapshot)).GetGetMethod();

            var prefix = typeof(WeaverMusicCue).GetMethod(nameof(SnapshotGetterPrefix), BindingFlags.NonPublic | BindingFlags.Static);

            patcher.Patch(snapshotGetter, prefix, null);
        }

        static bool SnapshotGetterPrefix(MusicCue __instance, ref AudioMixerSnapshot __result)
        {
            if (__instance is WeaverMusicCue wmc)
            {
                if (wmc.useSnapshot)
                {
                    __result = Music.GetSnapshot(wmc.SnapshotType);
                }
                else
                {
                    __result = null;
                }
                return false;
            }
            return true;
        }
    }
}
