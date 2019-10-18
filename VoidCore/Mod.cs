using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Modding;
using VoidCore.Hooks;

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
            VoidModLog.Log("LOADING HOOKS");
            LoadHooks(Assembly.GetAssembly(GetType()));
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

        /*private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }*/


        private void LoadHooks(Assembly assembly)
        {
            if (!HooksLoaded.ContainsKey(assembly))
            {
                HooksLoaded.Add(assembly, false);
            }
            VoidModLog.Log("LOAD TESTA");
            if (HooksLoaded[assembly] == false)
            {
                foreach (var type in assembly.GetTypes())
                {
                    VoidModLog.Log("POTENTIAL HOOK = " + type);
                    VoidModLog.Log("INHERITS IHOOK = " + typeof(IHook).IsAssignableFrom(type));
                    VoidModLog.Log("INHERITS IHOOK Generic = " + InheritsGeneric(type, typeof(IHook<>)));
                    foreach (var inter in type.GetInterfaces())
                    {
                        VoidModLog.Log("INTERFACE = " + inter);
                    }
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
                                    VoidModLog.Log("STARTING HOOK2 = " + type.FullName);
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
                            VoidModLog.Log("STARTING HOOK = " + type.FullName);
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
