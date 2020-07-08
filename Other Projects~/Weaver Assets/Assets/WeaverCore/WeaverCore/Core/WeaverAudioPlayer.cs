using UnityEngine;
using WeaverCore.Enums;

namespace WeaverCore
{
	public class WeaverAudioPlayer : MonoBehaviour
	{
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
			Destroy(gameObject, timer);
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
	}
}
