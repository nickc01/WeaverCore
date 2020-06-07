using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Game.Patches;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	public class G_WeaverAudio_I : WeaverAudio_I
	{
		public override AudioMixer MainMixer => AllMixers.Mixers.Master.audioMixer;

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
		}

		public override WeaverAudioPlayer Play(AudioClip clip, Vector3 position, float volume, AudioChannel channel, bool autoPlay, bool deleteWhenDone)
		{
			GameObject audioObject = new GameObject("__AUDIO_OBJECT__", typeof(AudioSource), typeof(WeaverAudioPlayer));

			return PlayReuse(audioObject.GetComponent<WeaverAudioPlayer>(), clip, position, volume, channel, autoPlay, deleteWhenDone);
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
					audioObject.AudioSource.outputAudioMixerGroup = AllMixers.Mixers.Master;
					break;
				case AudioChannel.Sound:
					audioObject.AudioSource.outputAudioMixerGroup = AllMixers.Mixers.Actors;
					break;
				case AudioChannel.Music:
					audioObject.AudioSource.outputAudioMixerGroup = AllMixers.Mixers.Main;
					break;
			}
		}

		public override AudioChannel GetObjectChannel(WeaverAudioPlayer audioObject)
		{
			if (audioObject.AudioSource.outputAudioMixerGroup == AllMixers.Mixers.Master)
			{
				return AudioChannel.Master;
			}
			else if (audioObject.AudioSource.outputAudioMixerGroup == AllMixers.Mixers.Actors)
			{
				return AudioChannel.Sound;
			}
			else if (audioObject.AudioSource.outputAudioMixerGroup == AllMixers.Mixers.Main)
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
