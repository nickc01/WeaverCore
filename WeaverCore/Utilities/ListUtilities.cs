using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains some utility functions related to the Lists and Arrays
    /// </summary>
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

		public static T GetRandomElement<T>(this List<T> list, int startIndex, int endIndex)
		{
			if (startIndex < 0 || startIndex > list.Count)
			{
				throw new Exception("Invalid Start Index");
			}
			if (endIndex < 0 || endIndex > list.Count)
			{
				throw new Exception("Invalid End Index");
			}

            return list[UnityEngine.Random.Range(startIndex, endIndex)];
        }

		/// <summary>
		/// Randomizes the list
		/// </summary>
		/// <typeparam name="T">The type of elements in the list</typeparam>
		/// <param name="list">The list to randomize</param>
		public static void RandomizeList<T>(this List<T> list)
		{
			list.Sort(Randomizer<T>.Instance);
		}

		/// <summary>
		/// Checks if the two lists are equivalent
		/// </summary>
		/// <typeparam name="T">The type of element in the lists</typeparam>
		/// <param name="listA">The first list to compare</param>
		/// <param name="listB">The second list to compare</param>
		/// <param name="comparer">The comparer used to check for element equality. If this is null, will use the default comparer</param>
		/// <returns></returns>
		public static bool AreEquivalent<T>(this IEnumerable<T> listA, IEnumerable<T> listB, EqualityComparer<T> comparer = null)
		{
			if (comparer == null)
			{
				comparer = EqualityComparer<T>.Default;
			}

			using (var enumA = listA.GetEnumerator())
			{
				using (var enumB = listB.GetEnumerator())
				{
					while (true)
					{
						var continueA = enumA.MoveNext();
						var continueB = enumB.MoveNext();

						if (continueA != continueB)
						{
							return false;
						}
						if (continueA == false)
						{
							return true;
						}
						if (!comparer.Equals(enumA.Current,enumB.Current))
						{
							return false;
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets a hash for all the elements in the list
		/// </summary>
		/// <typeparam name="T">The type of elements in the list</typeparam>
		/// <param name="list">The list to get the hash code from</param>
		/// <returns>Returns a hash for all the elements in the list</returns>
		public static int GetListHash<T>(this IEnumerable<T> list)
		{
			int hash = 0;
			foreach (var item in list)
			{
				HashUtilities.AdditiveHash(ref hash, item.GetHashCode());
			}
			return hash;
		}
	}
}
