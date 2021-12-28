using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Enums;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class WeaverAudio_I : IImplementation
	{
		public abstract AudioMixerGroup GetMixerForChannel(AudioChannel channel);

		public abstract AudioMixer MainMixer { get; }
		public abstract AudioMixerGroup MainMusic { get; }
		public abstract AudioMixerGroup Master { get; }
		public abstract AudioMixerGroup Sounds { get; }
	}
}
