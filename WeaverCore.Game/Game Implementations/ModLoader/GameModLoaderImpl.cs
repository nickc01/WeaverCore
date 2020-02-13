using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;
using WeaverCore.Internal;

namespace WeaverCore.Game.Implementations
{
    public class GameModLoaderImplementation : WeaverCore.Implementations.ModLoaderImplementation
    {
        static Type ModLoaderType = typeof(IMod).Assembly.GetType("Modding.ModLoader");

        static FieldInfo LoadedMods = ModLoaderType.GetField("LoadedMods", BindingFlags.Public | BindingFlags.Static);

        public override IEnumerable<IWeaverMod> LoadAllMods()
        {
            //Modding.Logger.Log("LOADING ALL THE MODS!!!");
            foreach (var type in typeof(WeaverCoreMod).Assembly.GetTypes())
            {
                if (typeof(IWeaverMod).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericTypeDefinition)
                {
                    IWeaverMod newMod = (IWeaverMod)Activator.CreateInstance(type);

                    var modHandle = typeof(WeaverMod<>).MakeGenericType(type);

                    IMod finalMod = (IMod)modHandle.GetMethod("CreateMod", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { newMod });

                    ((List<IMod>)LoadedMods.GetValue(null)).Add(finalMod);
                    yield return newMod;
                }
            }
        }

        
    }


    public class WeaverMod<T> : IMod where T : IWeaverMod
    {
        protected T Mod { get; private set; }
        protected Registry ModRegistry { get; private set; }

        internal WeaverMod(T mod)
        {
            Mod = mod;
        }

        public static WeaverMod<T> CreateMod(T mod)
        {
            if (mod.Unloadable)
            {
                return new UnloadableWeaverMod<T>(mod);
            }
            else
            {
                return new WeaverMod<T>(mod);
            }
        }

        public string GetName() => Mod.Name;

        public List<(string, string)> GetPreloadNames() => null;

        public string GetVersion() => Mod.Version;

        public void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            //Modding.Logger.Log("A");
            //Modding.Logger.Log($"Loading Weaver Mod: {GetName()} Version: {GetVersion()}");
            //Modding.Logger.Log("B");
            ModRegistry = RegistryLoader.GetModRegistry<T>();
            if (ModRegistry != null)
            {
                ModRegistry.RegistryEnabled = true;
            }
            //Modding.Logger.Log("C");
            Mod.Load();
            //Modding.Logger.Log("D");
        }

        public bool IsCurrent() => true;

        public int LoadPriority() => Mod.LoadPriority;

        public void Log(string message)
        {
            Modding.Logger.Log(Mod.Name + " : " + message);
        }

        public void Log(object message)
        {
            Modding.Logger.Log(Mod.Name + " : " + message);
        }

        public void LogDebug(string message)
        {
            Modding.Logger.LogDebug(Mod.Name + " : " + message);
        }

        public void LogDebug(object message)
        {
            Modding.Logger.LogDebug(Mod.Name + " : " + message);
        }

        public void LogError(string message)
        {
            Modding.Logger.LogError(Mod.Name + " : " + message);
        }

        public void LogError(object message)
        {
            Modding.Logger.LogError(Mod.Name + " : " + message);
        }

        public void LogFine(string message)
        {
            Modding.Logger.LogFine(Mod.Name + " : " + message);
        }

        public void LogFine(object message)
        {
            Modding.Logger.LogFine(Mod.Name + " : " + message);
        }

        public void LogWarn(string message)
        {
            Modding.Logger.LogWarn(Mod.Name + " : " + message);
        }

        public void LogWarn(object message)
        {
            Modding.Logger.LogWarn(Mod.Name + " : " + message);
        }
    }

    public class UnloadableWeaverMod<T> : WeaverMod<T>, ITogglableMod where T : IWeaverMod
    {
        internal UnloadableWeaverMod(T mod) : base(mod) { }

        public void Unload()
        {
            if (ModRegistry != null)
            {
                ModRegistry.RegistryEnabled = true;
            }
            Mod.Unload();
        }
    }
}