using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class WeaverAudioImplementation : IImplementation
	{
		public abstract WeaverAudioPlayer Play(AudioClip clip, Vector3 position, float volume, AudioChannel channel, bool autoPlay, bool deleteWhenDone);
		public abstract WeaverAudioPlayer PlayReuse(WeaverAudioPlayer audioObject, AudioClip clip, Vector3 position, float volume, AudioChannel channel, bool autoPlay, bool deleteWhenDone);

		public abstract void SetObjectChannel(WeaverAudioPlayer audioObject, AudioChannel channel);
		public abstract AudioChannel GetObjectChannel(WeaverAudioPlayer audioObject);

		public abstract AudioMixer MainMixer { get; }
	}
}
