using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using WeaverCore.Components;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Contains many utility functions related to the Hero (The player character)
	/// </summary>
	public static class HeroUtilities
    {
		static Component heroAnimator = null;
		static Type heroAnimatorType;
		static PropertyInfo CurrentClipP;
		static Type ClipType;
		static PropertyInfo ClipDurationP;

		static void InitHeroAnim()
		{
            if (heroAnimator == null)
            {
                heroAnimator = HeroController.instance.GetComponent("tk2dSpriteAnimator");
                heroAnimatorType = heroAnimator.GetType();
                CurrentClipP = heroAnimatorType.GetProperty("CurrentClip");
                ClipType = CurrentClipP.PropertyType;
                ClipDurationP = ClipType.GetProperty("Duration");
            }
        }

		/// <summary>
		/// Plays an animation clip on the player, and waits until the clip is done
		/// </summary>
		/// <param name="clip">The clip to play</param>
		public static IEnumerator PlayPlayerClipTillDone(string clip)
		{
			if (Initialization.Environment == Enums.RunningState.Game)
			{
				InitHeroAnim();
                heroAnimator.SendMessage("Play", clip);
				yield return new WaitForSeconds((float)ClipDurationP.GetValue(CurrentClipP.GetValue(heroAnimator)));
			}

			var weaverAnim = HeroController.instance.GetComponent<WeaverAnimationPlayer>();
			if (weaverAnim != null)
			{
				yield return weaverAnim.PlayAnimationTillDone(clip);
			}
		}

		/// <summary>
		/// Plays an animation clip on the player
		/// </summary>
		/// <param name="clip">The clip to play</param>
		public static void PlayPlayerClip(string clip)
		{
			if (Initialization.Environment == Enums.RunningState.Game)
			{
                InitHeroAnim();
                heroAnimator.SendMessage("Play", clip);
			}

			var weaverAnim = HeroController.instance.GetComponent<WeaverAnimationPlayer>();
			weaverAnim?.PlayAnimation(clip);
		}

		public static void PauseCurrentAnimation(bool pause)
		{
            InitHeroAnim();

			if (pause)
			{
				//heroAnimator.SendMessage("Pause");
				heroAnimator.ReflectCallMethod("Pause");
			}
			else
			{
                //heroAnimator.SendMessage("Resume");
                heroAnimator.ReflectCallMethod("Resume");
            }
		}
	}

}
