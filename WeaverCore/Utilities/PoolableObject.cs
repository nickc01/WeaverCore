using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore.DataTypes;

namespace WeaverCore.Utilities
{
	public sealed class PoolableObject : MonoBehaviour
	{
		public bool InPool { get; internal set; }
		public ObjectPool SourcePool { get; internal set; }

		static List<Component> cacheList = new List<Component>();
		static Type ComponentType = typeof(Component);
		static bool cacheIsCurrentlyUsed = false;

		[NonSerialized]
		Transform _transform;
		public new Transform transform
		{
			get
			{
				if (_transform == null)
				{
					_transform = GetComponent<Transform>();
				}
				return _transform;
			}
		}

		[NonSerialized]
		internal bool startCalledAutomatically = false;


		[NonSerialized]
		ComponentPath[] unserializedComponents = null;

		[NonSerialized]
		internal object CommonlyUsedComponent = null;

		internal T GetCommonComponent<T>()
		{
			if (CommonlyUsedComponent != null && CommonlyUsedComponent is T)
			{
				return (T)CommonlyUsedComponent;
			}
			else
			{
				var c = GetComponent<T>();
				CommonlyUsedComponent = c;
				return c;
			}
		}

		void Awake()
		{
			GetAllComponents();
		}

		internal ComponentPath[] GetAllComponents()
		{
			if (unserializedComponents == null)
			{
				unserializedComponents = RecursiveGetComponents(transform).ToArray();
			}
			return unserializedComponents;
		}

		internal IEnumerable<ComponentPath> RecursiveGetComponents(Transform t)
		{
			try
			{
				List<Component> currentCache = cacheList;
				if (cacheIsCurrentlyUsed)
				{
					currentCache = new List<Component>();
				}
				else
				{
					cacheIsCurrentlyUsed = true;
				}
				return RecursiveGetComponents(0,t, currentCache);
			}
			finally
			{
				cacheIsCurrentlyUsed = false;
			}
		}

		IEnumerable<ComponentPath> RecursiveGetComponents(int SiblingHash, Transform t, List<Component> reusableList)
		{
			t.GetComponents(ComponentType, reusableList);
			for (int i = 0; i < reusableList.Count; i++)
			{
				yield return new ComponentPath(SiblingHash, reusableList[i]);
			}
			for (int i = 0; i < t.childCount; i++)
			{
				foreach (var item in RecursiveGetComponents(HashUtilities.CombineHashCodes(SiblingHash,i),t.GetChild(i), reusableList))
				{
					yield return item;
				}
			}
		}

		public void ReturnToPool()
		{
			UnboundCoroutine.Start(ReturnToPoolRoutine(this, 0f));
			/*if (SourcePool != null)
			{
				SourcePool.ReturnToPool(this);
			}
			else
			{
				Debug.Log("DESTROY B");
				Destroy(gameObject);
			}*/
		}

		static IEnumerator ReturnToPoolRoutine(PoolableObject obj,float time)
		{
			yield return null;

			if (obj == null || obj.gameObject == null || obj.InPool)
			{
				yield break;
			}

			if (time > 0f)
			{
				if (obj.SourcePool != null)
				{
					obj.SourcePool.ReturnToPool(obj, time);
				}
				else
				{
					Destroy(obj.gameObject, time);
				}
			}
			else
			{
				if (obj.SourcePool != null)
				{
					obj.SourcePool.ReturnToPool(obj);
				}
				else
				{
					Destroy(obj.gameObject);
				}
			}
		}

		public void ReturnToPool(float time)
		{
			UnboundCoroutine.Start(ReturnToPoolRoutine(this, time));
			/*if (SourcePool != null)
			{
				SourcePool.ReturnToPool(this, time);
			}
			else
			{
				Debug.Log("DESTROY C");
				Destroy(gameObject, time);
			}*/
		}


	}
}
