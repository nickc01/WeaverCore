using System;
using UnityEngine;
using UnityEngine.Audio;


//[CreateAssetMenu(fileName = "MusicCue", menuName = "Hollow Knight/Music Cue", order = 1000)]
public class MusicCue : ScriptableObject
{
    [Serializable]
    public class MusicChannelInfo
    {
        [SerializeField]
        private AudioClip clip;

        [SerializeField]
        private MusicChannelSync sync;

        public AudioClip Clip => clip;

        public bool IsEnabled => clip != null;

        public bool IsSyncRequired
        {
            get
            {
                if (sync == MusicChannelSync.Implicit)
                {
                    return clip != null;
                }
                if (sync == MusicChannelSync.ExplicitOn)
                {
                    return true;
                }
                return false;
            }
        }
    }

    [Serializable]
    public struct Alternative
    {
        public string PlayerDataBoolKey;

        public MusicCue Cue;
    }

    [SerializeField]
    private string originalMusicEventName;

    [SerializeField]
    private int originalMusicTrackNumber;

    [SerializeField]
    [HideInInspector]
    private AudioMixerSnapshot snapshot;

    [SerializeField]
    private MusicChannelInfo[] channelInfos;

    [SerializeField]
    private Alternative[] alternatives;

    public string OriginalMusicEventName => originalMusicEventName;

    public int OriginalMusicTrackNumber => originalMusicTrackNumber;

    public AudioMixerSnapshot Snapshot => snapshot;

    public MusicChannelInfo GetChannelInfo(MusicChannels channel)
    {
        if (channel < MusicChannels.Main || (int)channel >= channelInfos.Length)
        {
            return null;
        }
        return channelInfos[(int)channel];
    }

    public MusicCue ResolveAlternatives(PlayerData playerData)
    {
        if (alternatives != null)
        {
            for (int i = 0; i < alternatives.Length; i++)
            {
                Alternative alternative = alternatives[i];
                if (playerData.GetBool(alternative.PlayerDataBoolKey))
                {
                    MusicCue cue = alternative.Cue;
                    if (!(cue != null))
                    {
                        return null;
                    }
                    return cue.ResolveAlternatives(playerData);
                }
            }
        }
        return this;
    }
}
