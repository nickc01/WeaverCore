using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class WeaverAnimationPlayer : MonoBehaviour
	{
		Action<string> onAnimationDone = null;


		[SerializeField]
		WeaverAnimationData animationData;
		[NonSerialized]
		WeaverAnimationData oldData;

		int currentFrame = -1;
		float timer = 0f;
		float frameTime = 0f;
		bool forceOnce = false;


		SpriteRenderer _spriteRenderer;
		public SpriteRenderer SpriteRenderer
		{
			get
			{
				if (_spriteRenderer == null)
				{
					_spriteRenderer = GetComponent<SpriteRenderer>();
				}
				return _spriteRenderer;
			}
		}

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

		public string PlayingClip { get; private set; }

		//Each time a new animation is played, this value gets regenerated. This is used to differentiate between different playing animations
		public Guid PlayingGUID { get; private set; }

		void OnValidate()
		{
			Guid.NewGuid();
			if (_spriteRenderer == null)
			{
				_spriteRenderer = GetComponent<SpriteRenderer>();
			}
			if (Application.isPlaying && animationData != oldData)
			{
				OnAnimationDataUpdate();
			}
		}

		void Update()
		{
			if (currentFrame != -1)
			{
				timer += Time.deltaTime;
				if (timer >= frameTime)
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
					}
				}
			}
		}

		void OnAnimationDataUpdate()
		{
			oldData = animationData;
		}

		public bool HasAnimationClip(string clipName)
		{
			if (AnimationData == null)
			{
				throw new Exception("The Animation Data is not set");
			}
			return AnimationData.HasClip(clipName);
		}

		public void PlayAnimation(string clipName, bool forceOnce = false)
		{
			this.forceOnce = forceOnce;
			if (!HasAnimationClip(clipName))
			{
				throw new Exception("The clip " + clipName + " does not exist in the animation data");
			}
			PlayingGUID = Guid.NewGuid();
			PlayingClip = clipName;
			timer = 0f;
			frameTime = 1f / AnimationData.GetClipFPS(clipName);
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
			}
		}

		public void PlayAnimation(string clipName, Action<string> OnDone, bool forceOnce = false)
		{
			onAnimationDone = OnDone;
			PlayAnimation(clipName,forceOnce);
		}

		public void PlayAnimation(string clipName, Action OnDone, bool forceOnce = false)
		{
			onAnimationDone = s => OnDone();
			PlayAnimation(clipName,forceOnce);
		}

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


	}
}
