﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using WeaverCore.Enums;
using WeaverCore.Attributes;
using WeaverCore.Components;
using System.Collections;

namespace WeaverCore
{
    /// <summary>
    /// Used for playing audio under certain channels.
    /// </summary>
    public static class WeaverAudio
	{
		[OnRegistryLoad]
		static void OnRuntimeStart(Registry registry)
		{
			if (registry.ModType == typeof(WeaverCore.Internal.WeaverCore_ModClass))
			{
				if (baseObject == null)
				{
					baseObject = WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Audio Player Prefab").GetComponent<AudioPlayer>();
				}
			}
		}

		static AudioPlayer baseObject;

		static WeaverAudio_I Impl = ImplFinder.GetImplementation<WeaverAudio_I>();

		/// <summary>
		/// Creates an audio player that can be used to play an audio clip under a certain audio channel. Use the <see cref="AudioPlayer.Play(bool)"/> function to play the sound
		/// </summary>
		/// <param name="clip">The clip to play</param>
		/// <param name="position">Where in the scene should the sound be played at</param>
		/// <param name="volume">The volme of the audio clip</param>
		/// <param name="channel">The audio channel the sound should be played under</param>
		/// <returns>Returns a new audio player for playing the audio clip</returns>
		public static AudioPlayer Create(AudioClip clip, Vector3 position, float volume = 1.0f, AudioChannel channel = AudioChannel.Sound)
		{
#if UNITY_EDITOR
			if (baseObject == null)
			{
                baseObject = WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Audio Player Prefab").GetComponent<AudioPlayer>();
            }
#endif
			var audioObject = Pooling.Instantiate(baseObject, position, Quaternion.identity);
			var audioSource = audioObject.AudioSource;
			audioObject.Channel = channel;
			audioSource.playOnAwake = false;
			audioSource.Stop();
			audioSource.clip = clip;
			audioSource.pitch = 1f;
			audioSource.loop = false;
			audioObject.gameObject.name = "(Sound) " + clip.name;
			audioSource.volume = volume;

			return audioObject;
		}

		/// <summary>
		/// Plays a clip at the specified point. Automatically deletes itself when the clip is done playing. For more control, refer to <see cref="Create(AudioClip, Vector3, float, AudioChannel)"/>
		/// </summary>
		/// <param name="clip">The clip to play</param>
		/// <param name="position">The position to play at</param>
		/// <param name="volume">How loud the player will be</param>
		/// <param name="channel">What audio channel the player will be playing under</param>
		/// <returns>The player that is playing the audio</returns>
		public static AudioPlayer PlayAtPoint(AudioClip clip, Vector3 position, float volume = 1.0f, AudioChannel channel = AudioChannel.Sound)
		{
			var audioObject = Create(clip, position, volume, channel);
			if (clip != null)
			{
				audioObject.AudioSource.Play();
				audioObject.Delete(() => !audioObject.AudioSource.isPlaying);
				//audioObject.Delete(clip.length);
			}

			return audioObject;
		}

        /// <summary>
        /// Plays a clip at the specified point. Automatically deletes itself when the clip is done playing. For more control, refer to <see cref="Create(AudioClip, Vector3, float, AudioChannel)"/>
        /// </summary>
        /// <param name="clip">The clip to play</param>
        /// <param name="position">The position to play at</param>
        /// <param name="volume">How loud the player will be</param>
        /// <param name="channel">What audio channel the player will be playing under</param>
        /// <returns>The player that is playing the audio</returns>
        public static AudioPlayer PlayAtPointDelayed(float delay, AudioClip clip, Vector3 position, float volume = 1.0f, AudioChannel channel = AudioChannel.Sound)
        {
            var audioObject = Create(clip, position, volume, channel);
            if (clip != null)
            {
				audioObject.AudioSource.PlayDelayed(delay);
				var time = Time.time;
                audioObject.Delete(() => Time.time >= time + delay && !audioObject.AudioSource.isPlaying);
                //audioObject.Delete(delay + clip.length);
            }

            return audioObject;
        }

        /// <summary>
        /// Plays a clip forever untill it's manually destroyed
        /// </summary>
        /// <param name="clip">The clip to play</param>
        /// <param name="position">The position to play at</param>
        /// <param name="volume">How loud the player will be</param>
        /// <param name="channel">What audio channel the player will be playing under</param>
        /// <returns>The player that is playing the audio</returns>
        public static AudioPlayer PlayAtPointLooped(AudioClip clip, Vector3 position, float volume = 1.0f, AudioChannel channel = AudioChannel.Sound)
		{
			var audioObject = Create(clip, position, volume, channel);
			audioObject.AudioSource.loop = true;
			audioObject.AudioSource.Play();
			return audioObject;
		}

		/// <summary>
		/// Gets the main audio mixer that hollow knight uses
		/// </summary>
		public static AudioMixer MainMixer
		{
			get
			{
				return Impl.MainMixer;
			}
		}

		/// <summary>
		/// Gets the "Music" audio group for music related audio
		/// </summary>
		public static AudioMixerGroup MusicMixerGroup
		{
			get
			{
				return Impl.MainMusic;
			}
		}

