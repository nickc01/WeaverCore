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

        public static void SortBy<T>(this List<T> list, Func<T,int> getter)
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

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			return source.OrderBy(e => UnityEngine.Random.Range(0f,1f));
		}

		/*public static int IndexOf<T>(this IEnumerable<T> values, T value)
		{
			int index = 0;
			foreach (var val in values)
			{
				if (val.Equals(value))
				{
					return index;
				}
				index++;
			}
			return -1;
		}*/

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

        /*        public bool Contains(T item)
        {
            if (m_buckets != null)
            {
                int num = InternalGetHashCode(item);
                for (int num2 = m_buckets[num % m_buckets.Length] - 1; num2 >= 0; num2 = m_slots[num2].next)
                {
                    if (m_slots[num2].hashCode == num && m_comparer.Equals(m_slots[num2].value, item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }*/

        class HashSetGetters<T>
		{
			//public static Func<HashSet<T>, int[]> bucketGetter;
			//public static Func<HashSet<T>, int[]> slotsGetter;
			//public static Func<HashSet<T>,T,int> internalGetHashCode;

			public delegate int InternalIndexOfDelegate(HashSet<T> instance, T item);
			public static InternalIndexOfDelegate InternalIndexOf;

            public delegate bool TryGetValueDelegate(HashSet<T> instance, T equalValue, out T actualValue);
            public static TryGetValueDelegate TryGetValue;

            public static FieldInfo slotsGetter;
			public static Type slotType;
			public static FieldInfo slotsValueGetter;

            public static bool Initialized = false;

        }

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
