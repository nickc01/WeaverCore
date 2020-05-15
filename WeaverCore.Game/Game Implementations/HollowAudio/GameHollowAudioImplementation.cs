using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Game.Patches;
using WeaverCore.Helpers;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	public class GameHollowAudioImplementation : HollowAudioImplementation
	{
		public override HollowPlayer.HollowAudioObject Play(AudioClip clip, Vector3 position, float volume, AudioChannel channel, bool autoPlay)
		{
			GameObject audioObject = new GameObject("__AUDIO_OBJECT__", typeof(AudioSource), typeof(HollowPlayer.HollowAudioObject));

			var audioSource = audioObject.GetComponent<AudioSource>();

			audioSource.clip = clip;
			audioObject.transform.position = position;
			audioSource.volume = volume;

			switch (channel)
			{
				case AudioChannel.Master:
					audioSource.outputAudioMixerGroup = AudioHunter.MixerGroups.FirstOrDefault(mixer => mixer.name == "Master");
					break;
				case AudioChannel.Sound:
					audioSource.outputAudioMixerGroup = AudioHunter.MixerGroups.FirstOrDefault(mixer => mixer.name == "Actors");
					break;
				case AudioChannel.Music:
					audioSource.outputAudioMixerGroup = AudioHunter.MixerGroups.FirstOrDefault(mixer => mixer.name == "Main");
					break;
			}

			if (autoPlay)
			{
				audioSource.Play();
			}

			return audioObject.GetComponent<HollowPlayer.HollowAudioObject>();
		}
	}
}
