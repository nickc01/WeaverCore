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
    /// Used for playing audio under certain channels.
    /// </summary>
    public static class WeaverAudio
	{
		[OnRegistryLoad]
		static void OnRuntimeStart(Registry registry)
		{
			if (registry.ModType == typeof(WeaverCore.Internal.WeaverCore_ModClass))
			{
				if (baseObject == null)
				{
					baseObject = WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Audio Player Prefab").GetComponent<AudioPlayer>();
				}
			}
		}

		static AudioPlayer baseObject;

		static WeaverAudio_I Impl = ImplFinder.GetImplementation<WeaverAudio_I>();

		/// <summary>
		/// Creates an audio player that can be used to play an audio clip under a certain audio channel. Use the <see cref="AudioPlayer.Play(bool)"/> function to play the sound
		/// </summary>
		/// <param name="clip">The clip to play</param>
		/// <param name="position">Where in the scene should the sound be played at</param>
		/// <param name="volume">The volme of the audio clip</param>
		/// <param name="channel">The audio channel the sound should be played under</param>
		/// <returns>Returns a new audio player for playing the audio clip</returns>
		public static AudioPlayer Create(AudioClip clip, Vector3 position, float volume = 1.0f, AudioChannel channel = AudioChannel.Sound)
		{
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

		/// <summary>
		/// Gets the main audio mixer that hollow knight uses
		/// </summary>
		public static AudioMixer MainMixer
		{
			get
			{
				return Impl.MainMixer;
			}
		}

		/// <summary>
		/// Gets the "Music" audio group for music related audio
		/// </summary>
		public static AudioMixerGroup MusicMixerGroup
		{
			get
			{
				return Impl.MainMusic;
			}
		}

		/// <summary>
		/// Gets the "Master" audio group
		/// </summary>
		public static AudioMixerGroup MasterMixerGroup
		{
			get
			{
				return Impl.Master;
			}
		}

		/// <summary>
		/// Gets the "Sounds" audio mixer for playing game sounds
		/// </summary>
		public static AudioMixerGroup SoundsMixerGroup
		{
			get
			{
				return Impl.Sounds;
			}
		}

		/// <summary>
		/// Gets the mixer group for a certain channel
		/// </summary>
		/// <param name="channel">The channel to get the mixer group for</param>
		public static AudioMixerGroup GetMixerForChannel(AudioChannel channel)
		{
			return Impl.GetMixerForChannel(channel);
		}
	}
}
