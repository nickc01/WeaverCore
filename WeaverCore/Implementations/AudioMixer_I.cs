﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Audio;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Implementations
{
	public abstract class AudioMixer_I : IImplementation
	{
		public static AudioMixer_I Instance = ImplFinder.GetImplementation<AudioMixer_I>();


		public abstract AudioMixer GetMixer(string mixerName);

		public abstract AudioMixerSnapshot GetSnapshotForMixer(AudioMixer mixer, string snapshotName);

		public abstract AudioMixerGroup GetGroupForMixer(AudioMixer mixer, string groupName);

		public abstract void PlayMusicPack(MusicPack pack, float delayTime, float snapshotTransitionTime, bool applySnapshot = true);

		public abstract void ApplyMusicSnapshot(AudioMixerSnapshot snapshot, float delayTime, float transitionTime);
	}
}
