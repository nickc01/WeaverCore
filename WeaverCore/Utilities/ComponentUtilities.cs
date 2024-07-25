using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WeaverCore.Utilities
{
    public static class ComponentUtilities
	{
		public static IEnumerable<T> GetComponentsInChildrenList<T>(IEnumerable<GameObject> rootObjects)
		{
            foreach (var gm in rootObjects)
            {
                foreach (var c in GetComponentsInChildren<T>(gm.transform))
                {
                    yield return c;
                }
            }
		}

        public static IEnumerable<T> GetComponentsInChildren<T>(Component obj)
        {
            if (obj.TryGetComponent<T>(out var c))
            {
                yield return c;
            }

            var transform = obj.transform;

            for (int i = 0; i < transform.childCount; i++)
            {
                foreach (var c2 in GetComponentsInChildren<T>(transform.GetChild(i)))
                {
                    yield return c2;
                }
            }
        }

        public static T GetComponentInChildrenList<T>(IEnumerable<GameObject> rootObjects)
        {
            return GetComponentsInChildrenList<T>(rootObjects).FirstOrDefault();
        }

        public static T GetComponentInChildren<T>(Component obj)
        {
            return GetComponentsInChildren<T>(obj).FirstOrDefault();
        }



        public static IEnumerable<Component> GetComponentsInChildrenList(Type componentType, IEnumerable<GameObject> rootObjects)
		{
            foreach (var gm in rootObjects)
            {
                foreach (var c in GetComponentsInChildren(componentType, gm.transform))
                {
                    yield return c;
                }
            }
		}

        public static IEnumerable<Component> GetComponentsInChildren(Type componentType, Component obj)
        {
            if (obj.TryGetComponent(componentType, out var c))
            {
                yield return c;
            }

            var transform = obj.transform;

            for (int i = 0; i < transform.childCount; i++)
            {
                foreach (var c2 in GetComponentsInChildren(componentType, transform.GetChild(i)))
                {
                    yield return c2;
                }
            }
        }

        public static Component GetComponentInChildrenList(Type componentType, IEnumerable<GameObject> rootObjects)
        {
            return GetComponentsInChildrenList(componentType, rootObjects).FirstOrDefault();
        }

        public static Component GetComponentInChildren(Type componentType, Component obj)
        {
            return GetComponentsInChildren(componentType, obj).FirstOrDefault();
        }
    }
}
