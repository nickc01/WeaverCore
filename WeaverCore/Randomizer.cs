using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore
{
    /// <summary>
    /// A comparer that is used to randomize a list
    /// </summary>
    /// <typeparam name="T">The type of elements to randomize</typeparam>
    public class Randomizer<T> : IComparer<T>
    {
        public static Randomizer<T> Instance = new Randomizer<T>();

        public int Compare(T x, T y)
        {
            return UnityEngine.Random.Range(-1, 2);
        }
    }
}
