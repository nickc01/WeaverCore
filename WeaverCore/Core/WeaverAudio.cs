using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using WeaverCore.Enums;

namespace WeaverCore
{
	/// <summary>
	/// Used for playing sounds within hollow knight with the game's standard audio effects applied
	/// </summary>
	public static class WeaverAudio
	{
		internal static WeaverAudio_I Impl;

		public static WeaverAudioPlayer Play(AudioClip clip, Vector3 position, float volume = 1.0f, AudioChannel channel = AudioChannel.Sound, bool autoPlay = true, bool deleteWhenDone = true)
		{
			if (Impl == null)
			{
				Impl = ImplFinder.GetImplementation<WeaverAudio_I>();
			}

			return Impl.Play(clip, position, volume, channel, autoPlay, deleteWhenDone);
		}

		public static void PlayReuse(WeaverAudioPlayer audioObject, AudioClip clip, Vector3 position, float volume = 1.0f, AudioChannel channel = AudioChannel.Sound, bool autoPlay = true, bool deleteWhenDone = false)
		{
			if (Impl == null)
			{
				Impl = ImplFinder.GetImplementation<WeaverAudio_I>();
			}
			Impl.PlayReuse(audioObject, clip, position, volume, channel, autoPlay, deleteWhenDone);
		}

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
	}
}
