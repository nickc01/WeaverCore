using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
#endif

namespace WeaverCore.Utilities
{
    public static class CacheUtilities
    {
		static Cache<string, object> arrayCache = new Cache<string, object>();

		public static T[] GetTempArray<T>(int size)
		{
			if (!arrayCache.GetCachedObject($"{typeof(T)}:{size}", out var array))
			{
				array = new T[size];
			}

			return (T[])array;
		}

		public static T[] GetTempArray<T>(int size, Func<int, T> initializer)
		{
			var array = GetTempArray<T>(size);
			for (int i = 0; i < size; i++)
			{
				array[i] = initializer(i);
			}
			return array;
		}

		public static T[] GetTempSingleArray<T>(T value)
		{
			var array = GetTempArray<T>(1);
			array[0] = value;
			return array;
		}
    }
}
