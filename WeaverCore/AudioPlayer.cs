using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;

namespace WeaverCore
{
    public sealed class AudioPlayer : MonoBehaviour, IOnPool
	{
		[SerializeField]
		AudioChannel channel;

		//public ObjectPool SourcePool { get; internal set; }

		[HideInInspector]
		[SerializeField]
		private PoolableObject poolComponent;

		[HideInInspector]
		[SerializeField]
		private AudioSource audioSource;
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

		public float Volume
		{
			get { return AudioSource.volume; }
			set { AudioSource.volume = value; }
		}

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

		public void Play(bool deleteWhenDone = false)
		{
			PlayDelayed(0f, deleteWhenDone);
		}

		public void Play(AudioClip clip, float volume = 1f, AudioChannel channel = AudioChannel.Sound, bool deleteWhenDone = false)
		{
			Clip = clip;
			Volume = volume;
			Channel = channel;
			Play(deleteWhenDone: deleteWhenDone);
		}

		public void StopPlaying()
		{
			audioSource.Stop();
		}

		/*public static AudioPlayer Create(AudioClip clip = null, Vector3 position = default(Vector3))
		{
			return Audio.Create(clip, position);
		}*/

		void IOnPool.OnPool()
		{
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
			//UpdateAudioMixer();
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
