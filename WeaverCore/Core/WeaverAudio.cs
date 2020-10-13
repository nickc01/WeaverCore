using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using WeaverCore.Enums;
using WeaverCore.Attributes;

namespace WeaverCore
{
	/// <summary>
	/// Used for playing sounds within hollow knight with the game's standard audio effects applied
	/// </summary>
	public static class WeaverAudio
	{
		[OnRuntimeInit]
		static void OnRuntimeStart()
		{
			if (AudioPlayerPool == null)
			{
				AudioPlayerPool = new ObjectPool(WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Audio Player Prefab"), PoolLoadType.Local);
				AudioPlayerPool.FillPool(1);
			}
		}

		static ObjectPool AudioPlayerPool;
		//static WeaverAudioPlayer baseObject;

		static WeaverAudio_I Impl = ImplFinder.GetImplementation<WeaverAudio_I>();

		public static WeaverAudioPlayer Create(AudioClip clip, Vector3 position, float volume = 1.0f, AudioChannel channel = AudioChannel.Sound)
		{
			var audioObject = AudioPlayerPool.Instantiate<WeaverAudioPlayer>(position, Quaternion.identity);
			var audioSource = audioObject.AudioSource;

			audioObject.Channel = channel;
			audioSource.playOnAwake = false;
			audioObject.SourcePool = AudioPlayerPool;
			audioSource.Stop();
			audioSource.clip = clip;
			audioObject.gameObject.name = "(Sound) " + clip.name;
			audioSource.volume = volume;

			return audioObject;
		}

		public static WeaverAudioPlayer PlayAtPoint(AudioClip clip, Vector3 position, float volume = 1.0f, AudioChannel channel = AudioChannel.Sound)
		{
			var audioObject = Create(clip, position, volume, channel);
			if (clip != null)
			{
				audioObject.AudioSource.Play();
				audioObject.Delete(clip.length);
			}

			return audioObject;
		}

		/*public static void PlayReuse(WeaverAudioPlayer audioObject, AudioClip clip, Vector3 position, float volume = 1.0f, AudioChannel channel = AudioChannel.Sound, bool autoPlay = true, bool deleteWhenDone = false)
		{
			if (Impl == null)
			{
				Impl = ImplFinder.GetImplementation<WeaverAudio_I>();
			}
			Impl.PlayReuse(audioObject, clip, position, volume, channel, autoPlay, deleteWhenDone);
		}*/

		/// <summary>
		/// Gets the main audio mixer that hollow knight uses. Currently returns null if used in the editor
		/// </summary>
		public static AudioMixer MainMixer
		{
			get
			{
				return Impl.MainMixer;
			}
		}

		public static AudioMixerGroup MusicMixerGroup
		{
			get
			{
				return Impl.MainMusic;
			}
		}
		public static AudioMixerGroup MasterMixerGroup
		{
			get
			{
				return Impl.Master;
			}
		}
		public static AudioMixerGroup SoundsMixerGroup
		{
			get
			{
				return Impl.Sounds;
			}
		}

		public static AudioMixerGroup GetMixerForChannel(AudioChannel channel)
		{
			return Impl.GetMixerForChannel(channel);
		}
	}
}
