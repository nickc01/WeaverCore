using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Utilities
{
	public static class ListUtilities
	{
		/// <summary>
		/// Adds an item to the list if it's not already in it
		/// </summary>
		/// <typeparam name="T">The list type</typeparam>
		/// <param name="list">The list to add to</param>
		/// <param name="item">The item to add</param>
		/// <returns>Whether it was able to add the item to the list or not</returns>
		public static bool AddIfNotContained<T>(this List<T> list, T item)
		{
			if (!list.Contains(item))
			{
				list.Add(item);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Returns a random item from the list
		/// </summary>
		/// <typeparam name="T">The type of items in the list</typeparam>
		/// <param name="list">The list to select from</param>
		/// <returns>A random element from the list</returns>
		public static T GetRandomElement<T>(this List<T> list)
		{
			if (list.Count == 0)
			{
				throw new Exception("The list to randomly select an element from is empty");
			}
			return list[UnityEngine.Random.Range(0,list.Count)];
		}
	}
}
