using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Contains a series of animation clips that can be played with <see cref="WeaverCore.Components.WeaverAnimationPlayer"/>
	/// </summary>
	[CreateAssetMenu(fileName = "WeaverAnimation", menuName = "WeaverCore/Weaver Sprite Animation Data")]
	public class WeaverAnimationData : ScriptableObject
	{
		/// <summary>
		/// An animation clip that contains a series of sprites to play
		/// </summary>
		public struct Clip
		{
			/// <summary>
			/// The name of the clip
			/// </summary>
			public string Name;

			/// <summary>
			/// The sprites to be played
			/// </summary>
			public List<Sprite> Frames;

			/// <summary>
			/// How many frames per second should the animation be playing at
			/// </summary>
			public float FPS;

			/// <summary>
			/// Determines how the animation will loop
			/// </summary>
			public WrapMode WrapMode;

			/// <summary>
			/// The frame index where the loop begins
			/// </summary>
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

			/// <summary>
			/// Adds some frames to the clip
			/// </summary>
			/// <param name="frames">The frames to add</param>
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

			/// <summary>
			/// Adds a frame to the clip
			/// </summary>
			/// <param name="sprite">The frame to add</param>
			public void AddFrame(Sprite sprite)
			{
				if (Frames == null)
				{
					Frames = new List<Sprite>();
				}
				Frames.Add(sprite);
			}
		}

		/// <summary>
		/// Determines how an animation clip will loop
		/// </summary>
		public enum WrapMode
		{
			/// <summary>
			/// The clip will only play once
			/// </summary>
			Once,
			/// <summary>
			/// The clip will loop back to the start when it reaches the end
			/// </summary>
			Loop,
			/// <summary>
			/// The clip will loop back to the <seealso cref="Clip.LoopStart"/> index it reaches the end
			/// </summary>
			LoopSection,
			/// <summary>
			/// When the clip reaches the end, it will start playing the animation backwards
			/// </summary>
			PingPong,
			/// <summary>
			/// Will play a random frame in the clip's frames list
			/// </summary>
			RandomFrame,
			/// <summary>
			/// When the clip reaches the end, it will loop back to a random frame
			/// </summary>
			RandomLoop,
			/// <summary>
			/// Will only play a single frame
			/// </summary>
			SingleFrame,
			/// <summary>
			/// Will constantly play a random frame
			/// </summary>
			RandomContinuous
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

		/// <summary>
		/// Adds a clip
		/// </summary>
		/// <param name="clip">The clip to be added</param>
		/// <returns>Returns true if the clip is added, or false of the clip is already added</returns>
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

		/// <summary>
		/// Returns whether the clip has been added
		/// </summary>
		/// <param name="clip">The clip to check for</param>
		/// <returns>Returns whether the clip has been added</returns>
		public bool HasClip(Clip clip)
		{
			return HasClip(clip.Name);
		}

		/// <summary>
		/// Returns whether the clip with the specified name is added
		/// </summary>
		/// <param name="clipName">The clip name to check for</param>
		/// <returns>Returns whether the clip with the specified name is added</returns>
		public bool HasClip(string clipName)
		{
			return clipNames.Contains(clipName);
		}

		/// <summary>
		/// Removes a clip
		/// </summary>
		/// <param name="clip">The clip to remove</param>
		/// <returns>Returns whether the clip has been removed</returns>
		public bool RemoveClip(Clip clip)
		{
			return RemoveClip(clip.Name);
		}

		/// <summary>
		/// Removes a clip with the specified name
		/// </summary>
		/// <param name="clipName">The name of the clip to remove</param>
		/// <returns>Returns whether the clip has been removed</returns>
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


			for (int i = clipIndex + 1; i < clipFrameStartIndexes.Count; i++)
			{
				clipFrameStartIndexes[i] = clipFrameStartIndexes[i] - frameCount;
			}

			clipFrameStartIndexes.RemoveAt(clipIndex);
			clipFrameCounts.RemoveAt(clipIndex);

			for (int i = frameCount - 1; i >= 0; i--)
			{
				frames.RemoveAt(startFrameIndex + i);
			}

			clipFPSs.RemoveAt(clipIndex);
			clipWrapModes.RemoveAt(clipIndex);
			clipLoopStarts.RemoveAt(clipIndex);
			return true;
		}

		/// <summary>
		/// Finds a clip with the specified name
		/// </summary>
		/// <param name="clipName">The name of the clip to look for</param>
		/// <returns>Returns the clip with the specified name</returns>
		/// <exception cref="Exception">Throws an exception if the clip does not exist</exception>
		public Clip GetClip(string clipName)
		{
			if (!HasClip(clipName))
			{
				throw new Exception("The clip " + clipName + " does not exist on this animator");
			}

			var clipIndex = GetClipIndex(clipName);

			var clip = new Clip
			{
				FPS = clipFPSs[clipIndex],
				LoopStart = clipLoopStarts[clipIndex],
				Name = clipName,
				WrapMode = clipWrapModes[clipIndex]
			};

			var frameStart = clipFrameStartIndexes[clipIndex];
			var frameCount = clipFrameCounts[clipIndex];

			for (int i = frameStart; i < frameStart + frameCount; i++)
			{
				clip.AddFrame(frames[i]);
			}
			return clip;
		}

		/// <summary>
		/// Attempts to get a clip with the specified name
		/// </summary>
		/// <param name="clipName">The name of the clip to get</param>
		/// <param name="clip">The output clip</param>
		/// <returns>Returns true if a clip was found</returns>
		public bool TryGetClip(string clipName, out Clip clip)
        {
            if (HasClip(clipName))
            {
				clip = GetClip(clipName);
				return true;
            }
			else
            {
				clip = default;
				return false;
            }
        }

		/// <summary>
		/// A list of all added clip names
		/// </summary>
		public IEnumerable<string> ClipNames
		{
			get
			{
				return clipNames;
			}
		}

		/// <summary>
		/// A list of all added clips
		/// </summary>
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

		/// <summary>
		/// How many clips have been added
		/// </summary>
		public int AnimationCount
		{
			get
			{
				return clipNames.Count;
			}
		}

		/// <summary>
		/// Removes all clips from the animation data object
		/// </summary>
		public void Clear()
		{
			for (int i = clipNames.Count - 1; i >= 0; i--)
			{
				RemoveClip(clipNames[i]);
			}
		}

		/// <summary>
		/// Given a frame index, will get the sprite corresponding to that frame
		/// </summary>
		/// <param name="clipName">The clip to get the frame from</param>
		/// <param name="frameIndex">The frame index</param>
		/// <returns>Returns the sprite at the frame index</returns>
		public Sprite GetFrameFromClip(string clipName, int frameIndex)
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
				if (frameIndex >= 0 && frameIndex < frameCount)
				{
					return frames[startingFrame + frameIndex];
				}
				else
				{
					frameIndex -= frameCount;

					if (frameIndex >= 0 && frameIndex < frameCount - 1)
					{
						return frames[endingFrame - frameIndex - 1];
					}
					else
					{
						return null;
					}
				}
			}
			else
			{
				if (frameIndex < 0 || frameIndex >= frameCount)
				{
					return null;
				}
				return frames[startingFrame + frameIndex];
			}
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
		}

		/// <summary>
		/// Given the wrap mode and previous frame of a clip, will give the next frame in the clip
		/// </summary>
		/// <param name="clipName">The name of the clip to get the frame from</param>
		/// <param name="previousFrame">The previous frame in the clip</param>
		/// <param name="wrapMode">The wrap mode of the clip</param>
		/// <returns>Returns the next frame in the clip's sequence</returns>
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
				case WrapMode.RandomContinuous:
					int newFrame = UnityEngine.Random.Range(0, frameCount - 1);
                    if (newFrame >= previousFrame)
                    {
						newFrame++;
                    }
					return newFrame;
				default:
					return -1;
			}
		}

		/// <summary>
		/// Gets the first frame of a clip
		/// </summary>
		/// <param name="clipName">The clip to get the first frame of</param>
		/// <returns>Returns the starting frame of the clip</returns>
		/// <exception cref="Exception">Throws if the clip doesn't exist in the Animation Data Object</exception>
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
				case WrapMode.RandomContinuous:
					return UnityEngine.Random.Range(0, frameCount);
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

		/// <summary>
		/// Gets the FPS of a certain clip
		/// </summary>
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

		/// <summary>
		/// Gets the amount of frames in a specific clip
		/// </summary>
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

		/// <summary>
		/// The amount of clips added to the data object
		/// </summary>
		public int ClipCount
		{
			get
			{
				return clipNames.Count;
			}
		}

		/// <summary>
		/// Gets the duration of a specific clip
		/// </summary>
		public float GetClipDuration(string clipName)
		{
			if (!HasClip(clipName))
			{
				throw new Exception("The clip " + clipName + " does not exist in this animation data object");
			}
			return GetClipDurationRaw(GetClipIndex(clipName));
		}

		float GetClipDurationRaw(int clipIndex)
		{
			var frames = clipFrameCounts[clipIndex];
			var fps = clipFPSs[clipIndex];
			return frames * (1f / fps);
		}
		
	}
}
