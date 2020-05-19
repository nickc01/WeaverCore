using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class EditorHollowAudioImplementation : HollowAudioImplementation
	{
		public override HollowPlayer.HollowAudioObject Play(AudioClip clip, Vector3 position, float volume, AudioChannel channel, bool autoPlay)
		{
			GameObject audioObject = new GameObject("__AUDIO_OBJECT__", typeof(AudioSource), typeof(HollowPlayer.HollowAudioObject));

			var audioSource = audioObject.GetComponent<AudioSource>();

			audioSource.clip = clip;
			audioObject.transform.position = position;
			audioSource.volume = volume;
			if (autoPlay)
			{
				audioSource.Play();
			}

			return audioObject.GetComponent<HollowPlayer.HollowAudioObject>();
		}
	}
}
