using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Modding;
using UnityEngine.SceneManagement;
using VoidCore;
using VoidCore.Hooks;
using VoidCore.Hooks.Utility;

namespace VoidCore
{
    /// <summary>
    /// Inherit this class to create a mod of your own
    /// </summary>
    /// <example>
    /// <code>
    /// public class NewMod : Mod
    /// {
    ///     public override void Initialize()
    ///     {
    ///         //Initialization Code Here
    ///         base.Initialize();
    ///     }
    ///     
    ///     public override string GetVersion() => "1.0.0";
    /// }
    /// </code>
    /// </example>
    public abstract class Mod : Modding.Mod
    {


        static Dictionary<Assembly, bool> HooksLoaded = new Dictionary<Assembly, bool>();

        List<IHook> Hooks = new List<IHook>();

        static List<(Type modType,Action executer)> ModStartFunctions = new List<(Type, Action)>();
        static bool FoundStartFunctions = false;


        /// <summary>
        /// Called when the mod is initalized.
        /// </summary>
        /// <remarks>
        /// Be sure to call the base method, or else hooks will not be loaded
        /// </remarks>
        /// <example>
        /// <code>
        /// public override void Initialize()
        /// {
        ///     //Initialization Code Here
        ///     base.Initialize();
        /// }
        /// </code>
        /// </example>
        public override void Initialize()
        {
            FindModStartFunctions();
            ModLog.Log("LOADING HOOKS");
            var assembly = Assembly.GetAssembly(GetType());
            DoModStartFunctions(assembly);
            LoadHooks(assembly);
        }

        private static void FindModStartFunctions()
        {
            if (!FoundStartFunctions)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        {
                            if (method.GetParameters().GetLength(0) == 0)
                            {
                                foreach (var attribute in (OnModStartAttribute[])method.GetCustomAttributes(typeof(OnModStartAttribute), true))
                                {
                                    ModStartFunctions.Add((attribute.ModType, () => method.Invoke(null, null)));
                                }
                            }

                        }
                    }
                }
                FoundStartFunctions = true;
            }
        }


        /// <summary>
        /// Gets the version of the mod
        /// </summary>
        /// <example>
        /// <code>
        /// public override string GetVersion() => "1.0.0";
        /// </code>
        /// </example>
        /// <returns>Returns the version of the mod</returns>
        public abstract override string GetVersion();

        static bool InheritsGeneric(Type type, Type genericType)
        {
            foreach (var inter in type.GetInterfaces())
            {
                if (inter.IsGenericType && inter.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }
            }
            return false;
        }


        private void DoModStartFunctions(Assembly assembly)
        {
            var modType = GetType();
            for (int i = ModStartFunctions.Count - 1; i >= 0; i--)
            {
                var pair = ModStartFunctions[i];
                if (pair.modType == typeof(object) || modType.IsAssignableFrom(pair.modType))
                {
                    ModStartFunctions.RemoveAt(i);
                    pair.executer();
                }
            }
            /*foreach (var pair in ModStartFunctions)
            {
                if (modType.IsAssignableFrom(pair.Key))
                {

                }
            }*/
            /*if (!HooksLoaded.ContainsKey(assembly) || HooksLoaded[assembly] == false)
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (method.GetParameters().GetLength(0) == 0 && method.GetCustomAttributes(typeof(OnModStartAttribute), false).GetLength(0) > 0)
                        {
                            method.Invoke(null, null);
                        }
                        
                    }
                }
            }*/
        }


        private void LoadHooks(Assembly assembly)
        {
            if (!HooksLoaded.ContainsKey(assembly))
            {
                HooksLoaded.Add(assembly, false);
            }
            if (HooksLoaded[assembly] == false)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsAbstract)
                    {
                        //type.IsSubclassOf(typeof(IHook<>))
                        if (InheritsGeneric(type,typeof(IHook<>)))
                        {
                            foreach (var inter in type.GetInterfaces())
                            {
                                if (inter.IsGenericType && inter.GetGenericTypeDefinition() == typeof(IHook<>))
                                {
                                    var allocatorType = inter.GetGenericArguments()[0];
                                    var allocator = (Allocator)Activator.CreateInstance(allocatorType);
                                    var hook = (IHook)allocator.Allocate(type);
                                    if (hook != null)
                                    {
                                        Hooks.Add(hook);
                                        hook.LoadHook();
                                    }
                                }
                            }

                        }
                        //type.IsSubclassOf(typeof(IHook))
                        else if (typeof(IHook).IsAssignableFrom(type))
                        {
                            var hook = (IHook)Activator.CreateInstance(type);
                            Hooks.Add(hook);
                            hook.LoadHook();
                        }
                    }
                }
                HooksLoaded[assembly] = true;
            }
        }

    }
}

