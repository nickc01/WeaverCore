using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Internal;

namespace WeaverCore.Utilities
{
	public static class ImplFinder
    {
        public static RunningState State { get; private set; }

        static Dictionary<Type, Type> Cache = new Dictionary<Type, Type>();
        static List<Type> FoundImplementations;
        static Assembly ImplAssembly;

        static RunningState GetState()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "UnityEditor")
                {
                    return RunningState.Editor;
                }
            }
            return RunningState.Game;
        }

        internal static void Init()
        {
            if (FoundImplementations == null)
            {
                FoundImplementations = new List<Type>();
                State = GetState();
                if (State == RunningState.Editor)
                {
                    //ImplAssembly = ResourceLoader.LoadAssembly($"{nameof(WeaverCore)}.Editor");
                    /*foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        Debug.Log("Assembly = " + assembly);
                    }*/
                    ImplAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "WeaverCore.Editor");
                }
                else
                {
                    ImplAssembly = ResourceLoader.LoadAssembly($"{nameof(WeaverCore)}.Game");
                }
                foreach (var type in ImplAssembly.GetTypes())
                {
                    if (typeof(IImplementation).IsAssignableFrom(type) && !type.IsAbstract && !type.ContainsGenericParameters)
                    {
                        FoundImplementations.Add(type);
                    }
                }
            }
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
                throw new Exception($"Implementation for {typeof(T).FullName} does not exist in {ImplAssembly.FullName}");
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
