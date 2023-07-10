using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore
{
    class PoolInfo
	{
		public ObjectPool Pool;
		public float Timer;
	}

	class PoolTimer
	{
		public float timer;
		public GameObject obj;
	}

	/// <summary>
	/// A class that makes it easy to pool any object you want. Acts as a drop-in replacement for GameObject.Instantiate, while still reaping the benefits of pooling
	/// 
	/// This class handles creating <see cref="ObjectPool"/>s for you, so you can just call <see cref="Instantiate(GameObject)"/> to instantiate a pooled object
	/// </summary>
	public static class Pooling
	{
		/// <summary>
		/// An object used to keep track of how long pools are being used for, and destroy old ones when needed
		/// </summary>
		class TimerObject : MonoBehaviour
		{
			Queue<GameObject> Removals = new Queue<GameObject>();

			void Awake()
			{
				StartCoroutine(CheckerRoutine());
			}

			IEnumerator CheckerRoutine()
			{
				while (true)
				{
					yield return null;

					foreach (var poolPair in pools)
					{
						var info = poolPair.Value;

						info.Timer += Time.deltaTime;
						if (info.Timer >= MaxPoolTime)
						{
							if (info.Pool != null && info.Pool.gameObject != null)
							{
								info.Pool.ClearPool();
								Destroy(info.Pool.gameObject);
							}
							Removals.Enqueue(poolPair.Key);
						}
					}

					while (Removals.Count > 0)
					{
						pools.Remove(Removals.Dequeue());
					}
				}
			}
		}

		const float MaxPoolTime = 60f * 5f;
		static TimerObject PoolTimerObject;
		static Dictionary<GameObject, PoolInfo> pools = new Dictionary<GameObject, PoolInfo>();

		static ObjectPool GetPool(GameObject obj)
		{
			if (!pools.ContainsKey(obj))
			{

				var pool = ObjectPool.Create(obj);

				pools.Add(obj, new PoolInfo
				{
					Pool = pool,
					Timer = 0f
				});

			}

			if (PoolTimerObject == null)
			{
				PoolTimerObject = new GameObject().AddComponent<TimerObject>();
				PoolTimerObject.gameObject.hideFlags = HideFlags.HideInHierarchy;
				GameObject.DontDestroyOnLoad(PoolTimerObject);
			}

			var info = pools[obj];
			if (info.Pool == null || info.Pool.gameObject == null)
			{
				info.Pool = ObjectPool.Create(obj);
			}
			info.Timer = 0f;
			return info.Pool;
		}

		/// <summary>
		/// Instantiates a pooled object
		/// </summary>
		/// <param name="prefab">The prefab to instantiate from a pool</param>
		/// <param name="position">The position of the newly instantiated object</param>
		/// <param name="rotation">The rotation of the newly instantiated object</param>
		/// <param name="parent">The parent of the newly instantiated object</param>
		/// <returns>Returns a newly instantiated object</returns>
		public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
		{
			return GetPool(prefab).Instantiate(position, rotation, parent);
		}

		/// <summary>
		/// Instantiates a pooled object
		/// </summary>
		/// <param name="prefab">The prefab to instantiate from a pool</param>
		/// <param name="parent">The parent of the newly instantiated object</param>
		/// <returns>Returns a newly instantiated object</returns>
		public static GameObject Instantiate(GameObject prefab, Transform parent)
		{
			return GetPool(prefab).Instantiate(parent);
		}

		/// <summary>
		/// Instantiates a pooled object
		/// </summary>
		/// <param name="prefab">The prefab to instantiate from a pool</param>
		/// <returns>Returns a newly instantiated object</returns>
		public static GameObject Instantiate(GameObject prefab)
		{
			return GetPool(prefab).Instantiate();
		}

		/// <summary>
		/// Instantiates a pooled object
		/// </summary>
		/// <param name="prefab">The prefab to instantiate from a pool</param>
		/// <param name="position">The position of the newly instantiated object</param>
		/// <param name="rotation">The rotation of the newly instantiated object</param>
		/// <returns>Returns a newly instantiated object</returns>
		public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			return GetPool(prefab).Instantiate(position, rotation);
		}

		/// <summary>
		/// Instantiates a pooled object
		/// </summary>
		/// <param name="prefab">The prefab to instantiate from a pool</param>
		/// <param name="parent">The parent of the newly instantiated object</param>
		/// <param name="instantiateInWorldSpace">Should the object be positioned in world space? If false, it will be positioned relative to the parent</param>
		/// <returns>Returns a newly instantiated object</returns>
		public static GameObject Instantiate(GameObject prefab, Transform parent, bool instantiateInWorldSpace)
		{
			if (instantiateInWorldSpace)
			{
				return GetPool(prefab).Instantiate(parent, instantiateInWorldSpace);
			}
			else
			{
				return GetPool(prefab).Instantiate(prefab.transform.position + parent.transform.position, parent.transform.rotation * prefab.transform.rotation, parent);
			}
		}

		/// <summary>
		/// Instantiates a pooled object
		/// </summary>
		/// <typeparam name="T">The component type to get from the instantiated object</typeparam>
		/// <param name="prefab">The prefab to instantiate from a pool</param>
		/// <param name="position">The position of the newly instantiated object</param>
		/// <param name="rotation">The rotation of the newly instantiated object</param>
		/// <param name="parent">The parent of the newly instantiated object</param>
		/// <returns>Returns a newly instantiated object</returns>
		public static T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
		{
			return GetPool(prefab.gameObject).Instantiate<T>(position, rotation, parent);
		}

		/// <summary>
		/// Instantiates a pooled object
		/// </summary>
		/// <typeparam name="T">The component type to get from the instantiated object</typeparam>
		/// <param name="prefab">The prefab to instantiate from a pool</param>
		/// <param name="parent">The parent of the newly instantiated object</param>
		/// <returns>Returns a newly instantiated object</returns>
		public static T Instantiate<T>(T prefab, Transform parent) where T : Component
		{
			return GetPool(prefab.gameObject).Instantiate<T>(parent);
		}

		/// <summary>
		/// Instantiates a pooled object
		/// </summary>
		/// <typeparam name="T">The component type to get from the instantiated object</typeparam>
		/// <param name="prefab">The prefab to instantiate from a pool</param>
		/// <returns>Returns a newly instantiated object</returns>
		public static T Instantiate<T>(T prefab) where T : Component
		{
			return GetPool(prefab.gameObject).Instantiate<T>(prefab.transform.position, prefab.transform.rotation, null);
		}

		/// <summary>
		/// Instantiates a pooled object
		/// </summary>
		/// <typeparam name="T">The component type to get from the instantiated object</typeparam>
		/// <param name="prefab">The prefab to instantiate from a pool</param>
		/// <param name="position">The position of the newly instantiated object</param>
		/// <param name="rotation">The rotation of the newly instantiated object</param>
		/// <returns>Returns a newly instantiated object</returns>
		public static T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
		{
			return GetPool(prefab.gameObject).Instantiate<T>(position, rotation, null);
		}

		/// <summary>
		/// Instantiates a pooled object
		/// </summary>
		/// <typeparam name="T">The component type to get from the instantiated object</typeparam>
		/// <param name="prefab">The prefab to instantiate from a pool</param>
		/// <param name="parent">The parent of the newly instantiated object</param>
		/// <param name="instantiateInWorldSpace">Should the object be positioned in world space? If false, it will be positioned relative to the parent</param>
		/// <returns>Returns a newly instantiated object</returns>
		public static T Instantiate<T>(T prefab, Transform parent, bool instantiateInWorldSpace) where T : Component
		{
			if (instantiateInWorldSpace)
			{
				return GetPool(prefab.gameObject).Instantiate<T>(prefab.transform.position, prefab.transform.rotation, parent);
			}
			else
			{
				return GetPool(prefab.gameObject).Instantiate<T>(prefab.transform.position + parent.transform.position, parent.transform.rotation * prefab.transform.rotation, parent);
			}
		}

		/// <summary>
		/// Destroys an existing object and returns it to a pool
		/// </summary>
		/// <param name="gameObject">The object to destroy</param>
		public static void Destroy(GameObject gameObject)
		{
			if (gameObject.TryGetComponent<PoolableObject>(out var pool))
			{
				pool.ReturnToPool();
			}
			else
			{
				GameObject.Destroy(gameObject);
			}
		}

		/// <summary>
		/// Destroys an existing object and returns it to a pool
		/// </summary>
		/// <param name="poolableObject">The object to destroy</param>
		public static void Destroy(PoolableObject poolableObject)
		{
			poolableObject.ReturnToPool();
		}

		/// <summary>
		/// Destroys an existing object and returns it to a pool
		/// </summary>
		/// <typeparam name="T">The type of component from the destroyed object</typeparam>
		/// <param name="component">The object to destroy</param>
		public static void Destroy<T>(T component) where T : Component
		{
			if (component is PoolableObject selfPool)
			{
				Destroy(selfPool);
			}
			else if (component.TryGetComponent<PoolableObject>(out var pool))
			{
				Destroy(pool);
			}
			else
			{
				GameObject.Destroy(component.gameObject);
			}
		}

		/// <summary>
		/// Destroys an existing object and returns it to a pool
		/// </summary>
		/// <param name="gameObject">The object to destroy</param>
		/// <param name="time">The delay before the object is returned to the pool</param>
		public static void Destroy(GameObject gameObject, float time)
		{
			gameObject.GetComponent<PoolableObject>().ReturnToPool(time);
		}

		/// <summary>
		/// Destroys an existing object and returns it to a pool
		/// </summary>
		/// <param name="poolableObject">The object to destroy</param>
		/// <param name="time">The delay before the object is returned to the pool</param>
		public static void Destroy(PoolableObject poolableObject, float time)
		{
			poolableObject.ReturnToPool(time);
		}

		/// <summary>
		/// Destroys an existing object and returns it to a pool
		/// </summary>
		/// <typeparam name="T">The type of component from the destroyed object</typeparam>
		/// <param name="component">The object to destroy</param>
		/// <param name="time">The delay before the object is returned to the pool</param>
		public static void Destroy<T>(T component, float time) where T : Component
		{
			component.GetComponent<PoolableObject>().ReturnToPool(time);
		}

		/// <summary>
		/// Creates a new pool for a prefab
		/// </summary>
		/// <param name="prefab">The prefab to create a new pool for</param>
		/// <returns>The new pool for the prefab</returns>
		public static ObjectPool CreatePool(GameObject prefab)
		{
			return ObjectPool.Create(prefab);
		}

		/// <summary>
		/// Creates a new pool for a prefab
		/// </summary>
		/// <param name="prefab">The prefab to create a new pool for</param>
		/// <returns>The new pool for the prefab</returns>
		public static ObjectPool CreatePool(PoolableObject prefab)
		{
			return ObjectPool.Create(prefab);
		}

		/// <summary>
		/// Creates a new pool for a prefab
		/// </summary>
		/// <typeparam name="T">The type of component on the prefab</typeparam>
		/// <param name="prefab">The prefab to create a new pool for</param>
		/// <returns>The new pool for the prefab</returns>
		public static ObjectPool CreatePool<T>(T prefab) where T : Component
		{
			return ObjectPool.Create(prefab);
		}
	}
}
