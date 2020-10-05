using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore
{
	public class WeaverAudioPlayer : MonoBehaviour, IPoolableObject
	{
		static ObjectPool<WeaverAudioPlayer> Pool;
		static WeaverAudioPlayer baseObject;


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
			get { return WeaverAudio.Impl.GetObjectChannel(this); }
			set { WeaverAudio.Impl.SetObjectChannel(this, value); }
		}

		public float Volume
		{
			get { return AudioSource.volume; }
			set { AudioSource.volume = value; }
		}

		public void Delete(float timer = 0f)
		{
			if (Pool == null)
			{
				Destroy(gameObject, timer);
			}
			else
			{
				Pool.ReturnToPool(this, timer);
			}
		}

		public void Play(bool deleteWhenDone = false)
		{
			if (Clip != null)
			{
				audioSource.Play();
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

		public void Play(AudioClip clip, float volume = 1f, AudioChannel channel = AudioChannel.Sound, bool deleteWhenDone = false)
		{
			Clip = clip;
			Volume = volume;
			Channel = channel;
			Play(deleteWhenDone);
		}

		public static WeaverAudioPlayer Create(Vector3 position = default(Vector3))
		{
			if (Pool == null || Pool.ObjectToPool == null || Pool.ObjectToPool.gameObject == null || Pool.Recycler == null || Pool.Recycler.gameObject == null)
			{
				GameObject newObj = new GameObject("__AUDIO_OBJECT_BASE__", typeof(AudioSource));
				if (baseObject == null)
				{
					baseObject = newObj.AddComponent<WeaverAudioPlayer>();
					GameObject.DontDestroyOnLoad(baseObject.gameObject);
				}
				Pool = ObjectPool<WeaverAudioPlayer>.CreatePool(baseObject, DataTypes.ObjectPoolStorageType.ActiveSceneOnly, 2, true);
			}
			return Pool.RetrieveFromPool(position,Quaternion.identity);
		}

		void IPoolableObject.OnPool()
		{
			AudioSource.Stop();
			AudioSource.pitch = 1f;
			AudioSource.mute = false;
			AudioSource.bypassEffects = false;
			AudioSource.bypassListenerEffects = false;
			AudioSource.playOnAwake = true;
			Clip = null;
			Volume = 1f;
			Channel = AudioChannel.None;
		}
	}
}
