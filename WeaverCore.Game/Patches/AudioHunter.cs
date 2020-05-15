using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Helpers;

namespace WeaverCore.Game.Patches
{
	public class AudioHunter : IPatch
	{
		public static List<AudioMixerGroup> MixerGroups = new List<AudioMixerGroup>();


		public void Apply()
		{
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnNewScene;
			URoutine.Start(FirstRun());
		}

		IEnumerator<IUAwaiter> FirstRun()
		{
			yield return null;
			ScanForMixers();
		}

		private void OnNewScene(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
		{
			ScanForMixers();
		}


		void ScanForMixers()
		{
			foreach (var audioSource in GameObject.FindObjectsOfType<AudioSource>())
			{
				if (audioSource.outputAudioMixerGroup != null && !MixerGroups.Contains(audioSource.outputAudioMixerGroup))
				{
					MixerGroups.Add(audioSource.outputAudioMixerGroup);
				}
			}

			Debugger.Log("Mixer Start");
			foreach (var mixer in MixerGroups)
			{
				Debugger.Log("New Mixer = " + mixer);
			}
			Debugger.Log("Mixer End");
		}


	}
}
