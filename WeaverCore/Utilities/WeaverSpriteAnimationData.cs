using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace WeaverCore.Utilities
{
	[CreateAssetMenu(fileName = "WeaverAnimation", menuName = "WeaverCore/Weaver Sprite Animation Data")]
	public class WeaverAnimationData : ScriptableObject
	{
		public struct Clip
		{
			public string Name;
			public List<Sprite> Frames;
			public float FPS;
			public WrapMode WrapMode;
			public int LoopStart;

			public Clip(string name, float fps, WrapMode wrapMode = WrapMode.Once, IEnumerable<Sprite> frames = null, int loopStart = 0)
			{
				Name = name;
				if (frames == null)
				{
					Frames = new List<Sprite>();
				}
				else
				{
					Frames = frames.ToList();
				}
				FPS = fps;
				WrapMode = wrapMode;
				LoopStart = loopStart;
			}

			public void AddFrames(IEnumerable<Sprite> frames)
			{
				if (Frames == null)
				{
					Frames = new List<Sprite>();
				}
				if (frames != null)
				{
					Frames.AddRange(frames);
				}
			}

			public void AddFrame(Sprite sprite)
			{
				if (Frames == null)
				{
					Frames = new List<Sprite>();
				}
				Frames.Add(sprite);
			}
		}

		public enum WrapMode
		{
			Once,
			Loop,
			LoopSection,
			PingPong,
			RandomFrame,
			RandomLoop,
			SingleFrame
		}


		[SerializeField]
		List<string> clipNames = new List<string>();

		[SerializeField]
		List<int> clipFrameStartIndexes = new List<int>();

		[SerializeField]
		List<int> clipFrameCounts = new List<int>();

		[SerializeField]
		List<Sprite> frames = new List<Sprite>();

		[SerializeField]
		List<float> clipFPSs = new List<float>();

		[SerializeField]
		List<WrapMode> clipWrapModes = new List<WrapMode>();

		[SerializeField]
		List<int> clipLoopStarts = new List<int>();

		public bool AddClip(Clip clip)
		{
			if (HasClip(clip))
			{
				return false;
			}
			clipNames.Add(clip.Name);
			clipFrameStartIndexes.Add(frames.Count);
			clipFrameCounts.Add(clip.Frames.Count);

			foreach (var frame in clip.Frames)
			{
				frames.Add(frame);
			}

			clipFPSs.Add(clip.FPS);
			clipWrapModes.Add(clip.WrapMode);
			clipLoopStarts.Add(clip.LoopStart);

			return true;
		}

		public bool HasClip(Clip clip)
		{
			return HasClip(clip.Name);
		}

		public bool HasClip(string clipName)
		{
			//Debug.Log("Adding Clip Name = " + clipName);
			//Debug.Log("Clip Names = " + (clipNames != null));
			return clipNames.Contains(clipName);
		}

		public bool RemoveClip(Clip clip)
		{
			return RemoveClip(clip.Name);
		}

		public bool RemoveClip(string clipName)
		{
			if (!HasClip(clipName))
			{
				return false;
			}
			var clipIndex = clipNames.IndexOf(clipName);

			clipNames.RemoveAt(clipIndex);

			var frameCount = clipFrameCounts[clipIndex];

			var startFrameIndex = clipFrameStartIndexes[clipIndex];

			for (int i = startFrameIndex + 1; i < clipFrameStartIndexes.Count; i++)
			{
				clipFrameStartIndexes[i] = clipFrameStartIndexes[i] - frameCount;
			}

			clipFrameStartIndexes.RemoveAt(clipIndex);
			clipFrameCounts.Remove(clipIndex);

			for (int i = 0; i < frameCount; i++)
			{
				frames.RemoveAt(startFrameIndex);
			}

			clipFPSs.RemoveAt(clipIndex);
			clipWrapModes.RemoveAt(clipIndex);
			clipLoopStarts.RemoveAt(clipIndex);
			return true;
		}

		public Clip GetClip(string clipName)
		{
			if (!HasClip(clipName))
			{
				throw new Exception("The clip " + clipName + " does not exist on this animator");
			}

			var clipIndex = GetClipIndex(clipName);//clipNames.IndexOf(clipName);

			var clip = new Clip
			{
				FPS = clipFPSs[clipIndex],
				LoopStart = clipLoopStarts[clipIndex],
				Name = clipName,
				WrapMode = clipWrapModes[clipIndex]
			};

			var frameStart = clipFrameStartIndexes[clipIndex];
			var frameCount = clipFrameCounts[clipIndex];

			for (int i = frameStart; i < frameCount; i++)
			{
				clip.AddFrame(frames[i]);
			}
			return clip;
		}

		public IEnumerable<string> ClipNames
		{
			get
			{
				return clipNames;
			}
		}

		public IEnumerable<Clip> AllClips
		{
			get
			{
				for (int i = clipNames.Count - 1; i >= 0; i--)
				{
					yield return GetClip(clipNames[i]);
				}
			}
		}

		public int AnimationCount
		{
			get
			{
				return clipNames.Count;
			}
		}

		public void Clear()
		{
			for (int i = clipNames.Count - 1; i >= 0; i--)
			{
				RemoveClip(clipNames[i]);
			}
		}


		public Sprite GetFrameFromClip(string clipName, int frameNumber)
		{
			if (!HasClip(clipName))
			{
				return null;
			}

			int clipIndex = GetClipIndex(clipName);

			int startingFrame = clipFrameStartIndexes[clipIndex];
			int frameCount = clipFrameCounts[clipIndex];
			int endingFrame = startingFrame + frameCount - 1;
			int loopStartFrame = clipLoopStarts[clipIndex] + startingFrame;
			WrapMode wrapMode = GetClipWrapModeRaw(clipIndex);

			if (wrapMode == WrapMode.PingPong)
			{
				if (frameNumber >= 0 && frameNumber < frameCount)
				{
					return frames[startingFrame + frameNumber];
				}
				else
				{
					frameNumber -= frameCount;

					if (frameNumber >= 0 && frameNumber < frameCount - 1)
					{
						return frames[endingFrame - frameNumber - 1];
					}
					else
					{
						return null;
					}
				}
			}
			else
			{
				if (frameNumber < 0 || frameNumber >= frameCount)
				{
					return null;
				}
				/*if (wrapMode == WrapMode.Loop)
				{
					Debug.Log("Clip Name = " + clipName);
					Debug.Log("Starting Frame = " + startingFrame);
					Debug.Log("Frame Number = " + frameNumber);
				}*/
				return frames[startingFrame + frameNumber];
			}

			/*int clipIndex = GetClipIndex(clipName);

			int startingFrame = clipFrameStartIndexes[clipIndex];
			int frameCount = clipFrameCounts[clipIndex];
			int endingFrame = startingFrame + frameCount - 1;
			int loopStartFrame = clipLoopStarts[clipIndex] + startingFrame;




			switch (GetClipWrapModeRaw(clipIndex))
			{
				case WrapMode.Loop:
					break;
				case WrapMode.PingPong:
					break;
				case WrapMode.ClampForever:
					break;
				//WrapMode.Once and WrapMode.Default case and WrapMode.Clamp
				default:
					if (frameNumber < 0 || frameNumber >= frameCount)
					{
						return null;
					}
					else
					{
						return frames[startingFrame + frameNumber];
					}
			}
			*/
		}

		/// <summary>
		/// Increments the clip to the next frame. Returns -1 if the animation is completed
		/// </summary>
		/// <param name="clipName">The clip to increment to the next frame</param>
		/// <param name="previousFrame">The previous frame the clip was on</param>
		/// <returns>The new frame the clip should go to, or -1 if the clip is done</returns>
		/// <exception cref="Exception">Throws an exception if the clip does not exist in the animation data object</exception>
		public int GoToNextFrame(string clipName, int previousFrame)
		{
			if (!HasClip(clipName))
			{
				return -1;
			}

			return GoToNextFrame(clipName, previousFrame, GetClipWrapMode(clipName));
			/*if (!HasClip(clipName))
			{
				return -1;
			}

			int clipIndex = GetClipIndex(clipName);

			int startingFrame = clipFrameStartIndexes[clipIndex];
			int frameCount = clipFrameCounts[clipIndex];
			int endingFrame = startingFrame + frameCount - 1;
			int loopStartFrame = clipLoopStarts[clipIndex] + startingFrame;

			switch (GetClipWrapModeRaw(clipIndex))
			{
				case WrapMode.Once:
					previousFrame += 1;
					if (previousFrame >= frameCount)
					{
						return -1;
					}
					else
					{
						return previousFrame;
					}
				case WrapMode.LoopSection:
					previousFrame += 1;
					if (previousFrame >= frameCount)
					{
						return loopStartFrame;
					}
					else
					{
						return previousFrame;
					}
				case WrapMode.PingPong:
					previousFrame += 1;
					if (previousFrame >= (frameCount * 2) - 1)
					{
						return 0;
					}
					else
					{
						return previousFrame;
					}
				case WrapMode.RandomFrame:
					return -1;
				case WrapMode.Loop:
				case WrapMode.RandomLoop:
					previousFrame += 1;
					if (previousFrame >= frameCount)
					{
						return 0;
					}
					else
					{
						return previousFrame;
					}
				case WrapMode.SingleFrame:
					return -1;
				default:
					return -1;
			}*/
		}

		public int GoToNextFrame(string clipName, int previousFrame, WrapMode wrapMode)
		{
			if (!HasClip(clipName))
			{
				return -1;
			}

			int clipIndex = GetClipIndex(clipName);

			int startingFrame = clipFrameStartIndexes[clipIndex];
			int frameCount = clipFrameCounts[clipIndex];
			int endingFrame = startingFrame + frameCount - 1;
			int loopStartFrame = clipLoopStarts[clipIndex] + startingFrame;

			switch (wrapMode)
			{
				case WrapMode.Once:
					previousFrame += 1;
					if (previousFrame >= frameCount)
					{
						return -1;
					}
					else
					{
						return previousFrame;
					}
				case WrapMode.LoopSection:
					previousFrame += 1;
					if (previousFrame >= frameCount)
					{
						return loopStartFrame - startingFrame;
					}
					else
					{
						return previousFrame;
					}
				case WrapMode.PingPong:
					previousFrame += 1;
					if (previousFrame >= (frameCount * 2) - 1)
					{
						return 0;
					}
					else
					{
						return previousFrame;
					}
				case WrapMode.RandomFrame:
					return -1;
				case WrapMode.Loop:
				case WrapMode.RandomLoop:
					previousFrame += 1;
					if (previousFrame >= frameCount)
					{
						return 0;
					}
					else
					{
						return previousFrame;
					}
				case WrapMode.SingleFrame:
					return -1;
				default:
					return -1;
			}
		}

		public int GetStartingFrame(string clipName)
		{
			if (!HasClip(clipName))
			{
				throw new Exception("The clip " + clipName + " does not exist in this animation data object");
			}

			int clipIndex = GetClipIndex(clipName);

			int startingFrame = clipFrameStartIndexes[clipIndex];
			int frameCount = clipFrameCounts[clipIndex];

			if (frameCount == 0)
			{
				return -1;
			}

			int endingFrame = startingFrame + frameCount - 1;
			int loopStartFrame = clipLoopStarts[clipIndex] + startingFrame;

			switch (GetClipWrapModeRaw(clipIndex))
			{
				case WrapMode.Once:
					return 0;
				case WrapMode.Loop:
					return 0;
				case WrapMode.LoopSection:
					return 0;
				case WrapMode.PingPong:
					return 0;
				case WrapMode.RandomFrame:
					return UnityEngine.Random.Range(0, frameCount);
				case WrapMode.RandomLoop:
					return UnityEngine.Random.Range(0, frameCount);
				case WrapMode.SingleFrame:
					return loopStartFrame - startingFrame;
				default:
					return -1;
			}
		}

		/// <summary>
		/// Gets the wrap mode for the specified clip. Throws an exception if the clip doesn't exist
		/// </summary>
		/// <param name="clipName">The clip to retrieve the wrap mode from</param>
		/// <returns>Returns the wrap mode for the clip</returns>
		public WrapMode GetClipWrapMode(string clipName)
		{
			if (!HasClip(clipName))
			{
				throw new Exception("The clip " + clipName + " does not exist in this animation data object");
			}
			return GetClipWrapModeRaw(GetClipIndex(clipName));
		}

		WrapMode GetClipWrapModeRaw(int clipIndex)
		{
			return clipWrapModes[clipIndex];
		}

		int GetClipIndex(string clipName)
		{
			return clipNames.IndexOf(clipName);
		}


		public float GetClipFPS(string clipName)
		{
			if (!HasClip(clipName))
			{
				throw new Exception("The clip " + clipName + " does not exist in this animation data object");
			}

			return GetClipFPSRaw(GetClipIndex(clipName));
		}

		float GetClipFPSRaw(int clipIndex)
		{
			return clipFPSs[clipIndex];
		}

		public int GetClipFrameCount(string clipName)
		{
			if (!HasClip(clipName))
			{
				throw new Exception("The clip " + clipName + " does not exist in this animation data object");
			}

			return GetClipFrameCountRaw(GetClipIndex(clipName));
		}

		int GetClipFrameCountRaw(int clipIndex)
		{
			return clipFrameCounts[clipIndex];
		}

		public int ClipCount
		{
			get
			{
				return clipNames.Count;
			}
		}
		
	}
}
