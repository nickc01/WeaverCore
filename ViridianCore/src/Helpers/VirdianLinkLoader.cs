using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ViridianCore.Helpers
{

    public static class VirdianLinkLoader
    {
        public static IMod CreateViridianMod(object vMod)
        {
            var modHandle = typeof(ViridianMod<>).MakeGenericType(vMod.GetType());
            return (IMod)modHandle.GetMethod("CreateMod", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { vMod });
        }

        static Type ModLoaderType = typeof(IMod).Assembly.GetType("Modding.ModLoader");

        static FieldInfo LoadedMods = ModLoaderType.GetField("LoadedMods", BindingFlags.Public | BindingFlags.Static);

        public class ViridianMod<T> : IMod
        {
            protected object vMod;
            protected Type modType;
            protected MethodInfo GetNameFunc;
            protected MethodInfo GetVersionFunc;
            protected MethodInfo GetLoadPriorityFunc;
            protected MethodInfo GetUnloadableFunc;
            protected MethodInfo LoadFunc;

            internal ViridianMod(object mod)
            {
                vMod = mod;
                modType = mod.GetType();
                var mapping = modType.GetInterfaceMap(VModType);
                var methods = mapping.TargetMethods;
                GetNameFunc = methods.First(m => m.Name == "get_Name");
                GetVersionFunc = methods.First(m => m.Name == "get_Version");
                GetLoadPriorityFunc = methods.First(m => m.Name == "get_LoadPriority");
                GetUnloadableFunc = methods.First(m => m.Name == "get_Unloadable");
                LoadFunc = methods.First(m => m.Name == "Load");
            }

            public static ViridianMod<T> CreateMod(object mod)
            {
                var modType = mod.GetType();
                var mapping = modType.GetInterfaceMap(VModType);
                var methods = mapping.TargetMethods;

                var GetUnloadable = methods.First(m => m.Name == "get_Unloadable");

                if ((bool)GetUnloadable.Invoke(mod,null) == true)
                {
                    return new UnloadableViridianMod<T>(mod);
                }
                else
                {
                    return new ViridianMod<T>(mod);
                }
            }

            public string GetName()
            {
                return (string)GetNameFunc.Invoke(vMod, null);
            }

            public List<(string, string)> GetPreloadNames()
            {
                return null;
            }

            public string GetVersion()
            {
                return (string)GetVersionFunc.Invoke(vMod, null);
            }

            public void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
            {
                Modding.Logger.Log($"Loading Viridian Mod: {GetName()} Version: {GetVersion()}");
                LoadFunc.Invoke(vMod, null);
            }

            public bool IsCurrent()
            {
                return true;
            }

            public int LoadPriority()
            {
                return (int)GetLoadPriorityFunc.Invoke(vMod, null);
            }

            public void Log(string message)
            {
                Modding.Logger.Log(VModType.Name + " : " + message);
            }

            public void Log(object message)
            {
                Modding.Logger.Log(VModType.Name + " : " + message);
            }

            public void LogDebug(string message)
            {
                Modding.Logger.LogDebug(VModType.Name + " : " + message);
            }

            public void LogDebug(object message)
            {
                Modding.Logger.LogDebug(VModType.Name + " : " + message);
            }

            public void LogError(string message)
            {
                Modding.Logger.LogError(VModType.Name + " : " + message);
            }

            public void LogError(object message)
            {
                Modding.Logger.LogError(VModType.Name + " : " + message);
            }

            public void LogFine(string message)
            {
                Modding.Logger.LogFine(VModType.Name + " : " + message);
            }

            public void LogFine(object message)
            {
                Modding.Logger.LogFine(VModType.Name + " : " + message);
            }

            public void LogWarn(string message)
            {
                Modding.Logger.LogWarn(VModType.Name + " : " + message);
            }

            public void LogWarn(object message)
            {
                Modding.Logger.LogWarn(VModType.Name + " : " + message);
            }
        }

        public class UnloadableViridianMod<T> : ViridianMod<T>, ITogglableMod
        {
            MethodInfo UnloadFunc;

            internal UnloadableViridianMod(object vMod) : base(vMod)
            {
                var mapping = modType.GetInterfaceMap(VModType);
                var methods = mapping.TargetMethods;

                UnloadFunc = methods.First(m => m.Name == "Unload");
            }

            public void Unload()
            {
                UnloadFunc.Invoke(vMod, null);
            }
        }


        static bool loaded = false;
        static Assembly ViridianASM;
        static Type VModType;

        public static void Init()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.Contains("ViridianLink"))
                {
                    LoadViridian(assembly);
                    return;
                }
            }
            AppDomain.CurrentDomain.AssemblyLoad += ViridianLoader;
        }

        private static void ViridianLoader(object sender, AssemblyLoadEventArgs args)
        {
            if (args.LoadedAssembly.FullName.Contains("ViridianLink"))
            {
                LoadViridian(args.LoadedAssembly);
            }
        }

        static void LoadViridian(Assembly assembly)
        {
            if (!loaded)
            {
                loaded = true;
                ViridianASM = assembly;
                VModType = ViridianASM.GetType("ViridianLink.Core.IViridianMod");
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    LoadViridianMods(asm);
                }
                AppDomain.CurrentDomain.AssemblyLoad += (_, args) => LoadViridianMods(args.LoadedAssembly);
            }
        }

        static void LoadViridianMods(Assembly assembly)
        {
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (VModType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericTypeDefinition)
                    {
                        try
                        {
                            object VMod = Activator.CreateInstance(type);
                            var mod = CreateViridianMod(VMod);
                            ((List<IMod>)LoadedMods.GetValue(null)).Add(mod);
                        }
                        catch (Exception e)
                        {
                            Modding.Logger.LogError($"Failed to load Viridian Mod: {type} -> {e}");
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {

            }
            catch (Exception e)
            {
                Modding.Logger.Log($"Viridian Load Error -> {e}");
            }
        }
    }
}
