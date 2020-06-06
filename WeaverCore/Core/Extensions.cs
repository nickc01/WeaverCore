using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore
{
	public static partial class Extensions
	{
		/// <summary>
		/// Adds an item to the list if it's not already in it
		/// </summary>
		/// <typeparam name="T">The list type</typeparam>
		/// <param name="list">The list to add to</param>
		/// <param name="item">The item to add</param>
		/// <returns>Whether it was able to add the item to the list or not</returns>
		public static bool AddConditional<T>(this List<T> list, T item)
		{
			if (!list.Contains(item))
			{
				list.Add(item);
				return true;
			}
			return false;
		}
	}

	public static partial class VectorExtensions
	{
		public static Vector3 With(this Vector3 v, float x = float.NaN, float y = float.NaN, float z = float.NaN)
		{
			if (!float.IsNaN(x))
			{
				v.x = x;
			}
			if (!float.IsNaN(y))
			{
				v.y = y;
			}
			if (!float.IsNaN(z))
			{
				v.z = z;
			}
			return v;
		}

		public static Vector2 With(this Vector2 v, float x = float.NaN, float y = float.NaN)
		{
			if (!float.IsNaN(x))
			{
				v.x = x;
			}
			if (!float.IsNaN(y))
			{
				v.y = y;
			}
			return v;
		}
	}
}
