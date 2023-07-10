using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
    public class G_SpriteFlasher_I : SpriteFlasher_I
    {
        public override void OnFlasherInit(SpriteFlasher flasher)
        {
			flasher.gameObject.AddComponent<SpriteFlashProxy>();
        }
    }

    public class G_AudioMixer_I : AudioMixer_I
	{
		static AudioMixer[] Mixers;
		static AudioMixerGroup[] Groups;
		static AudioMixerSnapshot[] Snapshots;

		static bool assetsLoaded = false;


		public override AudioMixerGroup GetGroupForMixer(AudioMixer mixer, string groupName)
		{
			LoadAssets();
			int length = Groups.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				if (Groups[i].audioMixer == mixer && Groups[i].name == groupName)
				{
					return Groups[i];
				}
			}
			return null;
		}

		public override AudioMixer GetMixer(string mixerName)
		{
			LoadAssets();
			int length = Mixers.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				if (Mixers[i].name == mixerName)
				{
					return Mixers[i];
				}
			}
			return null;
		}

		public override AudioMixerSnapshot GetSnapshotForMixer(AudioMixer mixer, string snapshotName)
		{
			LoadAssets();
			int length = Snapshots.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				if (Snapshots[i].audioMixer == mixer && Snapshots[i].name == snapshotName)
				{
					return Snapshots[i];
				}
			}
			return null;
		}

		[OnRuntimeInit]
		static void LoadAssets()
		{
			if (assetsLoaded)
			{
				return;
			}

			assetsLoaded = true;

			Mixers = Resources.FindObjectsOfTypeAll<AudioMixer>();
			Snapshots = Resources.FindObjectsOfTypeAll<AudioMixerSnapshot>();
			Groups = Resources.FindObjectsOfTypeAll<AudioMixerGroup>();

			LoadCueModifiers();
		}

		static Func<MusicCue, MusicCue.MusicChannelInfo[]> GetChannelInfos;
		static Action<MusicCue, MusicCue.MusicChannelInfo[]> SetChannelInfos;

		static Action<MusicCue, MusicCue.Alternative[]> SetAlternatives;
		static Action<MusicCue, AudioMixerSnapshot> SetSnapshot;

		static Action<MusicCue.MusicChannelInfo, AudioClip> SetInfoAudioClip;
		static Action<MusicCue.MusicChannelInfo, MusicChannelSync> SetInfoSync;

		static void LoadCueModifiers()
		{
			GetChannelInfos = ReflectionUtilities.CreateFieldGetter<MusicCue, MusicCue.MusicChannelInfo[]>("channelInfos");
			SetChannelInfos = ReflectionUtilities.CreateFieldSetter<MusicCue, MusicCue.MusicChannelInfo[]>("channelInfos");

			SetAlternatives = ReflectionUtilities.CreateFieldSetter<MusicCue, MusicCue.Alternative[]>("alternatives");
			SetSnapshot = ReflectionUtilities.CreateFieldSetter<MusicCue, AudioMixerSnapshot>("snapshot");

			SetInfoAudioClip = ReflectionUtilities.CreateFieldSetter<MusicCue.MusicChannelInfo, AudioClip>("clip");
			SetInfoSync = ReflectionUtilities.CreateFieldSetter<MusicCue.MusicChannelInfo, MusicChannelSync>("sync");
		}

		static HashSet<MusicCue> CreatedCues = new HashSet<MusicCue>();

		//NOTE : This creates a MusicCue that gets deleted when CollectUnusedCues() is called
		public MusicCue CreateCueFromPack(MusicPack pack)
		{
			LoadAssets();
			var cue = ScriptableObject.CreateInstance<MusicCue>();

			MusicCue.MusicChannelInfo[] channelInfos = new MusicCue.MusicChannelInfo[6];

			channelInfos[0] = new MusicCue.MusicChannelInfo();
			channelInfos[1] = new MusicCue.MusicChannelInfo();
			channelInfos[2] = new MusicCue.MusicChannelInfo();
			channelInfos[3] = new MusicCue.MusicChannelInfo();
			channelInfos[4] = new MusicCue.MusicChannelInfo();
			channelInfos[5] = new MusicCue.MusicChannelInfo();

			SetInfoAudioClip(channelInfos[0], pack.MainTrack);
			SetInfoSync(channelInfos[0], (MusicChannelSync)(int)pack.MainTrackSync);

			SetInfoAudioClip(channelInfos[1], pack.MainAltTrack);
			SetInfoSync(channelInfos[1], (MusicChannelSync)(int)pack.MainAltTrackSync);

			SetInfoAudioClip(channelInfos[2], pack.ActionTrack);
			SetInfoSync(channelInfos[2], (MusicChannelSync)(int)pack.ActionTrackSync);

			SetInfoAudioClip(channelInfos[3], pack.SubTrack);
			SetInfoSync(channelInfos[3], (MusicChannelSync)(int)pack.SubTrackSync);

			SetInfoAudioClip(channelInfos[4], pack.TensionTrack);
			SetInfoSync(channelInfos[4], (MusicChannelSync)(int)pack.TensionTrackSync);

			SetInfoAudioClip(channelInfos[5], pack.ExtraTrack);
			SetInfoSync(channelInfos[5], (MusicChannelSync)(int)pack.ExtraTrackSync);

			SetChannelInfos(cue, channelInfos);
			SetAlternatives(cue, new MusicCue.Alternative[0]);
			SetSnapshot(cue, Music.GetSnapshot(pack.Snapshot));
			CreatedCues.Add(cue);
			return cue;
		}

		static void CollectUnusedCues()
		{
			foreach (var cue in CreatedCues)
			{
				if (cue != GameManager.instance.AudioManager.CurrentMusicCue)
				{
					GameObject.Destroy(cue,Time.unscaledDeltaTime * 2f);
				}
			}
			CreatedCues.RemoveWhere(c => c != GameManager.instance.AudioManager.CurrentMusicCue);
		}


		public override void PlayMusicPack(MusicPack pack, float delayTime, float snapshotTransitionTime, bool applySnapshot = true)
		{
			var cue = CreateCueFromPack(pack);
			GameManager.instance.AudioManager.ApplyMusicCue(cue, delayTime, snapshotTransitionTime, applySnapshot);
			if (applySnapshot)
			{
				var snapshot = Music.GetSnapshot(pack.Snapshot);
				snapshot.TransitionTo(snapshotTransitionTime);
			}

			CollectUnusedCues();
		}

		public override void ApplyMusicSnapshot(AudioMixerSnapshot snapshot, float delayTime, float transitionTime)
		{
			GameManager.instance.AudioManager.ApplyMusicSnapshot(snapshot, delayTime, transitionTime);
		}

		static AtmosCue currentCue = null;
		static FieldInfo cueSnapshotSetter = null;
		static FieldInfo cueChannelEnabledSetter = null;

		public override void ApplyAtmosSnapshot(Atmos.SnapshotType snapshot, float transitionTime, Atmos.AtmosSources enabledSources)
		{
			if (currentCue != null)
			{
				GameObject.Destroy(currentCue, 1f + transitionTime);
			}
			currentCue = ScriptableObject.CreateInstance<AtmosCue>();
			if (cueSnapshotSetter == null)
			{
				cueSnapshotSetter = typeof(AtmosCue).GetField("snapshot", BindingFlags.NonPublic | BindingFlags.Instance);
				cueChannelEnabledSetter = typeof(AtmosCue).GetField("isChannelEnabled", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			bool[] enabledChannels = new bool[16];

			int counter = 0;
			for (int i = 1; i < (int)Atmos.AtmosSources.Everything; i *= 2)
			{
				enabledChannels[counter] = (enabledSources & (Atmos.AtmosSources)i) == (Atmos.AtmosSources)i;
				counter++;
			}

			cueSnapshotSetter.SetValue(currentCue, Atmos.GetSnapshot(snapshot));
			cueChannelEnabledSetter.SetValue(currentCue, enabledChannels);

			GameManager.instance.AudioManager.ApplyAtmosCue(currentCue, transitionTime);
		}
	}
}
