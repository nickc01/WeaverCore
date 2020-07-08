using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Utilities
{
	public static class AnimationUtilities
	{
		public static IEnumerator PlayAnimationTillDone(this Animator animator, string animationStateName)
		{
			animator.Play(animationStateName);

			yield return null;

			yield return new WaitForSeconds(animator.GetCurrentAnimationTime());
		}

		public static float GetCurrentAnimationTime(this Animator animator)
		{
			var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

			return stateInfo.length / stateInfo.speed;
		}

		public static void PlayAnimation(this Animator animator, string animationStateName)
		{
			animator.Play(animationStateName);
		}
	}
}
