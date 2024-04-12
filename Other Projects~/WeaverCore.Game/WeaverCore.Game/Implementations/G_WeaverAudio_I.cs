using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Attributes;
using WeaverCore.Enums;
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

		public static event Action<float> Internal_OnMasterVolumeUpdate;
		public static event Action<float> Internal_OnMusicVolumeUpdate;
		public static event Action<float> Internal_OnSoundVolumeUpdate;
		public static event Action<bool> Internal_OnPauseStateUpdate;

        public override event Action<float> OnMasterVolumeUpdate
		{
			add => Internal_OnMasterVolumeUpdate += value;
			remove => Internal_OnMasterVolumeUpdate -= value;
		}

        public override event Action<float> OnMusicVolumeUpdate
        {
            add => Internal_OnMusicVolumeUpdate += value;
            remove => Internal_OnMusicVolumeUpdate -= value;
        }

        public override event Action<float> OnSoundVolumeUpdate
        {
            add => Internal_OnSoundVolumeUpdate += value;
            remove => Internal_OnSoundVolumeUpdate -= value;
        }

        public override event Action<bool> OnPauseStateUpdate
        {
            add => Internal_OnPauseStateUpdate += value;
            remove => Internal_OnPauseStateUpdate -= value;
        }

        public override AudioMixerGroup Sounds => _sounds;

		public override float MasterVolume => GameManager.instance.gameSettings.masterVolume / 10f;

        public override float MusicVolume => GameManager.instance.gameSettings.musicVolume / 10f;

        public override float SoundsVolume => GameManager.instance.gameSettings.soundVolume / 10f;

        [OnInit]
		static void Init()
		{
			UnboundCoroutine.Start(ScanForMixers());
            On.MenuAudioSlider.SetMasterLevel += MenuAudioSlider_SetMasterLevel;
            On.MenuAudioSlider.SetMusicLevel += MenuAudioSlider_SetMusicLevel;
            On.MenuAudioSlider.SetSoundLevel += MenuAudioSlider_SetSoundLevel;
            On.GameManager.SetState += GameManager_SetState;
		}

        private static void GameManager_SetState(On.GameManager.orig_SetState orig, GameManager self, GlobalEnums.GameState newState)
        {
			orig(self, newState);
			Internal_OnPauseStateUpdate?.Invoke(newState == GlobalEnums.GameState.PAUSED);
        }

        private static void MenuAudioSlider_SetSoundLevel(On.MenuAudioSlider.orig_SetSoundLevel orig, MenuAudioSlider self, float soundLevel)
        {
            orig(self, soundLevel);
            Internal_OnSoundVolumeUpdate?.Invoke(soundLevel / 10f);
        }

        private static void MenuAudioSlider_SetMusicLevel(On.MenuAudioSlider.orig_SetMusicLevel orig, MenuAudioSlider self, float musicLevel)
        {
            orig(self, musicLevel);
            Internal_OnMusicVolumeUpdate?.Invoke(musicLevel / 10f);
        }

        private static void MenuAudioSlider_SetMasterLevel(On.MenuAudioSlider.orig_SetMasterLevel orig, MenuAudioSlider self, float masterLevel)
        {
			orig(self, masterLevel);
            Internal_OnMasterVolumeUpdate?.Invoke(masterLevel / 10f);
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

		public override AudioMixerGroup GetMixerForChannel(AudioChannel channel)
		{
			switch (channel)
			{
				case AudioChannel.Master:
					return Master;
				case AudioChannel.Sound:
					return Sounds;
				case AudioChannel.Music:
					return MainMusic;
				default:
					return null;
			}
		}
	}
}
