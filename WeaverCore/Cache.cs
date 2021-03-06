﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore
{
	public sealed class Cache<TKey,TValue> where TValue : class
	{
		Dictionary<TKey, WeakReference> cachedObjects = new Dictionary<TKey, WeakReference>();

		object cacheLock = new object();

		/// <summary>
		/// The amount of objects in the cache. This will include objects that are no longer alive. You can get an accurate count by calling <see cref="RefreshCache"/> first
		/// </summary>
		public int CacheSize 
		{ 
			get 
			{
				lock (cacheLock)
				{
					return cachedObjects.Count;
				}
			} 
		}

		/// <summary>
		/// Refreshes the cache by removing deleted objects
		/// </summary>
		public void RefreshCache()
		{
			lock (cacheLock)
			{
				var keys = cachedObjects.Keys.ToArray();
				var length = keys.GetLength(0);

				for (int i = 0; i < length; i++)
				{
					var reference = cachedObjects[keys[i]];
					if (!reference.IsAlive)
					{
						cachedObjects.Remove(keys[i]);
					}
				}
			}
		}

		public bool IsObjectAlive(TKey key)
		{
			lock (cacheLock)
			{
				return cachedObjects.ContainsKey(key) && cachedObjects[key].IsAlive;
			}
		}

		/// <summary>
		/// Gets the cached object based on the key. Make sure to use <see cref="IsObjectAlive(TKey)"/> before calling, or it could thrown an exception
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public TValue GetCachedObject(TKey key)
		{
			lock (cacheLock)
			{
				if (!IsObjectAlive(key))
				{
					throw new Exception("The cached object under the key (" + key + ") no longer exists or was never added");
				}

				return (TValue)cachedObjects[key].Target;
			}
		}

		public bool GetCachedObject(TKey key, out TValue obj)
		{
			lock (cacheLock)
			{
				if (IsObjectAlive(key))
				{
					obj = (TValue)cachedObjects[key].Target;
					return true;
				}
				else
				{
					obj = default(TValue);
					return false;
				}
			}
		}

		public void RemoveCachedObject(TKey key)
		{
			lock (cacheLock)
			{
				if (cachedObjects.ContainsKey(key))
				{
					cachedObjects.Remove(key);
				}
			}
		}

		public void CacheObject(TKey key, TValue value)
		{
			lock (cacheLock)
			{
				RemoveCachedObject(key);
				cachedObjects.Add(key, new WeakReference(value));
			}
		}
	}
}
