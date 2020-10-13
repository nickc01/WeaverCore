using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Game.Patches;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
	public class G_WeaverAudio_I : WeaverAudio_I
	{
		static List<AudioMixerGroup> MixerGroups = new List<AudioMixerGroup>();

		public override AudioMixer MainMixer => _master.audioMixer;

		static AudioMixerGroup _music;
		public override AudioMixerGroup MainMusic => _music;

		static AudioMixerGroup _master;
		public override AudioMixerGroup Master => _master;

		static AudioMixerGroup _sounds;
		public override AudioMixerGroup Sounds => _sounds;

		[OnInit]
		static void Init()
		{
			UnboundCoroutine.Start(ScanForMixers());
		}

		static IEnumerator ScanForMixers()
		{
			yield return null;
			foreach (var audioSource in GameObject.FindObjectsOfType<AudioSource>())
			{
				if (audioSource.outputAudioMixerGroup != null && !MixerGroups.Contains(audioSource.outputAudioMixerGroup))
				{
					MixerGroups.Add(audioSource.outputAudioMixerGroup);
				}
			}

			if (_master == null)
			{
				_master = MixerGroups.FirstOrDefault(mixer => mixer.name == "Master");
			}
			if (_sounds == null)
			{
				_sounds = MixerGroups.FirstOrDefault(mixer => mixer.name == "Actors");
			}
			if (_music == null)
			{
				_music = MixerGroups.FirstOrDefault(mixer => mixer.name == "Main");
			}
		}

		public override AudioMixerGroup GetMixerForChannel(AudioChannel channel)
		{
			switch (channel)
			{
				case AudioChannel.Master:
					return Master;
				case AudioChannel.Sound:
					return Sounds;
				case AudioChannel.Music:
					return MainMusic;
				default:
					return null;
			}
		}

		/*public override WeaverAudioPlayer Play(AudioClip clip, Vector3 position, float volume, AudioChannel channel, bool autoPlay, bool deleteWhenDone)
		{
			var audioObject = WeaverAudioPlayer.Create(position);

			return PlayReuse(audioObject, clip, position, volume, channel, autoPlay, deleteWhenDone);
		}

		public override WeaverAudioPlayer PlayReuse(WeaverAudioPlayer audioObject, AudioClip clip, Vector3 position, float volume, AudioChannel channel, bool autoPlay, bool deleteWhenDone)
		{
			var audioSource = audioObject.AudioSource;

			audioSource.Stop();
			audioSource.clip = clip;
			audioObject.transform.position = position;
			audioSource.volume = volume;

			SetObjectChannel(audioObject, channel);

			if (autoPlay)
			{
				audioSource.Play();
			}

			if (deleteWhenDone)
			{
				audioObject.Delete(clip.length);
			}

			return audioObject;
		}

		public override void SetObjectChannel(WeaverAudioPlayer audioObject, AudioChannel channel)
		{
			switch (channel)
			{
				case AudioChannel.Master:
					audioObject.AudioSource.outputAudioMixerGroup = Master;
					break;
				case AudioChannel.Sound:
					audioObject.AudioSource.outputAudioMixerGroup = Sounds;
					break;
				case AudioChannel.Music:
					audioObject.AudioSource.outputAudioMixerGroup = MainMusic;
					break;
			}
		}

		public override AudioChannel GetObjectChannel(WeaverAudioPlayer audioObject)
		{
			if (audioObject.AudioSource.outputAudioMixerGroup == Master)
			{
				return AudioChannel.Master;
			}
			else if (audioObject.AudioSource.outputAudioMixerGroup == Sounds)
			{
				return AudioChannel.Sound;
			}
			else if (audioObject.AudioSource.outputAudioMixerGroup == MainMusic)
			{
				return AudioChannel.Music;
			}
			else
			{
				return AudioChannel.None;
			}
		}*/
	}
}
