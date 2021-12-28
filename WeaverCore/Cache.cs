using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore
{
	/// <summary>
	/// A cache that stores objects in a dictonary structure. When an object gets destroyed or garbage collected, the cache object automatically removes it
	/// </summary>
	/// <typeparam name="TKey">The key data type</typeparam>
	/// <typeparam name="TValue">The value data type</typeparam>
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

		/// <summary>
		/// Checks if an object under the specified key is still alive
		/// </summary>
		/// <param name="key">The key to check the object under</param>
		/// <returns>Returns whether the object is still in the cache</returns>
		public bool IsCached(TKey key)
		{
			lock (cacheLock)
			{
				return cachedObjects.ContainsKey(key) && cachedObjects[key].IsAlive;
			}
		}

		/// <summary>
		/// Gets the cached object based on the key. Make sure to use <see cref="IsCached(TKey)"/> before calling, or it could thrown an exception
		/// </summary>
		/// <param name="key">The key to check the object under</param>
		/// <returns>Returns a reference to the cached object</returns>
		public TValue GetCachedObject(TKey key)
		{
			lock (cacheLock)
			{
				if (!IsCached(key))
				{
					throw new Exception("The cached object under the key (" + key + ") no longer exists or was never added");
				}

				return (TValue)cachedObjects[key].Target;
			}
		}

		/// <summary>
		/// Gets the cached object based on the key
		/// </summary>
		/// <param name="key">The key to check the object under</param>
		/// <param name="obj">The reference to the cached object</param>
		/// <returns>Returns whether the object was in the cache or not</returns>
		public bool GetCachedObject(TKey key, out TValue obj)
		{
			lock (cacheLock)
			{
				if (IsCached(key))
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

		/// <summary>
		/// Removes an object from the cache
		/// </summary>
		/// <param name="key">The key of the object to remove</param>
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

		/// <summary>
		/// Caches an object
		/// </summary>
		/// <param name="key">The key to cache the object under</param>
		/// <param name="value">The object to cache</param>
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
