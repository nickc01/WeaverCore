using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains some utility functions related to the Lists and Arrays
    /// </summary>
    public static class ListUtilities
	{
		static class GenericGlobals<T>
		{
			public static Func<System.Collections.Generic.List<T>, T[]> internalArrayGetter;
		}
		/// <summary>
		/// Adds an item to the list if it's not already in it
		/// </summary>
		/// <typeparam name="T">The list type</typeparam>
		/// <param name="list">The list to add to</param>
		/// <param name="item">The item to add</param>
		/// <returns>Whether it was able to add the item to the list or not</returns>
		public static bool AddIfNotContained<T>(this System.Collections.Generic.List<T> list, T item)
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
		public static T GetRandomElement<T>(this System.Collections.Generic.List<T> list)
		{
			if (list.Count == 0)
			{
				throw new Exception("The list to randomly select an element from is empty");
			}
			return list[UnityEngine.Random.Range(0,list.Count)];
		}

		public static T GetRandomElement<T>(this System.Collections.Generic.List<T> list, int startIndex, int endIndex)
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
		public static void RandomizeList<T>(this System.Collections.Generic.List<T> list)
		{
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        class SortByComparer<T> : IComparer<T>
        {
			public readonly Func<T, int> Getter;
			public readonly Comparer<int> Comparer = Comparer<int>.Default;

            public SortByComparer(Func<T, int> getter)
            {
                Getter = getter;
            }

            public int Compare(T x, T y)
            {
				return Comparer.Compare(Getter(x), Getter(y));
            }
        }

        public static void SortBy<T>(this System.Collections.Generic.List<T> list, Func<T,int> getter)
		{
			list.Sort(new SortByComparer<T>(getter));
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

        private static Random rng = new Random();

		/// <summary>
		/// Shuffles the elements in a list. Note: This modifies the list itself
		/// </summary>
		/// <typeparam name="T">The type of elements in the list</typeparam>
		/// <param name="source">The list to shuffle</param>
        public static void ShuffleInPlace<T>(this IList<T> source)
        {
            int n = source.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = source[k];
                source[k] = source[n];
                source[n] = value;
            }
        }

		/// <summary>
		/// Takes in an IEnumerable, and returns a new IEnumerable with the elements shuffled around
		/// </summary>
		/// <typeparam name="T">The type of the elements in the IEnumerable</typeparam>
		/// <param name="source">The source IEnumerator to use</param>
		/// <returns>Returns a new IEnumerable with the elements shuffled around</returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			return source.OrderBy(e => UnityEngine.Random.Range(0f,1f));
		}

		/// <summary>
		/// Gets the index of a value in an array
		/// </summary>
		/// <typeparam name="T">The type of elements in the array</typeparam>
		/// <param name="values">The array to check</param>
		/// <param name="value">The value to find</param>
		/// <returns>Returns the index of the value. Returns -1 if the value couldn't be found</returns>
        public static int IndexOf<T>(this T[] values, T value)
        {
			for (int i = 0; i < values.Length; i++)
            {
				if (values[i].Equals(value))
				{
					return i;
				}
            }
			return -1;
        }

        class HashSetGetters<T>
		{
			public delegate int InternalIndexOfDelegate(HashSet<T> instance, T item);
			public static InternalIndexOfDelegate InternalIndexOf;

            public delegate bool TryGetValueDelegate(HashSet<T> instance, T equalValue, out T actualValue);
            public static TryGetValueDelegate TryGetValue;

            public static FieldInfo slotsGetter;
			public static Type slotType;
			public static FieldInfo slotsValueGetter;

            public static bool Initialized = false;

        }

		/// <summary>
		/// Attempts to find a value in a hashset that is equivalent to a source value
		/// </summary>
		/// <typeparam name="T">The type of elements in the HashSet</typeparam>
		/// <param name="set">The HashSet to check over</param>
		/// <param name="source">The source value to find</param>
		/// <param name="equivalent">The equivalent value found in the HashSet</param>
		/// <returns>Returns true if an equivalent has been found</returns>
		public static bool TryGetEquivalent<T>(this HashSet<T> set, T source, out T equivalent)
		{
			if (!HashSetGetters<T>.Initialized)
			{
				HashSetGetters<T>.Initialized = true;
                if (HashSetGetters<T>.InternalIndexOf == null)
                {
                    HashSetGetters<T>.InternalIndexOf = ReflectionUtilities.MethodToDelegate<HashSetGetters<T>.InternalIndexOfDelegate, HashSet<T>>("InternalIndexOf");
                }

                if (HashSetGetters<T>.TryGetValue == null)
                {
                    HashSetGetters<T>.TryGetValue = ReflectionUtilities.MethodToDelegate<HashSetGetters<T>.TryGetValueDelegate, HashSet<T>>("TryGetValue");
                }

				if (HashSetGetters<T>.slotsGetter == null)
				{
					HashSetGetters<T>.slotsGetter = typeof(HashSet<T>).GetField("_slots", BindingFlags.NonPublic | BindingFlags.Instance);

					HashSetGetters<T>.slotType = typeof(HashSet<T>).GetNestedType("Slot", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

					HashSetGetters<T>.slotsValueGetter = HashSetGetters<T>.slotType.GetField("value", BindingFlags.Instance | BindingFlags.NonPublic);
                }
            }


			if (HashSetGetters<T>.TryGetValue != null)
			{
				return HashSetGetters<T>.TryGetValue(set, source, out equivalent);
			}
			else
			{
				var index = HashSetGetters<T>.InternalIndexOf(set, source);

				if (index < 0)
				{
					equivalent = default;
					return false;
				}
				else
				{
					var slots = (Array)HashSetGetters<T>.slotsGetter.GetValue(set);

					var slot = slots.GetValue(index);

					equivalent = (T)HashSetGetters<T>.slotsValueGetter.GetValue(slot);

					return true;
				}
			}
		}
    }
}
