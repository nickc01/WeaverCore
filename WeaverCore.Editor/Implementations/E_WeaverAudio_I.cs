using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_WeaverAudio_I : WeaverAudio_I
	{
		public override AudioMixer MainMixer => null;

		public override AudioChannel GetObjectChannel(WeaverAudioPlayer audioObject)
		{
			return AudioChannel.None;
		}

		public override WeaverAudioPlayer Play(AudioClip clip, Vector3 position, float volume, AudioChannel channel, bool autoPlay, bool deleteWhenDone)
		{
			GameObject audioObject = new GameObject("__AUDIO_OBJECT__", typeof(AudioSource), typeof(WeaverAudioPlayer));

			return PlayReuse(audioObject.GetComponent<WeaverAudioPlayer>(), clip, position, volume, channel, autoPlay, deleteWhenDone);
		}

		public override WeaverAudioPlayer PlayReuse(WeaverAudioPlayer audioObject, AudioClip clip, Vector3 position, float volume, AudioChannel channel, bool autoPlay, bool deleteWhenDone)
		{
			var audioSource = audioObject.GetComponent<AudioSource>();

			audioSource.Stop();
			audioSource.clip = clip;
			audioObject.transform.position = position;
			audioSource.volume = volume;
			if (autoPlay)
			{
				audioSource.Play();
			}

			var hollowAudio = audioObject.GetComponent<WeaverAudioPlayer>();

			if (deleteWhenDone)
			{
				hollowAudio.Delete(clip.length);
			}

			return hollowAudio;
		}

		public override void SetObjectChannel(WeaverAudioPlayer audioObject, AudioChannel channel)
		{
			audioObject.AudioSource.outputAudioMixerGroup = null;
		}
	}
}
