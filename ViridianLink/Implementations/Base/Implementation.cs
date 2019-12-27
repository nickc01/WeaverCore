using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ViridianLink
{
    public abstract class Implementation
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

        static void LoadImplementations()
        {
            if (FoundImplementations == null)
            {
                FoundImplementations = new List<Type>();

                State = GetState();
                if (State == RunningState.Editor)
                {
                    ImplAssembly = ResourceAssembly.Load("ViridianLink.Editor");
                }
                else
                {
                    try
                    {
                        ImplAssembly = ResourceAssembly.Load("ViridianLink.Game");
                    }
                    catch (Exception e)
                    {
                        throw new Exception("ERROR : ViridianLink requires the ViridianCore mod to be installed in the hollow knight mod directory to work. Please install it");
                    }
                }
                foreach (var type in ImplAssembly.GetTypes())
                {
                    if (typeof(Implementation).IsAssignableFrom(type) && !type.IsAbstract && !type.ContainsGenericParameters)
                    {
                        FoundImplementations.Add(type);
                    }
                }
            }
        }

        public static T GetImplementation<T>() where T : Implementation
        {
            var type = typeof(T);
            Type implType = null;
            LoadImplementations();
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
                return (T)Activator.CreateInstance(implType);
            }
        }
    }

}
