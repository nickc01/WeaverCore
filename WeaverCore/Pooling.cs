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

	public static class Pooling
	{
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

		public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
		{
			return GetPool(prefab).Instantiate(position, rotation, parent);
		}

		public static GameObject Instantiate(GameObject prefab, Transform parent)
		{
			return GetPool(prefab).Instantiate(parent);
		}

		public static GameObject Instantiate(GameObject prefab)
		{
			return GetPool(prefab).Instantiate();
		}

		public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			return GetPool(prefab).Instantiate(position, rotation);
		}

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

		public static T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
		{
			return GetPool(prefab.gameObject).Instantiate<T>(position, rotation, parent);
		}

		public static T Instantiate<T>(T prefab, Transform parent) where T : Component
		{
			return GetPool(prefab.gameObject).Instantiate<T>(parent);
		}

		public static T Instantiate<T>(T prefab) where T : Component
		{
			return GetPool(prefab.gameObject).Instantiate<T>(prefab.transform.position, prefab.transform.rotation, null);
		}

		public static T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
		{
			return GetPool(prefab.gameObject).Instantiate<T>(position, rotation, null);
		}

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

		public static void Destroy(GameObject gameObject)
		{
			gameObject.GetComponent<PoolableObject>().ReturnToPool();
		}

		public static void Destroy(PoolableObject poolableObject)
		{
			poolableObject.ReturnToPool();
		}

		public static void Destroy<T>(T component) where T : Component
		{
			Destroy(component.GetComponent<PoolableObject>());
		}

		public static void Destroy(GameObject gameObject, float time)
		{
			gameObject.GetComponent<PoolableObject>().ReturnToPool(time);
		}

		public static void Destroy(PoolableObject poolableObject, float time)
		{
			poolableObject.ReturnToPool(time);
		}

		public static void Destroy<T>(T component, float time) where T : Component
		{
			component.GetComponent<PoolableObject>().ReturnToPool(time);
		}

		public static ObjectPool CreatePool(GameObject prefab)
		{
			return ObjectPool.Create(prefab);
		}

		public static ObjectPool CreatePool(PoolableObject prefab)
		{
			return ObjectPool.Create(prefab);
		}

		public static ObjectPool CreatePool<T>(T prefab) where T : Component
		{
			return ObjectPool.Create(prefab);
		}
	}
}
