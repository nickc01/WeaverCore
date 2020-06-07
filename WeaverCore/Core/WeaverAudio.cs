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
		public static AudioMixer MainMixer => Impl.MainMixer;
	}

	public class WeaverAudioPlayer : MonoBehaviour
	{
		private AudioSource audioSource;
		public AudioSource AudioSource
		{
			get
			{
				if (audioSource == null)
				{
					audioSource = GetComponent<AudioSource>();
					if (audioSource == null)
					{
						audioSource = gameObject.AddComponent<AudioSource>();
					}
				}
				return audioSource;
			}
		}

		public AudioClip Clip
		{
			get => AudioSource.clip;
			set
			{
				if (AudioSource.isPlaying)
				{
					AudioSource.Stop();
				}
				AudioSource.clip = value;
			}
		}

		public AudioChannel Channel
		{
			get => WeaverAudio.Impl.GetObjectChannel(this);
			set => WeaverAudio.Impl.SetObjectChannel(this, value);
		}

		public float Volume
		{
			get => AudioSource.volume;
			set => AudioSource.volume = value;
		}

		public void Delete(float timer = 0f)
		{
			Destroy(gameObject, timer);
		}

		public void Play(bool deleteWhenDone = false)
		{
			if (Clip != null)
			{
				audioSource.Play();
			}
			if (deleteWhenDone)
			{
				if (Clip == null)
				{
					Delete();
				}
				else
				{
					Delete(Clip.length);
				}
			}
		}

		public void Play(AudioClip clip, float volume = 1f, AudioChannel channel = AudioChannel.Sound, bool deleteWhenDone = false)
		{
			Clip = clip;
			Volume = volume;
			Channel = channel;
			Play(deleteWhenDone);
		}
	}
}
