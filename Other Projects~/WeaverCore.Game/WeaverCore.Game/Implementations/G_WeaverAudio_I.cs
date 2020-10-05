using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Game.Patches;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
	public class G_WeaverAudio_I : WeaverAudio_I
	{
		static List<AudioMixerGroup> MixerGroups = new List<AudioMixerGroup>();

		public override AudioMixer MainMixer => _master.audioMixer;

		static AudioMixerGroup _music;
		public override AudioMixerGroup MainMusic => _music;

		static AudioMixerGroup _master;
		public override AudioMixerGroup Master => _master;

		static AudioMixerGroup _sounds;
		public override AudioMixerGroup Sounds => _sounds;

		[OnInit]
		static void Init()
		{
			UnboundCoroutine.Start(ScanForMixers());
		}

		static IEnumerator ScanForMixers()
		{
			yield return null;
			foreach (var audioSource in GameObject.FindObjectsOfType<AudioSource>())
			{
				if (audioSource.outputAudioMixerGroup != null && !MixerGroups.Contains(audioSource.outputAudioMixerGroup))
				{
					MixerGroups.Add(audioSource.outputAudioMixerGroup);
				}
			}

			if (_master == null)
			{
				_master = MixerGroups.FirstOrDefault(mixer => mixer.name == "Master");
			}
			if (_sounds == null)
			{
				_sounds = MixerGroups.FirstOrDefault(mixer => mixer.name == "Actors");
			}
			if (_music == null)
			{
				_music = MixerGroups.FirstOrDefault(mixer => mixer.name == "Main");
			}
		}

		/*class AudioHunter : IInit
		{
			public static List<AudioMixerGroup> MixerGroups = new List<AudioMixerGroup>();
			public static AudioMixerGroup Master;
			public static AudioMixerGroup Sound;
			public static AudioMixerGroup Music;


			public void Apply()
			{

			}

			IEnumerator FirstRun()
			{
				yield return null;
				ScanForMixers();
			}

			void IInit.OnInit()
			{
				UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnNewScene;
				UnboundCoroutine.Start(FirstRun());
			}

			private void OnNewScene(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
			{
				//ScanForMixers();
			}


			IEnumerator ScanForMixers()
			{
				yield return null;
				foreach (var audioSource in GameObject.FindObjectsOfType<AudioSource>())
				{
					if (audioSource.outputAudioMixerGroup != null && !MixerGroups.Contains(audioSource.outputAudioMixerGroup))
					{
						MixerGroups.Add(audioSource.outputAudioMixerGroup);
					}
				}

				if (Master == null)
				{
					Master = MixerGroups.FirstOrDefault(mixer => mixer.name == "Master");
				}
				if (Sound == null)
				{
					Sound = MixerGroups.FirstOrDefault(mixer => mixer.name == "Actors");
				}
				if (Music == null)
				{
					Music = MixerGroups.FirstOrDefault(mixer => mixer.name == "Main");
				}
			}


		}*/

		/*[Serializable]
		class AllMixers
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

			private static AllMixers mixers;
			public static AllMixers Mixers
			{
				get
				{
					if (mixers == null)
					{
						mixers = JsonUtility.FromJson<AllMixers>(@"{""SurfaceWind1"":{""m_FileID"":13792,""m_PathID"":0},""Shade"":{""m_FileID"":17862,""m_PathID"":0},""RainIndoor"":{""m_FileID"":13796,""m_PathID"":0},""MinesMachinery"":{""m_FileID"":13806,""m_PathID"":0},""Extra"":{""m_FileID"":15974,""m_PathID"":0},""Fungus"":{""m_FileID"":13786,""m_PathID"":0},""Master"":{""m_FileID"":18448,""m_PathID"":0},""RainOutdoor"":{""m_FileID"":13798,""m_PathID"":0},""WaterfallMed"":{""m_FileID"":13802,""m_PathID"":0},""CaveWind"":{""m_FileID"":13780,""m_PathID"":0},""SurfaceWind2"":{""m_FileID"":13794,""m_PathID"":0},""Greenpath"":{""m_FileID"":13790,""m_PathID"":0},""UI"":{""m_FileID"":17698,""m_PathID"":0},""GrassyWind"":{""m_FileID"":13788,""m_PathID"":0},""Tension"":{""m_FileID"":15982,""m_PathID"":0},""Actors"":{""m_FileID"":7482,""m_PathID"":0},""MinesCrystal"":{""m_FileID"":13804,""m_PathID"":0},""Sub"":{""m_FileID"":15980,""m_PathID"":0},""Main"":{""m_FileID"":15976,""m_PathID"":0},""Deepnest"":{""m_FileID"":13782,""m_PathID"":0},""MainAlt"":{""m_FileID"":15978,""m_PathID"":0},""Waterways"":{""m_FileID"":13800,""m_PathID"":0},""Action"":{""m_FileID"":15972,""m_PathID"":0},""MiscWind"":{""m_FileID"":13808,""m_PathID"":0},""FogCanyon"":{""m_FileID"":13784,""m_PathID"":0},""CaveNoises"":{""m_FileID"":13778,""m_PathID"":0}}");
					}
					return mixers;
				}
			}
		}*/

		public override WeaverAudioPlayer Play(AudioClip clip, Vector3 position, float volume, AudioChannel channel, bool autoPlay, bool deleteWhenDone)
		{
			//GameObject audioObject = new GameObject("__AUDIO_OBJECT__", typeof(AudioSource), typeof(WeaverAudioPlayer));
			var audioObject = WeaverAudioPlayer.Create(position);

			return PlayReuse(audioObject, clip, position, volume, channel, autoPlay, deleteWhenDone);
		}

		public override WeaverAudioPlayer PlayReuse(WeaverAudioPlayer audioObject, AudioClip clip, Vector3 position, float volume, AudioChannel channel, bool autoPlay, bool deleteWhenDone)
		{
			var audioSource = audioObject.AudioSource;

			audioSource.Stop();
			audioSource.clip = clip;
			audioObject.transform.position = position;
			audioSource.volume = volume;

			SetObjectChannel(audioObject, channel);

			if (autoPlay)
			{
				audioSource.Play();
			}

			if (deleteWhenDone)
			{
				audioObject.Delete(clip.length);
			}

			return audioObject;
		}

		public override void SetObjectChannel(WeaverAudioPlayer audioObject, AudioChannel channel)
		{
			switch (channel)
			{
				case AudioChannel.Master:
					//WeaverLog.Log("Master = " + AudioHunter.Master);
					audioObject.AudioSource.outputAudioMixerGroup = Master;
					break;
				case AudioChannel.Sound:
					//WeaverLog.Log("Actors = " + AudioHunter.Sound);
					audioObject.AudioSource.outputAudioMixerGroup = Sounds;
					break;
				case AudioChannel.Music:
					//WeaverLog.Log("Music = " + AudioHunter.Music);
					audioObject.AudioSource.outputAudioMixerGroup = MainMusic;
					break;
			}
		}

		public override AudioChannel GetObjectChannel(WeaverAudioPlayer audioObject)
		{
			if (audioObject.AudioSource.outputAudioMixerGroup == Master)
			{
				return AudioChannel.Master;
			}
			else if (audioObject.AudioSource.outputAudioMixerGroup == Sounds)
			{
				return AudioChannel.Sound;
			}
			else if (audioObject.AudioSource.outputAudioMixerGroup == MainMusic)
			{
				return AudioChannel.Music;
			}
			else
			{
				return AudioChannel.None;
			}
		}
	}
}
