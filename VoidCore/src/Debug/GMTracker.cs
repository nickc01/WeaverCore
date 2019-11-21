using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoidCore;
using VoidCore.Debug;
using VoidCore.Hooks;

using Logger = Modding.Logger;

namespace VoidCore
{
    internal class TrackingStarter : GMTrackingHook<VoidCore>
    {
        static bool Patched = false;

        protected override void TrackingDisabled()
        {
            GMTracker.AllGameObjects.Clear();
        }

        protected override void TrackingEnabled()
        {
            if (!Patched)
            {
                foreach (var gameObject in UnityEngine.Object.FindObjectsOfType<GameObject>())
                {
                    GMTracker.AddObject(gameObject);
                }
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) =>
                {
                    foreach (var gameObject in scene.GetRootGameObjects())
                    {
                        AddObject(gameObject);
                    }
                };
                Patched = true;
                var harmony = ModuleInitializer.GetVoidCoreHarmony() as HarmonyInstance;
                foreach (var method in typeof(UnityEngine.Object).GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (method.Name.Contains("Instantiate"))
                    {
                        var selection = method;
                        if (selection.ContainsGenericParameters)
                        {
                            selection = selection.MakeGenericMethod(typeof(UnityEngine.Object));
                        }
                        harmony.Patch(selection, null, new HarmonyMethod(typeof(GMTrackingPatches).GetMethod(nameof(GMTrackingPatches.InstantiatePostfix))));

                    }
                }
            }
        }
        public static void AddObject(GameObject gm)
        {
            if (Settings.GMTracking)
            {
                GMTracker.AddObject(gm);
                if (gm.transform.childCount > 0)
                {
                    for (int i = 0; i < gm.transform.childCount; i++)
                    {
                        AddObject(gm.transform.GetChild(i).gameObject);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Keeps track of all the objects in the game
    /// </summary>
    public static class GMTracker
    {
        /*class QuickHashSetIter<T> : IEnumerator<T>
        {
            HashSet<T> hashSet;
            int index;
            T current;
            int count;

            Func<int, int> GetLinkHashCode;
            Func<T[]> Slots;

            Func<HashSet<T>, T[]> slots_internal;

            IEqualityComparer<T> comparer = EqualityComparer<T>.Default;


            public QuickHashSetIter(HashSet<T> set)
            {
                hashSet = set;
                index = hashSet.Count - 1;
                count = hashSet.Count;
                GetLinkHashCode = (Func<int,int>)Delegate.CreateDelegate(typeof(Func<int, int>), hashSet, typeof(HashSet<T>).GetMethod("GetLinkHashCode",BindingFlags.NonPublic | BindingFlags.Instance));
                slots_internal = CreateGetter<HashSet<T>, T[]>(typeof(HashSet<T>).GetField("slots",BindingFlags.Instance | BindingFlags.NonPublic));
                Slots = () => slots_internal(hashSet);
                while (index >= 0)
                {
                    if (GetLinkHashCode(index) != 0)
                    {
                        current = Slots()[index];
                        break;
                    }
                    else
                    {
                        index--;
                    }
                }
            }

            T IEnumerator<T>.Current => current;

            object IEnumerator.Current => current;

            void IDisposable.Dispose() { }

            bool IEnumerator.MoveNext()
            {
                if (index < 0)
                {
                    return false;
                }
                else
                {
                    if (count != hashSet.Count)
                    {
                        count = hashSet.Count;
                        if (index > count)
                        {
                            index = count - 1;
                        }
                        if (count == 0)
                        {
                            return false;
                        }
                        if (GetLinkHashCode(index) != 0 && comparer.Equals(Slots()[index],current))
                        {
                            index--;
                        }
                    }
                    while (index >= 0)
                    {
                        //Logger.Log("T");
                        if (GetLinkHashCode(index) != 0)
                        {
                            //Logger.Log("S");
                            current = Slots()[index];
                            index--;
                            //Logger.Log("INDEX = " + index);
                            return true;
                        }
                        else
                        {
                            //Logger.Log("R");
                            index--;
                        }
                    }
                }
                index = -1;
                return false;
            }

            void IEnumerator.Reset()
            {
                index = hashSet.Count - 1;
            }

            static Func<S, T> CreateGetter<S, T>(FieldInfo field)
            {
                string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
                DynamicMethod setterMethod = new DynamicMethod(methodName, typeof(T), new Type[1] { typeof(S) }, true);
                ILGenerator gen = setterMethod.GetILGenerator();
                if (field.IsStatic)
                {
                    gen.Emit(OpCodes.Ldsfld, field);
                }
                else
                {
                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldfld, field);
                }
                gen.Emit(OpCodes.Ret);
                return (Func<S, T>)setterMethod.CreateDelegate(typeof(Func<S, T>));
            }
        }

        class QuickHashSetEnum<T> : IEnumerable<T>
        {
            HashSet<T> hashSet;

            public QuickHashSetEnum(HashSet<T> set)
            {
                hashSet = set;
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return new QuickHashSetIter<T>(hashSet);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new QuickHashSetIter<T>(hashSet);
            }
        }*/

        //internal static HashSet<GameObject> AllGameObjects = new HashSet<GameObject>();
        internal static List<GameObject> AllGameObjects = new List<GameObject>();

        /// <summary>
        /// Returns a list of all the enabled and active objects in the game
        /// </summary>
        public static IEnumerable<GameObject> ActiveGameObjects
        {
            get
            {
                int top = AllGameObjects.Count - 1;
                while (top >= 0)
                {
                    yield return AllGameObjects[top];
                    top--;
                    if (top > AllGameObjects.Count - 1)
                    {
                        top = AllGameObjects.Count - 1;
                    }
                }
            }
        }

        class Tracker : MonoBehaviour
        {
            bool Added = false;

            void Awake()
            {
                if (!Added)
                {
                    Added = true;
                    //Modding.Logger.Log("AWAKE = " + gameObject.name);
                    AllGameObjects.Add(gameObject);
                    Events.GameObjectCreated.Invoker?.Invoke(gameObject, true);
                }
            }

            void Start()
            {
                if (!Added)
                {
                    Added = true;
                    //Modding.Logger.Log("START = " + gameObject.name);
                    AllGameObjects.Add(gameObject);
                    Events.GameObjectCreated.Invoker?.Invoke(gameObject, true);
                }
            }

            void OnEnable()
            {
                if (!Added)
                {
                    Added = true;
                    //Modding.Logger.Log("ENABLED = " + gameObject.name);
                    AllGameObjects.Add(gameObject);
                    Events.GameObjectCreated.Invoker?.Invoke(gameObject, false);
                }
            }

            void OnDisable()
            {
                if (Added)
                {

                    Added = false;
                    RemoveObject(gameObject);
                    Events.GameObjectRemoved.Invoker?.Invoke(gameObject, true);
                }
            }

            void OnDestroy()
            {
                if (Added)
                {
                    Added = false;
                    //Modding.Logger.Log("DESTROYED = " + gameObject.name);
                    RemoveObject(gameObject);
                    Events.GameObjectRemoved.Invoker?.Invoke(gameObject, false);
                }
            }
        }

        internal static void AddObject(GameObject g)
        {
            if (Settings.GMTracking && g.GetComponent<Tracker>() == null)
            {
                g.AddComponent<Tracker>();
            }
        }

        internal static void RemoveObject(GameObject g)
        {
            AllGameObjects.Remove(g);
        }
    }
}
