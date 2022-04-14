using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Contains some utility functions related to the Linear Interpolation
	/// </summary>
	public static class LerpUtilities
	{
		/// <summary>
		/// Will move <paramref name="from"/> a value <paramref name="to"/> a value at a rate of 1 unit per second.
		/// </summary>
		/// <param name="from">The value to move from</param>
		/// <param name="to">The value to move to</param>
		/// <param name="action">The action that is called every frame until we get to the destination value</param>
		public static IEnumerator TimeLerp(float from, float to, Action<float> action)
		{
			for (float t = from; t < to; t = AdjustTowards(t, to, Time.deltaTime))
			{
				action(t);
				yield return null;
			}
			action(to);
		}

		/// <summary>
		/// Will move <paramref name="from"/> a value <paramref name="to"/> a value at a rate of 1 unit per second.
		/// </summary>
		/// <param name="from">The value to move from</param>
		/// <param name="to">The value to move to</param>
		/// <param name="action">The action that is called every frame until we get to the destination value. However, if the action returns false at any time, the interpolation routine is cancelled</param>
		/// <returns></returns>
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

		/// <summary>
		/// Will adjust <paramref name="from"/> a value <paramref name="to"/> a value by a set <paramref name="amount"/>
		/// </summary>
		/// <param name="from">The value to adjust from</param>
		/// <param name="to">The value to adjust to</param>
		/// <param name="amount">The amount to move from the "from" value to the "to" value</param>
		/// <returns>Returns the adjusted float value</returns>
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

		public static float UnclampedLerp(float a, float b, float t)
        {
			return ((b - a) * t) + a;
        }

		public static float UnclampedInverseLerp(float a, float b, float v)
		{
			return (v - a) / (b - a);
		}
	}
}
