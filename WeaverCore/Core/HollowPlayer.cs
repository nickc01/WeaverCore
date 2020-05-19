using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;
using WeaverCore.Implementations;

namespace WeaverCore
{
	/// <summary>
	/// Used for playing sounds within hollow knight with the game's standard audio effects applied
	/// </summary>
	public static class HollowPlayer
	{
		static HollowAudioImplementation Impl;

		public static HollowAudioObject Play(AudioClip clip, Vector3 position, float volume = 1.0f, AudioChannel channel = AudioChannel.Master, bool autoPlay = true)
		{
			if (Impl == null)
			{
				Impl = ImplFinder.GetImplementation<HollowAudioImplementation>();
			}

			return Impl.Play(clip, position, volume, channel, autoPlay);
		}


		public class HollowAudioObject : MonoBehaviour
		{
			private AudioSource audioSource;
			public AudioSource AudioSource
			{
				get
				{
					if (audioSource == null)
					{
						audioSource = GetComponent<AudioSource>();
					}
					return audioSource;
				}
			}

			void Start()
			{
				if (AudioSource == null || AudioSource.clip == null)
				{
					Destroy(gameObject);
				}
				else
				{
					Destroy(gameObject, AudioSource.clip.length + 1.0f);
				}
			}
		}

	}
}
