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

internal static class ModuleInitializer
{
    delegate TResult Func<T1, T2, T3, T4, T5, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);


    static Func<Type, Type, bool> IsSubclassOfRawGeneric;

    static Func<List<string>> GetErrorList;
    static List<string> Errors => GetErrorList();

    static Func<List<IMod>> GetLoadedModList;
    static List<IMod> LoadedMods => GetLoadedModList();

    static Func<string, object> CreateHarmonyInstance;

    static Func<object, MethodBase, MethodInfo, MethodInfo, MethodInfo, DynamicMethod> Patch;


    static Assembly HarmonyAssembly;



    static Dictionary<Assembly, List<AssemblyName>> GetInvalidAssemblies()
    {
        //A list of invalid assemblies. It also contains the dependencies that need to be loaded in order for it to be valid
        var InvalidAssemblies = new Dictionary<Assembly, List<AssemblyName>>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            foreach (var dependency in assembly.GetReferencedAssemblies())
            {
                bool found = assemblies.Any(a => a.FullName == dependency.FullName);
                if (!found)
                {
                    if (!InvalidAssemblies.ContainsKey(assembly))
                    {
                        InvalidAssemblies.Add(assembly, new List<AssemblyName>());
                    }
                    InvalidAssemblies[assembly].Add(dependency);
                }
            }
        }
        return InvalidAssemblies;
    }

    static HashSet<string> LoadedAssemblies = new HashSet<string>();

    static void LoadAssembly(AssemblyName assemblyName,bool force = false)
    {
        try
        {
            if (!LoadedAssemblies.Contains(assemblyName.Name))
            {
                Logger.Log("LOADING MOD = " + assemblyName);
                //Logger.LogError("Loading = " + assemblyName);
                /*if (!force && AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == assemblyName.Name))
                {
                    return;
                }*/
                var assembly = AppDomain.CurrentDomain.Load(assemblyName);
                LoadedAssemblies.Add(assemblyName.Name);
                foreach (var dependency in assembly.GetReferencedAssemblies())
                {
                    LoadAssembly(dependency, false);
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError("LOAD EXCEPTION: " + e);
        }


    }

    static void RetryResolving()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            LoadAssembly(assembly.GetName(),true);
        }
        //var assemblies = GetInvalidAssemblies();

        //assemblies = GetInvalidAssemblies();
        /*foreach (var pair in assemblies)
        {
            Logger.Log("AInvalid Assembly = " + pair.Key);
            foreach (var dependency in pair.Value)
            {
                Logger.Log("ADependency = " + dependency);
            }
        }*/

        //assemblies = GetInvalidAssemblies();
        //for (int i = 0; i < 100; i++)
        //{
            /*var assemblies = GetInvalidAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var dependency in assembly.Value)
                {
                    //Logger.Log("LOADING = " + dependency);
                    try
                    {
                        
                    }
                    catch (Exception e)
                    {
                        //Logger.Log("Load Failed " + e);
                    }
                }
            }*/
            //Logger.Log("HASH = " + assemblies.GetHashCode());
        //}

        /*assemblies = GetInvalidAssemblies();

        foreach (var pair in assemblies)
        {
            Logger.Log("BInvalid Assembly = " + pair.Key);
            foreach (var dependency in pair.Value)
            {
                Logger.Log("BDependency = " + dependency);
            }
        }*/
    }

    public static void Initialize()
    {
        AppDomain.CurrentDomain.AssemblyResolve += BuiltInLoader.AssemblyLoader;
        Logger.Log("IN MODULE INITIALIZER");

        HarmonyAssembly = AppDomain.CurrentDomain.Load("0Harmony");

        SetupReflection();

        Logger.Log("MODLIST = " + LoadedMods);
        if (LoadedMods != null)
        {
            Logger.Log("MODLIST.COUNT = " + LoadedMods.Count);
        }

        foreach (var mods in LoadedMods)
        {
            Logger.Log("CURRENTLY LOADED MOD = " + mods.GetType());
        }

        object harmonyInstance = CreateHarmonyInstance("com.voidpreinit.nickc01");

        Logger.Log("INSTANCE MADE = " + (harmonyInstance != null));

        Patch(harmonyInstance,typeof(List<IMod>).GetMethod(nameof(List<IMod>.Add),BindingFlags.Public | BindingFlags.Instance),typeof(ModuleInitializer).GetMethod(nameof(ListPrefix),BindingFlags.NonPublic | BindingFlags.Static),null,null);
        //Patch(harmonyInstance, typeof(Assembly).GetMethod(nameof(Assembly.GetTypes), BindingFlags.Instance | BindingFlags.Public), typeof(ModuleInitializer).GetMethod(nameof(AssemblyTypePrefix), BindingFlags.NonPublic | BindingFlags.Static), null, null);

        RetryResolving();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Logger.LogError("ATTEMTPING ASSEMBLY = " + assembly);
            try
            {
                /*Type[] types = null;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    Logger.LogWarn($"Not all types from ({assembly.GetName().Name}) where able to load. Exception details -> {e}");
                    foreach (var exception in e.LoaderExceptions)
                    {
                        Logger.LogWarn(exception);
                        if (exception is System.TypeLoadException tEx)
                        {
                            foreach (var data in tEx.Data)
                            {
                                Logger.LogWarn("Data = " + data);
                            }
                            Logger.LogWarn("Inner Exception = " + tEx.InnerException);
                            Logger.LogWarn("Message = " + tEx.Message);
                            Logger.LogWarn("Source = " + tEx.Source);
                            Logger.LogWarn("Stack Trace = " + tEx.StackTrace);
                            //Logger.LogWarn("Data = " + tEx.Data);
                        }
                    }
                    types = e.Types;
                    Logger.LogError("NEW TYPES LOADED");
                }*/
                foreach (var module in assembly.GetLoadedModules())
                {
                    Logger.Log("LOADED MODULE = " + module);
                }

                foreach (var module in assembly.GetModules())
                {
                    Logger.Log("MODULE = " + module);
                }


                foreach (var type in assembly.GetTypes())
                {
                    //Logger.Log("TYPE = " + type);
                    if (IsSubclassOfRawGeneric(typeof(Mod<>), type) && !type.IsAbstract && !type.ContainsGenericParameters)
                    {
                        //Logger.LogError("FOUND MOD1 = " + type);
                        if (!LoadedMods.Any(v => v.GetType() == type))
                        {
                            //Logger.LogError("[VOIDINIT] - Trying to instantiate mod<T>: " + type);
                            IMod mod = Activator.CreateInstance(type) as IMod;
                            if (mod == null) continue;
                            LoadedMods.Add((Modding.Mod)mod);
                        }
                    }
                    else if (!type.IsGenericType && type.IsClass && typeof(Modding.Mod).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        //Logger.LogError("FOUND MOD2 = " + type);
                        if (!LoadedMods.Any(v => v.GetType() == type))
                        {
                            //Logger.LogError("[VOIDINIT] - Trying to instantiate mod: " + type);
                            Modding.Mod mod2 = type.GetConstructor(new Type[0])?.Invoke(new object[0]) as Modding.Mod;
                            if (mod2 == null) continue;
                            LoadedMods.Add(mod2);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("[VOIDAINIT] - Error: " + e);
                Errors.Add(string.Concat(assembly.Location, ": FAILED TO LOAD! Check ModLog.txt."));
            }
        }


        /*foreach (var type in typeof(Logger).Assembly.GetTypes())
        {
            if (type.FullName == "Modding.ModLoader")
            {
                Logger.Log("Mod Loader = " + type.FullName);
                foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    Logger.Log("Prop = " + prop.Name);
                }
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    Logger.Log("Field = " + field.Name);
                }
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    Logger.Log("Method = " + method.Name);
                    foreach (var param in method.GetParameters())
                    {
                        Logger.Log("Param = " + param.Name);
                    }
                }
            }
        }*/




        //RetryResolving();

        /*foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Logger.LogError("[VOIDINIT] - Trying to load assembly: " + assembly);
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    //Logger.LogError("TYPE = " + type);
                    //Logger.LogError("[VOIDINIT] - Success for " + assembly);
                    if (IsSubclassOfRawGeneric(typeof(Mod<>), type) && !type.IsAbstract)
                    {
                        Logger.LogError("FOUND MOD1 = " + type);
                        if (!LoadedMods.Any(v => v.GetType() == type))
                        {
                            Logger.LogError("[VOIDINIT] - Trying to instantiate mod<T>: " + type);
                            IMod mod = Activator.CreateInstance(type) as IMod;
                            if (mod == null) continue;
                            LoadedMods.Add((Modding.Mod)mod);
                        }
                    }
                    else if (!type.IsGenericType && type.IsClass && typeof(Modding.Mod).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        Logger.LogError("FOUND MOD2 = " + type);
                        if (!LoadedMods.Any(v => v.GetType() == type))
                        {
                            Logger.LogError("[VOIDINIT] - Trying to instantiate mod: " + type);
                            Modding.Mod mod2 = type.GetConstructor(new Type[0])?.Invoke(new object[0]) as Modding.Mod;
                            if (mod2 == null) continue;
                            LoadedMods.Add(mod2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("[VOIDAINIT] - Error: " + ex);
                Errors.Add(string.Concat(assembly.Location, ": FAILED TO LOAD! Check ModLog.txt."));
            }

        }*/
    }

    static void SetupReflection()
    {
        foreach (var type in typeof(Logger).Assembly.GetTypes())
        {
            if (type.FullName == "Modding.ModLoader")
            {
                var subMethod = type.GetMethod("IsSubclassOfRawGeneric", BindingFlags.Static | BindingFlags.NonPublic);
                IsSubclassOfRawGeneric = (generic, toCheck) => (bool)subMethod.Invoke(null, new object[] {generic,toCheck });

                var errorField = type.GetField("Errors", BindingFlags.NonPublic | BindingFlags.Static);
                GetErrorList = () => (List<string>)errorField.GetValue(null);

                var modField = type.GetField("LoadedMods", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                GetLoadedModList = () => (List<IMod>)modField.GetValue(null);

                break;
            }
        }
        Type HarmonyInstance = null;
        Type HarmonyMethod = null;
        foreach (var type in HarmonyAssembly.GetTypes())
        {
            if (type.FullName == "Harmony.HarmonyInstance")
            {
                Logger.Log("HARMONY TYPE = " + type);
                HarmonyInstance = type;
                var createMethod = type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);

                CreateHarmonyInstance = name => createMethod.Invoke(null, new object[] { name });
            }
            else if (type.FullName == "Harmony.HarmonyMethod")
            {
                Logger.Log("HARMONY TYPE 2 = " + type);
                HarmonyMethod = type;
            }
        }
        var patchMethod = HarmonyInstance.GetMethod("Patch", BindingFlags.Public | BindingFlags.Instance);



        Patch = (instance, original, prefix, postfix, transpiler) =>
        {
            List<object> Params = new List<object>();
            Params.Add(original);
            Logger.Log("A");
            Params.Add(Activator.CreateInstance(HarmonyMethod, new object[] { prefix }));
            Params.Add(Activator.CreateInstance(HarmonyMethod, new object[] { postfix }));
            Params.Add(Activator.CreateInstance(HarmonyMethod, new object[] { transpiler }));
            Logger.Log("D");
            return (DynamicMethod)patchMethod.Invoke(instance, Params.ToArray());
        };
    }


    static internal bool ListPrefix(List<IMod> __instance, IMod item)
    {
        if (__instance == LoadedMods)
        {
            if (LoadedMods.Any(m => m.GetType() == item.GetType()))
            {
                Logger.Log("LIST FAILURE");
                Logger.Log("ITEM = " + item);
                return false;
            }
            Logger.Log("LIST ADDITION");
            Logger.Log("ITEM = " + item);
        }
        return true;
    }


    /*static object mutex = new object();
    static bool CallOriginal = false;

    static internal bool AssemblyTypePrefix(Assembly __instance, ref Type[] __result, MethodInfo __originalMethod)
    {
        try
        {
            Monitor.Enter(mutex);
            if (!CallOriginal)
            {
                Logger.Log("CALLING ORIGINAL");
                CallOriginal = true;

                Logger.Log("ORIGINAL METHOD = " + __originalMethod);
                if (__originalMethod != null)
                {
                    Logger.Log("Name = " + __originalMethod.Name);
                    foreach (var param in __originalMethod.GetParameters())
                    {
                        Logger.Log("Parameter = " + param);
                    }
                }

                __result = (Type[])__originalMethod.Invoke(__instance, null);
            }
            else
            {
                Logger.Log("AT ORIGINAL");
                return true;
            }
        }
        catch(ReflectionTypeLoadException e)
        {
            Logger.Log("AT BACKUP");
            List<Type> NonNullList = new List<Type>();
            foreach (var type in e.Types)
            {
                if (type != null)
                {
                    NonNullList.Add(type);
                }
            }
            __result = NonNullList.ToArray();
        }
        catch(Exception e)
        {
            throw;
        }
        finally
        {
            CallOriginal = false;
            Monitor.Exit(mutex);
        }
        return false;
    }*/

}
