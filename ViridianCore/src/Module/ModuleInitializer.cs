using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ViridianCore;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using ViridianCore.Hooks.Internal;
using UnityEngine;
using ViridianCore.Hooks.Utility;

using Logger = Modding.Logger;
using ViridianCore.Helpers;

internal static class ModuleInitializer
{
    static string LoadInfo = "";

    static List<(Type modType,MethodInfo starter)> ModStarters = new List<(Type, MethodInfo)>();
    static List<(Type modType, MethodInfo ender)> ModEnders = new List<(Type, MethodInfo)>();
    static HashSet<IMod> LoadedMods = new HashSet<IMod>();

    static List<(Type mod, Type hook)> HookTypes = new List<(Type, Type)>();
    static List<(Type mod, Type hook,Type alloc)> AllocatorHookTypes = new List<(Type, Type,Type)>();

    static List<(IMod mod,object hook)> Hooks = new List<(IMod,object)>();
    static List<(IMod mod,IHookBase hook,IAllocator allocator)> AllocatorHooks = new List<(IMod,IHookBase,IAllocator)>();


    public static void Initialize()
    {
        AppDomain.CurrentDomain.AssemblyResolve += BuiltInLoader.AssemblyLoader;
        AppDomain.CurrentDomain.AssemblyLoad += (s, a) => OnNewAssembly(a.LoadedAssembly, false);

        //Moves the resolve event from "Assembly-CSharp" to the back to prevent false errors
        try
        {
            var eventInfo = typeof(AppDomain).GetEvent("AssemblyResolve", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            var fieldInfo = typeof(AppDomain).GetField(eventInfo.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var dl = fieldInfo.GetValue(AppDomain.CurrentDomain) as Delegate;

            var InvoList = dl.GetInvocationList();

            for (int i = InvoList.Length - 1; i >= 0; i--)
            {
                var invo = InvoList[i];
                if (invo.Method.Module.Assembly.FullName.Contains("Assembly-CSharp"))
                {
                    eventInfo.RemoveEventHandler(AppDomain.CurrentDomain, invo);
                    LoadViridianLink();
                    eventInfo.AddEventHandler(AppDomain.CurrentDomain, invo);
                }
            }
        }
        catch (Exception)
        {
            LoadInfo += "Assembly-CSharp AssemblyResolve event is first;";
            LoadViridianLink();
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            OnNewAssembly(assembly, true);
        }

        ViridianCore.Helpers.Harmony.ViridianHarmonyInstance.PatchAll(typeof(ViridianCore.ViridianCore).Assembly);
        Logger.Log("Loaded ViridianCore Backend. Load Info : " + LoadInfo);
    }


    static void LoadViridianLink()
    {
        ViridianCore.Helpers.VirdianLinkLoader.Init();
    }

    static void OnNewAssembly(Assembly assembly, bool justStarted)
    {
        try
        {
            foreach (var type in assembly.GetTypes())
            {
                ModStartLoader(type);
                FindHooks(type);
                RunPatches(assembly,type);
            }
        }
        catch (ReflectionTypeLoadException)
        {
            
        }
        catch (Exception e)
        {
            Modding.Logger.LogError($"Failed to get ViridianCore attributes for the following assembly: {assembly.FullName}. Exception Details -> " + e);
        }
    }

    static void RunPatches(Assembly assembly, Type type)
    {
        if (!type.ContainsGenericParameters && !type.IsAbstract)
        {
            if (typeof(IMod).IsAssignableFrom(type))
            {
                var method = type.GetMethod("Initialize", new Type[] { typeof(Dictionary<string, Dictionary<string, GameObject>>) });
                if (method == null)
                {
                    method = type.GetMethod("Initialize", new Type[] { });
                }
                if (method == null)
                {
                    throw new Exception($"Could not find Initializer for mod {type}");
                }
                ViridianCore.Helpers.Harmony.ViridianHarmonyInstance.Patch(method, null, typeof(IModPatch).GetMethod("Postfix"));
                //Patch(method, null, typeof(IModPatch).GetMethod("Postfix"));
            }
            if (typeof(ITogglableMod).IsAssignableFrom(type))
            {
                var method = type.GetMethod("Unload", new Type[] { });
                if (method == null)
                {
                    throw new Exception($"Could not find Unloader for mod {type}");
                }
                else
                {
                    ViridianCore.Helpers.Harmony.ViridianHarmonyInstance.Patch(method, typeof(ITogglableModPatch).GetMethod("Prefix"), null);
                    //Patch(method, typeof(ITogglableModPatch).GetMethod("Prefix"),null);
                }
            }
        }
    }

    static void ModStartLoader(Type type)
    {
        foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var parameters = method.GetParameters();
            var paramAmount = parameters.GetLength(0);
            if (!method.IsAbstract && !method.ContainsGenericParameters && (paramAmount == 0 || (paramAmount == 1 && typeof(Modding.Mod).IsAssignableFrom(parameters[0].ParameterType))))
            {
                var modStarters = (ModStartAttribute[])method.GetCustomAttributes(typeof(ModStartAttribute), true);
                if (modStarters.GetLength(0) > 0)
                {
                    foreach (var modStarter in modStarters)
                    {
                        ModStarters.Add((modStarter.ModType, method));
                    }
                }
                var modEnders = (ModEndAttribute[])method.GetCustomAttributes(typeof(ModEndAttribute), true);
                if (modEnders.GetLength(0) > 0)
                {
                    foreach (var modEnder in modEnders)
                    {
                        ModEnders.Add((modEnder.ModType, method));
                    }
                }
            }
        }
    }


    static void CallStarters(IMod mod)
    {
        foreach (var pair in ModStarters)
        {
            //  pair.modType == typeof(object) || pair.modType == mod.GetType()
            if (pair.modType == mod.GetType() || pair.modType.IsAssignableFrom(mod.GetType()))
            {
                var parameters = pair.starter.GetParameters();
                if (parameters.GetLength(0) == 1)
                {
                    pair.starter.Invoke(null, new object[] { mod });
                }
                else
                {
                    pair.starter.Invoke(null, null);
                }
            }
        }
    }

    static void CallEnders(IMod mod)
    {
        foreach (var pair in ModEnders)
        {
            if (pair.modType == typeof(object) || pair.modType == mod.GetType())
            {
                var parameters = pair.ender.GetParameters();
                if (parameters.GetLength(0) == 1)
                {
                    pair.ender.Invoke(null, new object[] { mod });
                }
                else
                {
                    pair.ender.Invoke(null, null);
                }
            }
        }
    }

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

    class GenericCompare : IEqualityComparer<Type>
    {
        public bool Equals(Type x, Type y)
        {

            return x.IsGenericType && x.GetGenericTypeDefinition() == y;
        }

        public int GetHashCode(Type obj)
        {
            return obj.GetHashCode();
        }
    }



    static void FindHooks(Type type)
    {
        if (!type.ContainsGenericParameters && !type.IsAbstract)
        {
            var interfaces = type.GetInterfaces();
            Type inter = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHook<,>));
            if (inter != null)
            {
                var arguments = inter.GetGenericArguments();
                if (arguments.GetLength(0) == 2)
                {
                    AllocatorHookTypes.Add((arguments[0], type, arguments[1]));
                }
            }
            else
            {
                inter = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHook<>));
                if (inter != null)
                {
                    var arguments = inter.GetGenericArguments();
                    if (arguments.GetLength(0) == 1)
                    {
                        HookTypes.Add((arguments[0], type));
                    }
                }
            }
        }
    }

    static void LoadHooks(IMod mod)
    {
        var modType = mod.GetType();
        foreach (var hook in HookTypes)
        {
            if (hook.mod == modType)
            {
                var instance = (IHookBase)Activator.CreateInstance(hook.hook);
                Hooks.Add((mod,instance));
                instance.LoadHook(mod);
            }
        }
        foreach (var hook in AllocatorHookTypes)
        {
            if (hook.mod == modType)
            {
                var allocator = (IAllocator)Activator.CreateInstance(hook.alloc);
                var instance = allocator.Allocate(hook.hook,mod);
                AllocatorHooks.Add((mod, instance,allocator));
                if (instance != null)
                {
                    instance.LoadHook(mod);
                }
            }
        }
    }

    static void UnloadHooks(IMod mod)
    {
        var modType = mod.GetType();
        for (int i = Hooks.Count - 1; i >= 0; i--)
        {
            var hook = Hooks[i];
            if (hook.mod == mod)
            {
                (hook.hook as IHookBase).UnloadHook(mod);
                Hooks.Remove(hook);
            }
        }
        for (int i = AllocatorHooks.Count - 1; i >= 0; i--)
        {
            var hook = AllocatorHooks[i];
            if (hook.mod == mod)
            {
                hook.allocator.Deallocate(hook.hook, mod);
                if (hook.hook != null)
                {
                    (hook.hook as IHookBase).UnloadHook(mod);
                }
                AllocatorHooks.Remove(hook);
            }
        }
    }

    static class IModPatch
    {
        public static void Postfix(IMod __instance)
        {
            Modding.Logger.Log("IN IMOD POSTFIX");
            if (LoadedMods.Add(__instance))
            {
                CallStarters(__instance);
                LoadHooks(__instance);
            }
            Modding.Logger.Log("IMOD POSTFIX END");
        }
    }

    static class ITogglableModPatch
    {
        public static bool Prefix(ITogglableMod __instance)
        {
            if (__instance is IMod mod && LoadedMods.Contains(mod))
            {
                CallEnders(mod);
                LoadedMods.Remove(mod);
                UnloadHooks(mod);
            }
            return true;
        }
    }
}
