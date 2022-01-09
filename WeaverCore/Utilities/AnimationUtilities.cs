using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Misc utility functions for animation related tasks
	/// </summary>
    public static class AnimationUtilities
	{
		/// <summary>
		/// Plays an animation state and waits until it is finished
		/// </summary>
		/// <param name="animator">The animator to play the state on</param>
		/// <param name="animationStateName">The state to play</param>
		public static IEnumerator PlayAnimationTillDone(this Animator animator, string animationStateName)
		{
			animator.Play(animationStateName);

			yield return null;

			yield return new WaitForSeconds(animator.GetCurrentAnimationTime());
		}

		/// <summary>
		/// Gets the duration of the currently playing animation state
		/// </summary>
		/// <param name="animator">The animator to check under</param>
		/// <returns>Returns the duration of the currently playing animation state</returns>
		public static float GetCurrentAnimationTime(this Animator animator)
		{
			var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

			return stateInfo.length / stateInfo.speed;
		}

		/// <summary>
		/// Plays an animation state
		/// </summary>
		/// <param name="animator">The animator to play the state on</param>
		/// <param name="animationStateName">The state to play</param>
		public static void PlayAnimation(this Animator animator, string animationStateName)
		{
			animator.Play(animationStateName);
        }
    }

}
