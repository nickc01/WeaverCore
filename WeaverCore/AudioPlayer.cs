using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;

namespace WeaverCore
{
	/// <summary>
	/// Used for playing audio sounds. 
	/// 
	/// This class allows you to play sounds on specific channels (like the music channel, or the SFX channel), for greater sound control
	/// </summary>
    public sealed class AudioPlayer : MonoBehaviour, IOnPool
	{
		[SerializeField]
		[Tooltip("The audio channel the sound is to be played on")]
		AudioChannel channel;
		[HideInInspector]
		[SerializeField]
		private PoolableObject poolComponent;

		[HideInInspector]
		[SerializeField]
		private AudioSource audioSource;

		/// <summary>
		/// The audio source that is playing the sound
		/// </summary>
		public AudioSource AudioSource
		{
			get
			{
				if (audioSource == null)
				{
					audioSource = GetComponent<AudioSource>();
					if (audioSource == null)
					{
						audioSource = gameObject.AddComponent<AudioSource>();
					}
				}
				return audioSource;
			}
		}

		/// <summary>
		/// The clip that is currently being played
		/// </summary>
		public AudioClip Clip
		{
			get { return AudioSource.clip; }
			set
			{
				if (AudioSource.isPlaying)
				{
					AudioSource.Stop();
				}
				AudioSource.clip = value;
			}
		}

		/// <summary>
		/// The audio channel the sound is to be played on
		/// </summary>
		public AudioChannel Channel
		{
			get
			{
				return channel;
			}
			set
			{
				if (channel != value)
				{
					channel = value;
					UpdateAudioMixer();
				}
			}
		}

		/// <summary>
		/// The volume of the audio source
		/// </summary>
		public float Volume
		{
			get { return AudioSource.volume; }
			set { AudioSource.volume = value; }
		}

		/// <summary>
		/// Destroys the audio player. If this audio player was created from a pool, this function will return it to the pool
		/// </summary>
		/// <param name="timer"></param>
		public void Delete(float timer = 0f)
		{
			var SourcePool = GetComponent<PoolableObject>();
			if (SourcePool == null)
			{
				Destroy(gameObject, timer);
			}
			else
			{
				SourcePool.ReturnToPool(timer);
			}
		}

		/// <summary>
		/// Plays the Audio Source after a set delay
		/// </summary>
		/// <param name="delay">The delay before the sound is played</param>
		/// <param name="deleteWhenDone">Should this audio player be deleted when done?</param>
		public void PlayDelayed(float delay, bool deleteWhenDone = true)
		{
			if (Clip != null)
			{
				if (delay > 0f)
				{
					audioSource.PlayDelayed(delay);
				}
				else
				{
					audioSource.Play();
				}
			}
			if (deleteWhenDone)
			{
				if (Clip == null)
				{
					Delete();
				}
				else
				{
					Delete(Clip.length);
				}
			}
		}

		/// <summary>
		/// Plays the Audio Source
		/// </summary>
		/// <param name="deleteWhenDone">Should this audio player be deleted when done?</param>
		public void Play(bool deleteWhenDone = false)
		{
			PlayDelayed(0f, deleteWhenDone);
		}

		/// <summary>
		/// Plays an audio clip on this audio player
		/// </summary>
		/// <param name="clip">The clip to be played</param>
		/// <param name="volume">The volume of the clip</param>
		/// <param name="channel">What audio channel should the sound be played under?</param>
		/// <param name="deleteWhenDone">Should this audio player be deleted when done?</param>
		public void Play(AudioClip clip, float volume = 1f, AudioChannel channel = AudioChannel.Sound, bool deleteWhenDone = false)
		{
			Clip = clip;
			Volume = volume;
			Channel = channel;
			Play(deleteWhenDone: deleteWhenDone);
		}

		/// <summary>
		/// Stops playing the current Audio Source
		/// </summary>
		public void StopPlaying()
		{
			audioSource.Stop();
		}

		void IOnPool.OnPool()
		{
			transform.SetParent(null);
			AudioSource.Stop();
			AudioSource.pitch = 1f;
			AudioSource.mute = false;
			AudioSource.bypassEffects = false;
			AudioSource.bypassListenerEffects = false;
			AudioSource.playOnAwake = false;
			Clip = null;
			Volume = 1f;
			Channel = AudioChannel.Sound;
		}

		void Awake()
		{
			UpdateAudioMixer();
		}

		void OnValidate()
		{
			if (poolComponent == null)
			{
				poolComponent = GetComponent<PoolableObject>();
			}
		}

		private void UpdateAudioMixer()
		{
			if (AudioSource != null)
			{
				AudioSource.outputAudioMixerGroup = WeaverAudio.GetMixerForChannel(channel);
			}
			if (poolComponent == null)
			{
				poolComponent = GetComponent<PoolableObject>();
			}
		}
	}
}
