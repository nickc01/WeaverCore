using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Utilities
{
	public class Randomizer<T> : IComparer<T>
	{
		public static Randomizer<T> Instance = new Randomizer<T>();

		public int Compare(T x, T y)
		{
			return UnityEngine.Random.Range(-1, 2);
		}
	}
}
