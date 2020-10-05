using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Internal;

namespace WeaverCore.Utilities
{
	public static class ImplFinder
    {
#if UNITY_EDITOR
        public const RunningState State = RunningState.Editor;
#else
        public const RunningState State = RunningState.Game;
#endif
        static Dictionary<Type, Type> Cache = new Dictionary<Type, Type>();
        static List<Type> FoundImplementations;
        static Assembly ImplAssembly;

        internal static bool Initialized { get; private set; }

        /*static RunningState GetState()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "UnityEditor")
                {
                    return RunningState.Editor;
                }
            }
            return RunningState.Game;
        }*/

       // internal static void Init()
        [OnInit(int.MinValue)]
        static void Initializer()
        {
            if (Initialized)
            {
                return;
            }
            Initialized = true;
           //Debug.LogError("D");
            //Debug.Log("Running ImplFinder");
            if (FoundImplementations == null)
            {
                //Debug.LogError("C");
                FoundImplementations = new List<Type>();
                //State = GetState();
                //try
                //{
#if UNITY_EDITOR
                    ImplAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "WeaverCore.Editor");
#else
                ImplAssembly = ResourceLoader.LoadAssembly("WeaverCore.Game");
#endif
                //}
                //catch (Exception e)
                //{
                    //Debug.LogError("ERROR = " + e);
                    //return;
                //}
                try
                {
                    foreach (var type in ImplAssembly.GetTypes())
                    {
                        if (typeof(IImplementation).IsAssignableFrom(type) && !type.IsAbstract && !type.ContainsGenericParameters)
                        {
                            FoundImplementations.Add(type);
                        }
                    }
                }
                catch (ReflectionTypeLoadException e)
                {
                    Debug.LogError("Failed Types Below");
                    //foreach (var type in e.Types)
                    for (int i = 0; i < e.Types.GetLength(0); i++)
                    {
                        var type = e.Types[i];
                        if (type == null)
                        {
                            Debug.LogError("Null");
                        }
                        else
                        {
                            Debug.Log(type.FullName);
                        }
                    }
                    foreach (var exception in e.LoaderExceptions)
                    {
                        Debug.LogWarning("Loader Exception = " + exception);
                    }
                }
            }
        }

        static ImplFinder()
        {
            Initializer();
        }

        public static Type GetImplementationType<T>() where T : IImplementation
        {
            var type = typeof(T);
            Type implType = null;
            //LoadImplementations();
            if (Cache.ContainsKey(type))
            {
                implType = Cache[type];
            }
            else
            {
                foreach (var foundType in FoundImplementations)
                {
                    if (typeof(T).IsAssignableFrom(foundType))
                    {
                        implType = foundType;
                        Cache.Add(type, implType);
                        break;
                    }
                }
            }
            if (implType == null)
            {
                throw new Exception("Implementation for " + typeof(T).FullName + " does not exist in " + ImplAssembly.FullName);
            }
            else
            {
                return implType;
            }
        }

        public static T GetImplementation<T>() where T : IImplementation
        {
            return (T)Activator.CreateInstance(GetImplementationType<T>());
        }
    }
}
