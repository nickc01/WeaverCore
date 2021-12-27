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
using WeaverCore.Utilities;

namespace WeaverCore
{
    /// <summary>
    /// Used for finding the implementation for a type that inherits from <see cref="IImplementation"/>.
    /// 
    /// This is used to load up a different type implementation depending on whether we are in-game, or in the Unity Editor
    /// </summary>
	public static class ImplFinder
    {
        static Dictionary<Type, Type> Cache = new Dictionary<Type, Type>();
        static List<Type> FoundImplementations;
        static Assembly ImplAssembly;

        static Assembly LoadAsmIfNotFound(string assemblyName)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
            if (assembly == null)
            {
                assembly = ResourceUtilities.LoadAssembly(assemblyName);
            }
            return assembly;
        }

        [OnInit(int.MinValue)]
        static void Initializer()
        {
            if (FoundImplementations == null)
            {
#if UNITY_EDITOR
                ImplAssembly = LoadAsmIfNotFound("WeaverCore.Editor");
#else
                ImplAssembly = LoadAsmIfNotFound("WeaverCore.Game");
#endif
                FoundImplementations = new List<Type>();

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

        /// <summary>
        /// Gets the implementation for a specific type
        /// </summary>
        /// <typeparam name="T">The type to get the implementation for</typeparam>
        /// <returns>Returns the implementation for the type</returns>
        /// <exception cref="Exception">Throws if an implemention for the type doesn't exist</exception>
        public static Type GetImplementationType<T>() where T : IImplementation
        {
            if (FoundImplementations == null)
            {
                Initializer();
            }
            var type = typeof(T);
            Type implType = null;
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

        /// <summary>
        /// Gets an instance of the implementation for a specific type
        /// </summary>
        /// <typeparam name="T">The type to get the implementation for</typeparam>
        /// <returns>Returns the implementation for the type</returns>
        public static T GetImplementation<T>() where T : IImplementation
        {
            return (T)Activator.CreateInstance(GetImplementationType<T>());
        }
    }
}
