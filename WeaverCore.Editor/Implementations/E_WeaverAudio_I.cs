using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Enums;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_WeaverAudio_I : WeaverAudio_I
	{
		public override AudioMixer MainMixer
		{
			get
			{
				return null;
			}
		}

		public override AudioMixerGroup MainMusic
		{
			get
			{
				return null;
			}
		}

		public override AudioMixerGroup Master
		{
			get
			{
				return null;
			}
		}

		public override AudioMixerGroup Sounds
		{
			get
			{
				return null;
			}
		}

		/*public override AudioChannel GetChannel(AudioPlayer audioObject)
		{
			return AudioChannel.None;
		}*/

		public override AudioMixerGroup GetMixerForChannel(AudioChannel channel)
		{
			return null;
		}

		/*public AudioPlayer Play(AudioClip clip, Vector3 position, float volume, AudioChannel channel, bool autoPlay, bool deleteWhenDone)
		{
			//GameObject audioObject = new GameObject("__AUDIO_OBJECT__", typeof(AudioSource), typeof(AudioPlayer));
			var audioObject = AudioPlayer.Create(position);

			return PlayReuse(audioObject, clip, position, volume, channel, autoPlay, deleteWhenDone);
		}

		public AudioPlayer PlayReuse(AudioPlayer audioObject, AudioClip clip, Vector3 position, float volume, AudioChannel channel, bool autoPlay, bool deleteWhenDone)
		{
			var audioSource = audioObject.AudioSource;//audioObject.GetComponent<AudioSource>();

			audioSource.Stop();
			audioSource.clip = clip;
			audioObject.gameObject.name = "(Sound) " + clip.name;
			audioObject.transform.position = position;
			audioSource.volume = volume;
			if (autoPlay)
			{
				audioSource.Play();
			}

			//var hollowAudio = audioObject.GetComponent<AudioPlayer>();

			if (deleteWhenDone)
			{
				audioObject.Delete(clip.length);
			}

			return audioObject;
		}*/

		/*public override void SetChannel(AudioPlayer audioObject, AudioChannel channel)
		{
			audioObject.AudioSource.outputAudioMixerGroup = null;
		}*/
	}
}
