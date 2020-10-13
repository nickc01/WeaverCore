using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverCore.Attributes;
using WeaverCore.DataTypes;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;


/*namespace WeaverCore.Utilities
{
	public class RecyclerOLD : MonoBehaviour
	{
		static Recycler global;
		static Recycler inActiveScene;

		/// <summary>
		/// Gets the recycler that is not bound to a specific scene
		/// </summary>
		public static Recycler Global
		{
			get
			{
				if (global == null || global.gameObject == null)
				{
					global = CreateRecycler("Global Recycler");
					GameObject.DontDestroyOnLoad(global.gameObject);
				}
				return global;
			}
		}

		/// <summary>
		/// Gets the recycler that is bound to the active scene
		/// </summary>
		public static Recycler ActiveScene
		{
			get
			{
				if (inActiveScene == null || inActiveScene.gameObject == null)
				{
					inActiveScene = CreateRecycler("Active Scene Recycler");
				}
				return inActiveScene;
			}
		}



		public ObjectPool<T> CreatePool<T>(T objectToPool, uint amountToPool = 0, bool asynchronous = true) where T : class, IPoolableObject
		{
			return new ObjectPool<T>(this, objectToPool, amountToPool, asynchronous);
		}

		public static Recycler CreateRecycler(string name = "Recycler")
		{
			var instanceGM = new GameObject(name);
			return instanceGM.AddComponent<Recycler>();
		}
	}

	public class ObjectPoolOLD<T> where T : class, IPoolableObject
	{
		static HarmonyPatcher Patcher = HarmonyPatcher.Create("com.ObjectPool.patch");

		class InstanceInfo
		{
			public bool calledAwake = false;
			public bool calledStart = false;
		}


		Queue<T> pooledInstances = new Queue<T>();
		public T ObjectToPool { get; private set; }
		Recycler _recycler;
		public Recycler Recycler
		{
			get
			{
				return _recycler;
			}
			set
			{
				if (value != _recycler)
				{
					var previous = _recycler;
					_recycler = value;
					if (previous != null)
					{
						for (int i = 0; i < pooledInstances.Count; i++)
						{
							var instance = pooledInstances.Dequeue();
							instance.gameObject.transform.parent = _recycler.transform;
							pooledInstances.Enqueue(instance);
						}
					}
				}
			}
		}

		static bool functionsLoaded = false;
		static Action<T> AwakeFunction;
		static Action<T> StartFunction;
		static Action<T> UpdateFunction;
		static Action<T> LateUpdateFunction;
		static Dictionary<T, InstanceInfo> InfoMap = new Dictionary<T, InstanceInfo>();
		static Action<T, T> Copier;

		public ObjectPool(Recycler recycler, T objectToPool, uint amountToPool = 0, bool asynchronous = true)
		{
			if (objectToPool == null)
			{
				throw new Exception("There is no object set to pool");
			}
			this.Recycler = recycler;
			ObjectToPool = objectToPool;
			FillPool(amountToPool, asynchronous);
			//Debug.Log("<b>Functions Loaded = </b>" + functionsLoaded);
			if (!functionsLoaded)
			{
				var copyCreator = new FieldCopier<T>();
				foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
				{
					//Debug.Log("Field = " + field.Name);
					//Debug.Log("Is Value Type = " + field.FieldType.IsValueType);
					if (field.GetCustomAttributes(typeof(ExcludeFieldFromPoolAttribute), false).GetLength(0) == 0 && !field.IsInitOnly && !field.IsLiteral)
					{
						if (field.FieldType.IsValueType || field.FieldType.IsEnum)
						{
							//Debug.Log("Adding Field = " + field.Name);
							copyCreator.AddField(field);
						}
						else if (field.FieldType.IsClass && (field.IsPublic || field.GetCustomAttributes(typeof(SerializeField), true).GetLength(0) > 0))
						{
							var value = field.GetValue(ObjectToPool);
							if (value == null)
							{
								//Debug.Log("Adding Field = " + field.Name);
								copyCreator.AddField(field);
							}
							else
							{
								var valueType = value.GetType();
								if (value is UnityEngine.Object && !IsObjectRelated(value as UnityEngine.Object, ObjectToPool.gameObject))
								{
									//Debug.Log("Adding Field = " + field.Name);
									copyCreator.AddField(field);
								}
								else if (valueType.GetCustomAttributes(typeof(SerializableAttribute), false).GetLength(0) > 0)
								{
									//Debug.Log("Adding Field = " + field.Name);
									copyCreator.AddField(field);
								}
							}
						}
					}
				}
				Copier = copyCreator.Finish();
				//Debug.Log("Type to Patch = " + typeof(T).FullName);
				functionsLoaded = true;
				var awakeMethod = typeof(T).GetMethod("Awake", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
				var startMethod = typeof(T).GetMethod("Start", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
				var updateMethod = typeof(T).GetMethod("Update", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
				var lateUpdateMethod = typeof(T).GetMethod("LateUpdate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
				//var awakeMethod = typeof(T).GetMethod("Awake", new Type[] {  });
				//var startMethod = typeof(T).GetMethod("Start", new Type[] {  });
				var awakePostMethod = typeof(ObjectPool<T>).GetMethod("AwakePostfix", BindingFlags.Static | BindingFlags.NonPublic);
				var startPostMethod = typeof(ObjectPool<T>).GetMethod("StartPostfix", BindingFlags.Static | BindingFlags.NonPublic);
				var updatePreMethod = typeof(ObjectPool<T>).GetMethod("UpdatePrefix", BindingFlags.Static | BindingFlags.NonPublic);
				var lateUpdatePreMethod = typeof(ObjectPool<T>).GetMethod("LateUpdatePrefix", BindingFlags.Static | BindingFlags.NonPublic);

				//Debug.Log("Awake Method = " + awakeMethod);
				//Debug.Log("Start Method = " + startMethod);
				if (awakeMethod != null)
				{
					//Debug.Log("Awake Method Patched in class " + typeof(T));
					AwakeFunction = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), awakeMethod);
					Patcher.Patch(awakeMethod, null, awakePostMethod);
				}
				if (startMethod != null)
				{
					//Debug.Log("Start Method Patched in class " + typeof(T));
					StartFunction = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), startMethod);
					Patcher.Patch(startMethod, null, startPostMethod);
				}
				if (updateMethod != null)
				{
					UpdateFunction = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), updateMethod);
					Patcher.Patch(updateMethod, updatePreMethod, null);
				}

				if (lateUpdateMethod != null)
				{
					LateUpdateFunction = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), lateUpdateMethod);
					Patcher.Patch(lateUpdateMethod, lateUpdatePreMethod, null);
				}
			}
		}

		static void AwakePostfix(T __instance)
		{
			//if (__instance is T)
			//{
			var typedInstance = __instance;
			if (InfoMap.ContainsKey(typedInstance))
			{
				//Debug.Log("Awake Called in class " + __instance.GetType());
				InfoMap[typedInstance].calledAwake = true;
			}
			//}
		}

		static void StartPostfix(T __instance)
		{
			//if (__instance is T)
			//{
			var typedInstance = __instance;
			if (InfoMap.ContainsKey(typedInstance))
			{
				//Debug.Log("Start Called in class " + __instance.GetType());
				InfoMap[typedInstance].calledStart = true;
			}
			//}
		}

		static bool UpdatePrefix(T __instance)
		{
			var typedInstance = __instance;
			if (InfoMap.ContainsKey(typedInstance))
			{
				var info = InfoMap[typedInstance];
				if ((AwakeFunction != null && !info.calledAwake) || (StartFunction != null && !info.calledStart))
				{
					return false;
				}
			}
			return true;
		}

		static bool LateUpdatePrefix(T __instance)
		{
			var typedInstance = __instance;
			if (InfoMap.ContainsKey(typedInstance))
			{
				var info = InfoMap[typedInstance];
				if ((AwakeFunction != null && !info.calledAwake) || (StartFunction != null && !info.calledStart))
				{
					return false;
				}
			}
			return true;
		}

		static IEnumerator StartCaller(T instance)
		{
			yield return null;
			var rawInstance = (object)instance;


			if (Application.isPlaying)
			{
				bool calledStart = false;

				if (rawInstance != null && InfoMap.ContainsKey(instance))
				{
					calledStart = InfoMap[instance].calledStart;
				}

				if (StartFunction != null && !calledStart && instance != null)
				{
					StartFunction(instance);
				}

				if (rawInstance != null && InfoMap.ContainsKey(instance))
				{
					InfoMap.Remove(instance);
				}
			}

			//}
		}

		public void FillPool(uint amountToPool, bool asynchronous = false)
		{
			if (Recycler == null)
			{
				throw new Exception("This pool is not bound to a recycler");
			}
			if (ObjectToPool == null)
			{
				throw new Exception("There is no object set to pool");
			}
			if (asynchronous)
			{
				UnboundCoroutine.Start(AsyncFillPool(amountToPool));
			}
			else
			{
				for (uint i = 0; i < amountToPool; i++)
				{
					var instance = (T)UniversalInstantiate(ObjectToPool);
					instance.gameObject.name = ObjectToPool.gameObject.name + "(Pooled)";
					instance.gameObject.SetActive(false);
					instance.gameObject.transform.parent = Recycler.transform;
					pooledInstances.Enqueue(instance);
					InfoMap.Add(instance, new InstanceInfo());
				}
			}
		}

		public static ObjectPool<T> CreatePool(T objectToPool, ObjectPoolStorageType storageType, uint amountToPool = 0, bool asynchronous = true)
		{
			return new ObjectPool<T>(storageType == ObjectPoolStorageType.ActiveSceneOnly ? Recycler.ActiveScene : Recycler.Global, objectToPool, amountToPool, asynchronous);
		}

		public T RetrieveFromPool()
		{
			if (Recycler == null)
			{
				throw new Exception("This pool is not bound to a recycler");
			}
			if (ObjectToPool == null)
			{
				throw new Exception("There is no object set to pool");
			}
			return RetrieveFromPool(ObjectToPool.gameObject.transform.position, ObjectToPool.gameObject.transform.rotation);
		}

		public T RetrieveFromPool(Vector3 position, Quaternion rotation)
		{
			if (Recycler == null)
			{
				throw new Exception("This pool is not bound to a recycler");
			}
			if (ObjectToPool == null || ObjectToPool.gameObject == null)
			{
				throw new Exception("There is no object set to pool");
			}
			T instance = null;
			while (pooledInstances.Count > 0)
			{
				instance = pooledInstances.Dequeue();
				if (instance == null || instance.gameObject == null)
				{
					continue;
				}
				//WeaverLog.Log("Retrieve A");
				var instTransform = instance.gameObject.transform;
				//WeaverLog.Log("Retrieve B");
				instTransform.parent = null;
				instTransform.position = position;
				instTransform.rotation = rotation;
				//WeaverLog.Log("Retrieve C");
				instance.gameObject.SetActive(ObjectToPool.gameObject.activeSelf);
				//WeaverLog.Log("Retrieve D");
				if (InfoMap.ContainsKey(instance))
				{
					var info = InfoMap[instance];
					if (!info.calledAwake && AwakeFunction != null)
					{
						//Debug.Log("Called Awake");
						AwakeFunction.Invoke(instance);
					}
					UnboundCoroutine.Start(StartCaller(instance));
				}
				return instance;
			}
			instance = (T)UniversalInstantiate(ObjectToPool);
			//WeaverLog.Log("Retrieve E");
			instance.gameObject.name = ObjectToPool.gameObject.name + "(Pooled)";
			//WeaverLog.Log("Retrieve F");
			instance.gameObject.transform.position = position;
			instance.gameObject.transform.rotation = rotation;
			return instance;
		}

		public void ReturnToPool(T obj)
		{
			if (obj == null)
			{
				return;
			}
			if (Recycler == null || Recycler.gameObject == null)
			{
				//throw new Exception("This pool is not bound to a recycler");
				GameObject.Destroy(obj.gameObject);
				return;
			}
			if (InfoMap.ContainsKey(obj))
			{
				UnboundCoroutine.Start(ReturnToPoolAsync(obj));
				return;
			}
			if (obj is MonoBehaviour)
			{
				(obj as MonoBehaviour).StopAllCoroutines();
			}
			Copier(ObjectToPool, obj);
			obj.OnPool();
			obj.gameObject.SetActive(false);
			obj.gameObject.transform.parent = Recycler.transform;
			pooledInstances.Enqueue(obj);
			InfoMap.Add(obj, new InstanceInfo());
		}

		public void ReturnToPool(T obj, float time)
		{
			UnboundCoroutine.Start(ReturnToPoolAsync(obj, time));
		}

		IEnumerator ReturnToPoolAsync(T obj, float time)
		{
			for (float i = 0; i < time; i += Time.deltaTime)
			{
				yield return null;
			}
			if (Application.isPlaying && obj != null && Recycler != null && Recycler.gameObject != null)
			{
				ReturnToPool(obj);
			}
		}

		IEnumerator ReturnToPoolAsync(T obj)
		{
			yield return null;
			if (Application.isPlaying && obj != null && Recycler != null && Recycler.gameObject != null)
			{
				ReturnToPool(obj);
			}
		}

		IEnumerator AsyncFillPool(uint amountToPool)
		{
			yield return null;
			for (uint i = 0; Recycler != null && Recycler.gameObject != null && i < amountToPool; i++)
			{
				var instance = (T)UniversalInstantiate(ObjectToPool);
				instance.gameObject.name = ObjectToPool.gameObject.name + "(Pooled)";
				instance.gameObject.SetActive(false);
				instance.gameObject.transform.parent = Recycler.transform;
				pooledInstances.Enqueue(instance);
				InfoMap.Add(instance, new InstanceInfo());
				yield return null;
			}
		}

		static IPoolableObject UniversalInstantiate(IPoolableObject objectToInstantiate, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
		{
			if (rotation == default(Quaternion))
			{
				rotation = Quaternion.identity;
			}
			if (objectToInstantiate is Component)
			{
				return (IPoolableObject)GameObject.Instantiate(objectToInstantiate as Component, position, rotation);
			}
			else
			{
				return (IPoolableObject)GameObject.Instantiate(objectToInstantiate.gameObject, position, rotation).GetComponent(objectToInstantiate.GetType());
			}
		}


		static bool IsObjectRelated(UnityEngine.Object obj, GameObject relatedTo)
		{
			if (obj == null)
			{
				return false;
			}

			GameObject gameObject = null;
			if (obj is Component)
			{
				var component = obj as Component;
				gameObject = component.gameObject;
			}
			else if (obj is GameObject)
			{
				gameObject = obj as GameObject;
			}
			if (gameObject == null)
			{
				return false;
			}

			var transform = gameObject.transform;
			var relatedTransform = relatedTo.transform;

			return relatedTransform.IsChildOf(transform) || transform.IsChildOf(relatedTransform);
		}
	}
}*/