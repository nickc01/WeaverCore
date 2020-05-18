using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Helpers;

namespace WeaverCore.Game.Patches
{
	/*public class AudioHunter : IPatch
	{
		public static List<AudioMixerGroup> MixerGroups = new List<AudioMixerGroup>();

		class test
		{
			public AudioMixerGroup SurfaceWind1;
			public AudioMixerGroup Shade;
			public AudioMixerGroup RainIndoor;
			public AudioMixerGroup MinesMachinery;
			public AudioMixerGroup Extra;
			public AudioMixerGroup Fungus;
			public AudioMixerGroup Master;
			public AudioMixerGroup RainOutdoor;
			public AudioMixerGroup WaterfallMed;
			public AudioMixerGroup CaveWind;
			public AudioMixerGroup SurfaceWind2;
			public AudioMixerGroup Greenpath;
			public AudioMixerGroup UI;
			public AudioMixerGroup GrassyWind;
			public AudioMixerGroup Tension;
			public AudioMixerGroup Actors;
			public AudioMixerGroup MinesCrystal;
			public AudioMixerGroup Sub;
			public AudioMixerGroup Main;
			public AudioMixerGroup Deepnest;
			public AudioMixerGroup MainAlt;
			public AudioMixerGroup Waterways;
			public AudioMixerGroup Action;
			public AudioMixerGroup MiscWind;
			public AudioMixerGroup FogCanyon;
			public AudioMixerGroup CaveNoises;
		}



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

			try
			{
				var test = new test();

				test.SurfaceWind1 = MixerGroups.First(m => m.name == "Surface Wind 1");
				Debugger.Log("A");
				test.Shade = MixerGroups.First(m => m.name == "Shade");
				Debugger.Log("B");
				test.RainIndoor = MixerGroups.First(m => m.name == "Rain Indoor");
				Debugger.Log("C");
				test.MinesMachinery = MixerGroups.First(m => m.name == "Mines Machinery");
				Debugger.Log("D");
				test.Extra = MixerGroups.First(m => m.name == "Extra");
				Debugger.Log("E");
				test.Fungus = MixerGroups.First(m => m.name == "Fungus");
				Debugger.Log("F");
				test.Master = MixerGroups.First(m => m.name == "Master");
				Debugger.Log("G");
				test.RainOutdoor = MixerGroups.First(m => m.name == "Rain Outdoor");
				Debugger.Log("H");
				test.WaterfallMed = MixerGroups.First(m => m.name == "Waterfall Med");
				Debugger.Log("I");
				test.CaveWind = MixerGroups.First(m => m.name == "Cave Wind");
				Debugger.Log("J");
				test.SurfaceWind2 = MixerGroups.First(m => m.name == "Surface Wind 2");
				Debugger.Log("K");
				test.Greenpath = MixerGroups.First(m => m.name == "Greenpath");
				Debugger.Log("L");
				test.UI = MixerGroups.First(m => m.name == "UI");
				Debugger.Log("M");
				test.GrassyWind = MixerGroups.First(m => m.name == "Grassy Wind");
				Debugger.Log("N");
				test.Tension = MixerGroups.First(m => m.name == "Tension");
				Debugger.Log("O");
				test.Actors = MixerGroups.First(m => m.name == "Actors");
				Debugger.Log("P");
				test.MinesCrystal = MixerGroups.First(m => m.name == "Mines Crystal");
				Debugger.Log("Q");
				test.Sub = MixerGroups.First(m => m.name == "Sub");
				Debugger.Log("R");
				test.Main = MixerGroups.First(m => m.name == "Main");
				Debugger.Log("S");
				test.Deepnest = MixerGroups.First(m => m.name == "Deepnest");
				Debugger.Log("T");
				test.MainAlt = MixerGroups.First(m => m.name == "Main Alt");
				Debugger.Log("U");
				test.Waterways = MixerGroups.First(m => m.name == "Waterways");
				Debugger.Log("V");
				test.Action = MixerGroups.First(m => m.name == "Action");
				Debugger.Log("W");
				test.MiscWind = MixerGroups.First(m => m.name == "Misc Wind");
				Debugger.Log("X");
				test.FogCanyon = MixerGroups.First(m => m.name == "Fog Canyon");
				Debugger.Log("Y");
				test.CaveNoises = MixerGroups.First(m => m.name == "Cave Noises");
				Debugger.Log("Z");
				Debugger.Log("Mixer Serialized = " + JsonUtility.ToJson(test));
			}
			catch (Exception e)
			{
				Debugger.Log("Error = " + e);
			}
		}


	}*/
}
