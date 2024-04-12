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

		public abstract float MasterVolume { get; }
		public abstract float MusicVolume { get; }
		public abstract float SoundsVolume { get; }

        public abstract event Action<float> OnMasterVolumeUpdate;
        public abstract event Action<float> OnMusicVolumeUpdate;
        public abstract event Action<float> OnSoundVolumeUpdate;

        public abstract event Action<bool> OnPauseStateUpdate;
    }
}
