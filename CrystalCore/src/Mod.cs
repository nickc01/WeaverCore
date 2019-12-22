using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Modding;
using UnityEngine.SceneManagement;
using CrystalCore;
using CrystalCore.Hooks;
using CrystalCore.Hooks.Internal;

/*namespace CrystalCore
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
            //var assembly = Assembly.GetAssembly(GetType());
            //Logger.Log("AA");
            //FindModStartFunctions(assembly);
           // Logger.Log("BB");
            //ModLog.Log("LOADING HOOKS");
            //Logger.Log("CC");
            //DoModStartFunctions(assembly);
            //Logger.Log("DD");
            //LoadHooks(assembly);
            //Logger.Log("EE");
        }

        private static void FindModStartFunctions(Assembly assembly)
        {
           //Logger.Log("AB");
            if (!FoundStartFunctions)
            {
               // Logger.Log("AC");
                //foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                //{
                //Logger.Log("AD");
                foreach (var type in assembly.GetTypes())
                {
                    //Logger.Log("AE");
                    foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                       // Logger.Log("AF");
                        //Logger.Log("Method = " + method.Name);
                        //Logger.Log("Type = " + type.Name);
                        //Logger.Log("Assembly = " + assembly);
                        //Logger.Log("MethodType = " + method.GetType());
                        if (method.Name.Contains("LoadResourceAudio"))
                        {
                            continue;
                        }
                        if (method.GetParameters().GetLength(0) == 0)
                        {
                            //Logger.Log("AG");
                            foreach (var attribute in (ModStartAttribute[])method.GetCustomAttributes(typeof(ModStartAttribute), true))
                            {
                                //Logger.Log("AH");
                                ModStartFunctions.Add((attribute.ModType, () => method.Invoke(null, null)));
                            }
                        }

                    }
                }
                //}
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
                                    Logger.Log("FOUND_HOOK1 = " + type);
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
                            Logger.Log("FOUND_HOOK2 = " + type);
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

*/