using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Utilities
{
	public static class LerpUtilities
	{
		public static IEnumerator TimeLerp(float from, float to, Action<float> action)
		{
			for (float t = from; t < to; t = AdjustTowards(t, to, Time.deltaTime))
			{
				action(t);
				yield return null;
			}
			action(to);
		}

		public static IEnumerator TimeLerp(float from, float to, Func<float, bool> action)
		{
			for (float t = from; t < to; t = AdjustTowards(t, to, Time.deltaTime))
			{
				if (!action(t))
				{
					yield break;
				}
				yield return null;
			}
			action(to);
		}


		public static float AdjustTowards(float from, float to, float amount)
		{
			if (from < to)
			{
				return Mathf.Clamp(from + amount, from, to);
			}
			else
			{
				return Mathf.Clamp(from - amount, from, to);
			}
		}
	}
}
