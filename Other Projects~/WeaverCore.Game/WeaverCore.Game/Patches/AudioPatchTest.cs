using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;

namespace WeaverCore.Game.Patches
{
	static class AudioPatchTest
	{
		static void AudioManager_ApplyMusicSnapshot(On.AudioManager.orig_ApplyMusicSnapshot orig, AudioManager self, UnityEngine.Audio.AudioMixerSnapshot snapshot, float delayTime, float transitionTime)
		{
			//WeaverLog.Log("Music Snapshot Changed");
			orig(self,snapshot,delayTime,transitionTime);
		}

		static void AudioManager_ApplyMusicCue(On.AudioManager.orig_ApplyMusicCue orig, AudioManager self, MusicCue musicCue, float delayTime, float transitionTime, bool applySnapshot)
		{
			//WeaverLog.Log("Music Cue Applied");
			orig(self,musicCue,delayTime,transitionTime,applySnapshot);
		}

		[OnInit]
		static void Init()
		{
			On.AudioManager.ApplyMusicCue += AudioManager_ApplyMusicCue;
			On.AudioManager.ApplyMusicSnapshot += AudioManager_ApplyMusicSnapshot;
		}
	}
}
