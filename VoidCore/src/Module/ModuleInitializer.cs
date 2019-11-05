using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VoidCore;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using VoidCore.Hooks.Utility;
using Harmony;
using UnityEngine;

internal static class ModuleInitializer
{
    static Dictionary<Type, MethodInfo> ModStarters = new Dictionary<Type, MethodInfo>();
    static HashSet<IMod> LoadedMods = new HashSet<IMod>();

    static List<IHook> Hooks = new List<IHook>();


    public static void Initialize()
    {
        AppDomain.CurrentDomain.AssemblyResolve += BuiltInLoader.AssemblyLoader;
        AppDomain.CurrentDomain.AssemblyLoad += (s, a) => OnNewAssembly(a.LoadedAssembly, false);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            OnNewAssembly(assembly, true);
        }
        UpdateStarters();

        //Load Harmony and run all patches
        var harmonyAssembly = Assembly.Load("0Harmony");
        Type harmonyInstance = harmonyAssembly.GetType("HarmonyInstance");

        var createMethod = harmonyInstance.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);

        var harmony = createMethod.Invoke(null, new object[] { "com." + nameof(VoidCore.VoidCore).ToLower() + ".nickc01" });

        var patchAll = harmonyInstance.GetMethod("PatchAll", new Type[] { typeof(Assembly) });
        patchAll.Invoke(harmony, new object[] { typeof(VoidCore.VoidCore).Assembly });

    }







    static void OnNewAssembly(Assembly assembly, bool justStarted)
    {
        try
        {
            foreach (var type in assembly.GetTypes())
            {
                ModStartLoader(type);
                HookLoader(type);
            }
            if (!justStarted)
            {
                UpdateStarters();
            }
        }
        catch (Exception e)
        {
            Modding.Logger.LogError($"Failed to get VoidCore attributes for Assembly {assembly.FullName}. Exception Details -> " + e);
        }
    }

    static void ModStartLoader(Type type)
    {
        foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.NonPublic))
        {
            var parameters = method.GetParameters();
            var paramAmount = parameters.GetLength(0);
            if (paramAmount == 0 || (paramAmount == 1 && typeof(Modding.Mod).IsAssignableFrom(parameters[1].ParameterType)))
            {
                var modStarters = (ModStartAttribute[])method.GetCustomAttributes(typeof(ModStartAttribute), true);
                if (modStarters.GetLength(0) > 0)
                {
                    foreach (var modStarter in modStarters)
                    {
                        ModStarters.Add(modStarter.ModType, method);
                    }
                }
            }
        }
    }


    static void UpdateStarters()
    {
        foreach (var mod in LoadedMods)
        {
            MethodInfo method = null;
            if (ModStarters.TryGetValue(mod.GetType(),out method))
            {
                var parameters = method.GetParameters();
                if (parameters.GetLength(0) == 1)
                {
                    method.Invoke(null, new object[] { mod });
                }
                else
                {
                    method.Invoke(null, null);
                }
                ModStarters.Remove()
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

    static void HookLoader(Type type)
    {
        if (!type.IsAbstract)
        {
            if (InheritsGeneric(type, typeof(IHook<>)))
            {
                foreach (var inter in type.GetInterfaces())
                {
                    if (inter.IsGenericType && inter.GetGenericTypeDefinition() == typeof(IHook<>))
                    {
                        var allocatorType = inter.GetGenericArguments()[0];
                        var allocator = (Allocator)Activator.CreateInstance(allocatorType);
                        try
                        {
                            var hook = (IHook)allocator.Allocate(type);
                            if (hook != null)
                            {
                                Hooks.Add(hook);
                                hook.LoadHook();
                            }
                        }
                        catch (Exception e)
                        {
                            Modding.Logger.LogError($"Failed to load hook of type {type}. Exception Details -> " + e);
                        }
                    }
                }

            }
            else if (typeof(IHook).IsAssignableFrom(type))
            {
                try
                {
                    var hook = (IHook)Activator.CreateInstance(type);
                    Hooks.Add(hook);
                    hook.LoadHook();
                }
                catch (Exception e)
                {
                    Modding.Logger.LogError($"Failed to load hook of type {type}. Exception Details -> " + e);
                }
            }
        }
    }

    //[HarmonyPatch(typeof(IMod))]
   // [HarmonyPatch("Initialize")]
    //[HarmonyPatch(new Type[]{})]
    [HarmonyPatch()]
    static class IModPatch
    {
        static MethodInfo TargetMethod()
        {
            try
            {
                return typeof(IMod).GetMethod("Initialize", new Type[] {typeof(Dictionary<string, Dictionary<string, GameObject>>) });
            }
            catch(Exception)
            {
                return typeof(IMod).GetMethod("Initialize", new Type[] { });
            }
        }

        static bool Prefix()
        {
            return true;
        }

        static void Postfix(IMod __instance)
        {
            if (LoadedMods.Add(__instance))
            {
                UpdateStarters();
            }
        }
    }
}
