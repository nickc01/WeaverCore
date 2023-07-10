using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
	/// <summary>
	/// Used for playing sprite animations
	/// </summary>
	public class WeaverAnimationPlayer : MonoBehaviour
	{
		Action<string> onAnimationDone = null;

		[SerializeField]
		[Tooltip("The sprite animation data this player will be using")]
		WeaverAnimationData animationData;
		[NonSerialized]
		WeaverAnimationData oldData;

		[SerializeField]
		[Tooltip("Should an animation be played when the object starts?")]
		bool autoPlay = false;

		[Tooltip("Determines the behaviour that occurs when the player is done playing a clip")]
		public OnDoneBehaviour OnClipFinish;

		[SerializeField]
		[Tooltip("The clip played when the object starts up. Used only if \"Auto Play\" is enabled")]
		string autoPlayClip = "";

		int currentFrame = -1;
		float timer = 0f;
		float frameTime = 0f;
		bool forceOnce = false;

		/// <summary>
		/// The current frame index that is playing
		/// </summary>
		public int PlayingFrame
		{
			get
			{
				if (PlayingClip == null)
				{
					return 0;
				}
				else
				{
					return currentFrame;
				}
			}
		}

		/// <summary>
		/// When a clip is playing, this is how long the current frame has been displayed for
		/// </summary>
		public float FrameTime
		{
			get
			{
				if (PlayingClip == null)
				{
					return 0;
				}
				else
				{
					return timer;
				}
			}
		}


		SpriteRenderer _spriteRenderer;
		public SpriteRenderer SpriteRenderer
		{
			get
			{
				if (_spriteRenderer == null)
				{
					_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
					if (_spriteRenderer == null)
					{
						throw new Exception("No SpriteRenderer could be found on the object " + gameObject.name);
					}
				}
				return _spriteRenderer;
			}
		}

		/// <summary>
		/// The sprite animation data this player will be using
		/// </summary>
		public WeaverAnimationData AnimationData
		{
			get { return animationData; }
			set
			{
				if (animationData != value)
				{
					animationData = value;
					OnAnimationDataUpdate();
				}
			}
		}

		float _playBackSpeed = 1f;

		/// <summary>
		/// How fast the animations will be played at
		/// </summary>
		public float PlaybackSpeed
		{
			get { return _playBackSpeed; }
			set { _playBackSpeed = value; }
		}

		/// <summary>
		/// The currently playing clip. Returns null if no clip is playing
		/// </summary>
		public string PlayingClip { get; private set; }

		/// <summary>
		/// Each time a new animation is played, this value gets regenerated. This is used to differentiate between different playing animations
		/// </summary>
		public Guid PlayingGUID { get; private set; }

		protected virtual void OnEnable()
		{
			if (autoPlay && AnimationData.HasClip(autoPlayClip))
			{
				PlayAnimation(autoPlayClip);

				Action<string> onDone = null;

				onDone = s =>
				{
					onAnimationDone -= onDone;
					OnClipFinish.DoneWithObject(this);
				};

				onAnimationDone += onDone;
			}
		}

		protected virtual void OnValidate()
		{
			if (_spriteRenderer == null)
			{
				_spriteRenderer = GetComponent<SpriteRenderer>();
			}
			if (Application.isPlaying && animationData != oldData)
			{
				OnAnimationDataUpdate();
			}
		}

		/// <summary>
		/// If an animation is currently being played, this will stop it
		/// </summary>
		public void StopCurrentAnimation()
        {
            if (PlayingGUID != default)
            {
				currentFrame = -1;
				PlayingGUID = default(Guid);
				string originalClip = PlayingClip;
				PlayingClip = null;
				if (onAnimationDone != null)
				{
					onAnimationDone(originalClip);
					onAnimationDone = null;
				}
			}
        }

		protected virtual void Update()
		{
			if (currentFrame != -1)
			{
				timer += Time.deltaTime * PlaybackSpeed;
				while (currentFrame != -1 && timer >= frameTime)
				{
					timer -= frameTime;
					if (forceOnce)
					{
						currentFrame = AnimationData.GoToNextFrame(PlayingClip, currentFrame, WeaverAnimationData.WrapMode.Once);
					}
					else
					{
						currentFrame = AnimationData.GoToNextFrame(PlayingClip, currentFrame);
					}
					if (currentFrame == -1)
					{
						PlayingGUID = default(Guid);
						string originalClip = PlayingClip;
						PlayingClip = null;
						if (onAnimationDone != null)
						{
							onAnimationDone(originalClip);
							onAnimationDone = null;
						}
					}
					else
					{
						SpriteRenderer.sprite = AnimationData.GetFrameFromClip(PlayingClip, currentFrame);
						OnPlayingFrame(currentFrame);
					}
				}
			}
		}

		void OnAnimationDataUpdate()
		{
			oldData = animationData;
		}

		/// <summary>
		/// Does the <see cref="AnimationData"/> have a clip with the specified name?
		/// </summary>
		/// <param name="clipName">The clip name to check for</param>
		public bool HasAnimationClip(string clipName)
		{
			if (AnimationData == null)
			{
				throw new Exception("The Animation Data is not set");
			}
			return AnimationData.HasClip(clipName);
		}

		/// <summary>
		/// Plays an animation clip with the specified name
		/// </summary>
		/// <param name="clipName">The name of the clip to be played</param>
		/// <param name="forceOnce">If the clip is set to loop, setting this to true will force it to play once</param>
		/// <exception cref="Exception">Throws if the clip doesn't exist in <see cref="AnimationData"/></exception>
		public void PlayAnimation(string clipName, bool forceOnce = false)
		{
			//Debug.Log($"PLAYING ANIMATION {clipName} on object {gameObject.name}");
			this.forceOnce = forceOnce;
			if (!HasAnimationClip(clipName))
			{
				throw new Exception("The clip " + clipName + " does not exist in the animation data");
			}
			PlayingGUID = Guid.NewGuid();
			PlayingClip = clipName;
			timer = 0f;
			frameTime = 1f / AnimationData.GetClipFPS(clipName);
			OnPlayingAnimation(clipName);
			if (forceOnce)
			{
				currentFrame = AnimationData.GoToNextFrame(clipName, currentFrame, WeaverAnimationData.WrapMode.Once);
			}
			else
			{
				currentFrame = AnimationData.GetStartingFrame(clipName);
			}
			if (currentFrame == -1)
			{
				PlayingGUID = default(Guid);
				string originalClip = PlayingClip;
				PlayingClip = null;
				if (onAnimationDone != null)
				{
					onAnimationDone(originalClip);
					onAnimationDone = null;
				}
			}
			else
			{
				SpriteRenderer.sprite = AnimationData.GetFrameFromClip(clipName, currentFrame);
				OnPlayingFrame(currentFrame);
			}
		}

		/// <summary>
		/// Plays an animation clip with the specified name
		/// </summary>
		/// <param name="clipName">The name of the clip to be played</param>
		/// <param name="OnDone">An action that is executed when the animation is done playing</param>
		/// <param name="forceOnce">If the clip is set to loop, setting this to true will force it to play once</param>
		public void PlayAnimation(string clipName, Action<string> OnDone, bool forceOnce = false)
		{
			onAnimationDone = OnDone;
			PlayAnimation(clipName,forceOnce);
		}

		/// <summary>
		/// Plays an animation clip with the specified name
		/// </summary>
		/// <param name="clipName">The name of the clip to be played</param>
		/// <param name="OnDone">An action that is executed when the animation is done playing</param>
		/// <param name="forceOnce">If the clip is set to loop, setting this to true will force it to play once</param>
		public void PlayAnimation(string clipName, Action OnDone, bool forceOnce = false)
		{
			onAnimationDone = s => OnDone();
			PlayAnimation(clipName,forceOnce);
		}

		/// <summary>
		/// Plays an animation clip with the specified name, and waits until it is done
		/// </summary>
		/// <param name="clipName">The name of the clip to be played</param>
		/// <param name="forceOnce">If the clip is set to loop, setting this to true will force it to play once</param>
		public IEnumerator PlayAnimationTillDone(string clipName, bool forceOnce = false)
		{
			PlayAnimation(clipName,forceOnce);

			if (PlayingGUID == default(Guid))
			{
				yield break;
			}
			else
			{
				var currentGUID = PlayingGUID;
				while (currentGUID == PlayingGUID)
				{
					yield return null;
				}
			}
		}

		/// <summary>
		/// If a clip is currently playing, this will wait until the clip is done
		/// </summary>
		public IEnumerator WaitforClipToFinish()
		{
			var guid = PlayingGUID;
			if (guid == default(Guid))
			{
				yield break;
			}
			else
			{
				while (guid == PlayingGUID)
				{
					yield return null;
				}
			}
		}

		/// <summary>
		/// Called when a new animation is being played
		/// </summary>
		/// <param name="clip">The new clip being played</param>
		protected virtual void OnPlayingAnimation(string clip)
		{

		}

		/// <summary>
		/// Called when a new frame is being displayed
		/// </summary>
		/// <param name="frame">The frame index in the current clip being displayed</param>
		protected virtual void OnPlayingFrame(int frame)
		{

		}
	}
}