		/// <summary>
		/// Gets the "Master" audio group
		/// </summary>
		public static AudioMixerGroup MasterMixerGroup
		{
			get
			{
				return Impl.Master;
			}
		}

		/// <summary>
		/// Gets the "Sounds" audio mixer for playing game sounds
		/// </summary>
		public static AudioMixerGroup SoundsMixerGroup
		{
			get
			{
				return Impl.Sounds;
			}
		}

		/// <summary>
		/// Gets the mixer group for a certain channel
		/// </summary>
		/// <param name="channel">The channel to get the mixer group for</param>
		public static AudioMixerGroup GetMixerForChannel(AudioChannel channel)
		{
			return Impl.GetMixerForChannel(channel);
		}

        public static void AddVolumeDistanceControl(AudioPlayer audio, Vector2 volumeRange, float baseVolume = -1)
		{
			AddVolumeDistanceControl(audio, Player.Player1.transform, volumeRange, baseVolume);
		}

		public static void AddVolumeDistanceControl(IEnumerable<AudioPlayer> audios, Vector2 volumeRange)
		{
			if (audios == null)
			{
				return;
			}
			foreach (var audio in audios)
			{
				AddVolumeDistanceControl(audio, volumeRange);
			}
			//AddVolumeDistanceControl(audio, Player.Player1.transform, volumeRange, baseVolume);
		}


        public static void AddVolumeDistanceControl(AudioPlayer audio, Transform target, Vector2 volumeRange, float baseVolume = -1)
		{
			if (baseVolume < 0)
			{
				baseVolume = audio.AudioSource.volume;
			}

			if (volumeRange.x > volumeRange.y)
			{
				var temp = volumeRange.x;
                volumeRange.x = volumeRange.y;
                volumeRange.y = temp;
			}

			if (audio != null && !audio.IsInPool && audio.AudioSource != null && audio.isActiveAndEnabled)
			{
                var distance = Vector2.Distance(target.position, audio.AudioSource.transform.position);

				audio.AudioSource.volume = (1f - Mathf.InverseLerp(volumeRange.x, volumeRange.y, distance)) * baseVolume;

                UnboundCoroutine.Start(DistanceVolumeControlRoutine(audio, target, volumeRange, baseVolume));
			}
		}

		static IEnumerator DistanceVolumeControlRoutine(AudioPlayer audio, Transform target, Vector2 volumeRange, float baseVolume)
		{
			while (true)
			{
				if (audio == null || !audio.isActiveAndEnabled || audio.IsInPool || audio.AudioSource == null)
				{
					break;
				}

				var distance = Vector2.Distance(target.position, audio.AudioSource.transform.position);

				audio.AudioSource.volume = (1f - Mathf.InverseLerp(volumeRange.x, volumeRange.y, distance)) * baseVolume;

				yield return null;
			}

			//WeaverLog.Log("DISTANCE STUFF DONE");
		}

		public static float MasterVolume => Impl.MasterVolume;
		public static float MusicVolume => Impl.MusicVolume;
		public static float SoundVolume => Impl.SoundsVolume;

		public static event Action<float> OnMasterVolumeUpdate
		{
			add => Impl.OnMasterVolumeUpdate += value;
			remove => Impl.OnMasterVolumeUpdate -= value;
		}

		public static event Action<float> OnMusicVolumeUpdate
        {
            add => Impl.OnMusicVolumeUpdate += value;
            remove => Impl.OnMusicVolumeUpdate -= value;
        }

        public static event Action<float> OnSoundVolumeUpdate
        {
            add => Impl.OnSoundVolumeUpdate += value;
            remove => Impl.OnSoundVolumeUpdate -= value;
        }

		public static event Action<bool> OnPauseStateUpdate
		{
            add => Impl.OnPauseStateUpdate += value;
            remove => Impl.OnPauseStateUpdate -= value;
        }

		public static List<AudioPlayer> PlayAudioGroup(bool play, Transform transform, IEnumerable<AudioClip> clips, Vector2 pitchRange, float volume) 
        {
            if (!play)
            {
                return null;
            }
            List<AudioPlayer> instances = new List<AudioPlayer>();
            foreach (var clip in clips)
            {
                var instance = WeaverAudio.PlayAtPoint(clip, transform.position, volume);
                instance.AudioSource.pitch = pitchRange.RandomInRange();
                instances.Add(instance);
            }

            return instances;
        }

        public static List<AudioPlayer> PlayAudioGroupLooped(bool play, Transform transform, IEnumerable<AudioClip> clips, Vector2 pitchRange, float volume) 
        {
            if (!play)
            {
                return null;
            }
            List<AudioPlayer> instances = new List<AudioPlayer>();
            foreach (var clip in clips)
            {
                var instance = WeaverAudio.PlayAtPointLooped(clip, transform.position, volume);
                instance.AudioSource.pitch = pitchRange.RandomInRange();
                instances.Add(instance);
            }

            return instances;
        }

		public static void StopAudioGroup(ref List<AudioPlayer> players)
        {
            if (players != null)
            {
                foreach (var instance in players)
                {
                    instance.Delete();
                }
                players = null;
            }
        }
    }
}
