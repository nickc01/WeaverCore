using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore.DataTypes;
using WeaverCore.Utilities;

namespace WeaverCore
{
    /// <summary>
    /// When attached to an object, this object can now be used in a <see cref="ObjectPool"/>
    /// </summary>
    public sealed class PoolableObject : MonoBehaviour
    {
        /// <summary>
        /// Is the object currently in a pool?
        /// </summary>
        public bool InPool { get; internal set; }

        /// <summary>
        /// The pool the object was instantiated from
        /// </summary>
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

        /// <summary>
        /// Gets all components on the object, including all children
        /// </summary>
        internal ComponentPath[] GetAllComponents()
        {
            if (unserializedComponents == null)
            {
                unserializedComponents = RecursiveGetComponents(transform).ToArray();
            }
            return unserializedComponents;
        }

        /// <summary>
        /// Recursively gets all components on a transform, including all children
        /// </summary>
        /// <param name="t">The transform to get the components on</param>
        /// <returns>Returns a list of all the components on the object's hierarchy</returns>
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
                return RecursiveGetComponents(0, t, currentCache);
            }
            finally
            {
                cacheIsCurrentlyUsed = false;
            }
        }

        /// <summary>
        /// Recursively gets the components on an object's hiearchy
        /// </summary>
        /// <param name="SiblingHash">The hash to make it easier to identify components in the hierarchy</param>
        /// <param name="t">The transform to traverse</param>
        /// <param name="reusableList">A reusable list to help cache the result</param>
        /// <returns></returns>
        IEnumerable<ComponentPath> RecursiveGetComponents(int SiblingHash, Transform t, List<Component> reusableList)
        {
            t.GetComponents(ComponentType, reusableList);
            for (int i = 0; i < reusableList.Count; i++)
            {
                yield return new ComponentPath(SiblingHash, reusableList[i]);
            }
            for (int i = 0; i < t.childCount; i++)
            {
                foreach (var item in RecursiveGetComponents(Utilities.HashUtilities.CombineHashCodes(SiblingHash, i), t.GetChild(i), reusableList))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Returns the object to the pool is came from
        /// </summary>
        public void ReturnToPool()
        {
            UnboundCoroutine.Start(ReturnToPoolRoutine(this, 0f));
        }

        static IEnumerator ReturnToPoolRoutine(PoolableObject obj, float time)
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

        /// <summary>
        /// Returns the object to the pool it came from
        /// </summary>
        /// <param name="time">A time delay before it's returned to the pool</param>
        public void ReturnToPool(float time)
        {
            UnboundCoroutine.Start(ReturnToPoolRoutine(this, time));
        }


    }
}
