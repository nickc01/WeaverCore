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
		//[OnRuntimeInit]
		[OnRegistryLoad]
		static void OnRuntimeStart(Registry registry)
		{
			//WeaverLog.Log("Registry = " + registry);
			/*if (registry != null)
			{
				WeaverLog.Log("Mod Assembly Name = " + registry.ModAssemblyName);
				WeaverLog.Log("Mod Name = " + registry.ModName);
				WeaverLog.Log("Mod Registry Name = " + registry.RegistryName);
				WeaverLog.Log("Mod Registry Enabled = " + registry.RegistryEnabled);
				WeaverLog.Log("Mod Type = " + registry.ModType);
			}*/
			if (registry.ModType == typeof(WeaverCore.Internal.WeaverCore_ModClass))
			{
				if (baseObject == null)
				{
					baseObject = WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Audio Player Prefab").GetComponent<AudioPlayer>();
					//AudioPlayerPool = new ObjectPool(WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Audio Player Prefab"), PoolLoadType.Local);
					//AudioPlayerPool.FillPool(1);
				}
			}
		}

		//static ObjectPool AudioPlayerPool;
		static AudioPlayer baseObject;

		static WeaverAudio_I Impl = ImplFinder.GetImplementation<WeaverAudio_I>();

		public static AudioPlayer Create(AudioClip clip, Vector3 position, float volume = 1.0f, AudioChannel channel = AudioChannel.Sound)
		{
			//var audioObject = AudioPlayerPool.Instantiate<AudioPlayer>(position, Quaternion.identity);
			var audioObject = Pooling.Instantiate(baseObject, position, Quaternion.identity);
			var audioSource = audioObject.AudioSource;

			audioObject.Channel = channel;
			audioSource.playOnAwake = false;
			audioSource.Stop();
			audioSource.clip = clip;
			audioSource.pitch = 1f;
			audioSource.loop = false;
			audioObject.gameObject.name = "(Sound) " + clip.name;
			audioSource.volume = volume;

			return audioObject;
		}

		/// <summary>
		/// Plays a clip at the specified point. Automatically deletes itself when the clip is done playing. For more control, refer to <see cref="Create(AudioClip, Vector3, float, AudioChannel)"/>
		/// </summary>
		/// <param name="clip">The clip to play</param>
		/// <param name="position">The position to play at</param>
		/// <param name="volume">How loud the player will be</param>
		/// <param name="channel">What audio channel the player will be playing under</param>
		/// <returns>The player that is playing the audio</returns>
		public static AudioPlayer PlayAtPoint(AudioClip clip, Vector3 position, float volume = 1.0f, AudioChannel channel = AudioChannel.Sound)
		{
			var audioObject = Create(clip, position, volume, channel);
			if (clip != null)
			{
				audioObject.AudioSource.Play();
				audioObject.Delete(clip.length);
			}

			return audioObject;
		}

		/// <summary>
		/// Plays a clip forever untill it's manually destroyed
		/// </summary>
		/// <param name="clip">The clip to play</param>
		/// <param name="position">The position to play at</param>
		/// <param name="volume">How loud the player will be</param>
		/// <param name="channel">What audio channel the player will be playing under</param>
		/// <returns>The player that is playing the audio</returns>
		public static AudioPlayer PlayAtPointLooped(AudioClip clip, Vector3 position, float volume = 1.0f, AudioChannel channel = AudioChannel.Sound)
		{
			var audioObject = Create(clip, position, volume, channel);
			audioObject.AudioSource.loop = true;
			audioObject.AudioSource.Play();
			return audioObject;
		}

		/*public static void PlayReuse(AudioPlayer audioObject, AudioClip clip, Vector3 position, float volume = 1.0f, AudioChannel channel = AudioChannel.Sound, bool autoPlay = true, bool deleteWhenDone = false)
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
