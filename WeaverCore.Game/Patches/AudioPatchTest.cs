using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Interfaces;

namespace WeaverCore.Game.Patches
{
	class AudioPatchTest : IInit
	{
		private void AudioManager_ApplyMusicSnapshot(On.AudioManager.orig_ApplyMusicSnapshot orig, AudioManager self, UnityEngine.Audio.AudioMixerSnapshot snapshot, float delayTime, float transitionTime)
		{
			Debugger.Log("Music Snapshot Changed");
			orig(self,snapshot,delayTime,transitionTime);
		}

		private void AudioManager_ApplyMusicCue(On.AudioManager.orig_ApplyMusicCue orig, AudioManager self, MusicCue musicCue, float delayTime, float transitionTime, bool applySnapshot)
		{
			Debugger.Log("Music Cue Applied");
			orig(self,musicCue,delayTime,transitionTime,applySnapshot);
		}

		void IInit.OnInit()
		{
			On.AudioManager.ApplyMusicCue += AudioManager_ApplyMusicCue;
			On.AudioManager.ApplyMusicSnapshot += AudioManager_ApplyMusicSnapshot;
		}
	}
}
