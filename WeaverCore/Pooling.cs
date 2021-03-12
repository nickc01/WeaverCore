using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore
{
	class PoolInfo
	{
		public ObjectPool Pool;
		public int timerIndex;
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
			void Awake()
			{
				StartCoroutine(CheckerRoutine());
			}

			IEnumerator CheckerRoutine()
			{
				while (true)
				{
					yield return null;
					for (int i = timers.Count - 1; i >= 0; i--)
					{
						timers[i].timer += Time.deltaTime;
						if (timers[i].timer >= MaxPoolTime)
						{
							var pool = pools[timers[i].obj];
							if (pool.Pool != null && pool.Pool.gameObject != null)
							{
								pool.Pool.ClearPool();
								Destroy(pool.Pool.gameObject);
							}
							pools.Remove(timers[i].obj);
							timers.RemoveAt(i);
						}
					}
				}
			}
		}

		const float MaxPoolTime = 60f * 5f;
		static TimerObject PoolTimerObject;
		static List<PoolTimer> timers = new List<PoolTimer>();
		static Dictionary<GameObject, PoolInfo> pools = new Dictionary<GameObject, PoolInfo>();

		static ObjectPool GetPool(GameObject obj)
		{
			if (!pools.ContainsKey(obj))
			{
				timers.Add(new PoolTimer
				{
					obj = obj,
					timer = 0f
				});

				var pool = ObjectPool.Create(obj);

				pools.Add(obj, new PoolInfo
				{
					Pool = pool,
					timerIndex = timers.Count - 1
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
			timers[info.timerIndex].timer = 0f;
			return info.Pool;
		}

		public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
		{
			return GetPool(prefab).Instantiate(position, rotation, parent);
			//return InstantiateInternal(position, rotation, parent).gameObject;
		}

		public static GameObject Instantiate(GameObject prefab, Transform parent)
		{
			return GetPool(prefab).Instantiate(parent);
			//return InstantiateInternal(Prefab.transform.position, Prefab.transform.rotation, parent).gameObject;
		}

		public static GameObject Instantiate(GameObject prefab)
		{
			return GetPool(prefab).Instantiate();
			//return InstantiateInternal(Prefab.transform.position, Prefab.transform.rotation, null).gameObject;
		}

		public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			return GetPool(prefab).Instantiate(position, rotation);
			//return InstantiateInternal(position, rotation, null).gameObject;
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
			/*var instance = InstantiateInternal(position, rotation, parent);
			var component = instance.GetCommonComponent<T>();
			if (component == null)
			{
				throw new Exception("The objects in this pool do not have a " + typeof(T).FullName + " component");
			}
			return component;*/
		}

		public static T Instantiate<T>(T prefab, Transform parent) where T : Component
		{
			return GetPool(prefab.gameObject).Instantiate<T>(parent);
			//return Instantiate<T>(Prefab.transform.position, Prefab.transform.rotation, parent);
		}

		public static T Instantiate<T>(T prefab) where T : Component
		{
			return GetPool(prefab.gameObject).Instantiate<T>(prefab.transform.position, prefab.transform.rotation, null);
			//return Instantiate<T>(Prefab.transform.position, Prefab.transform.rotation, null);
		}

		public static T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
		{
			return GetPool(prefab.gameObject).Instantiate<T>(position, rotation, null);
			//return Instantiate<T>(position, rotation, null);
		}

		public static T Instantiate<T>(T prefab, Transform parent, bool instantiateInWorldSpace) where T : Component
		{
			if (instantiateInWorldSpace)
			{
				return GetPool(prefab.gameObject).Instantiate<T>(prefab.transform.position, prefab.transform.rotation, parent);
				//return Instantiate<T>(Prefab.transform.position, Prefab.transform.rotation, parent);
			}
			else
			{
				return GetPool(prefab.gameObject).Instantiate<T>(prefab.transform.position + parent.transform.position, parent.transform.rotation * prefab.transform.rotation, parent);
				//return Instantiate<T>(Prefab.transform.position + parent.transform.position, parent.transform.rotation * Prefab.transform.rotation, parent);
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
			//return new ObjectPool(prefab);
			return ObjectPool.Create(prefab);
		}

		public static ObjectPool CreatePool(PoolableObject prefab)
		{
			//return new ObjectPool(prefab);
			return ObjectPool.Create(prefab);
		}

		public static ObjectPool CreatePool<T>(T prefab) where T : Component
		{
			//return new ObjectPool(prefab);
			return ObjectPool.Create(prefab);
		}
	}
}
