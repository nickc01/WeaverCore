using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Internal;

namespace WeaverCore.Game.Implementations
{
	public class G_Modloader_I : WeaverCore.Implementations.ModLoader_I
    {
        static Type ModLoaderType = typeof(IMod).Assembly.GetType("Modding.ModLoader");

        static FieldInfo LoadedMods = ModLoaderType.GetField("LoadedMods", BindingFlags.Public | BindingFlags.Static);

        //This just needs to load all mods within WeaverCore.dll, since all the other mods are loaded using a different method
        public override IEnumerable<IWeaverMod> LoadAllMods()
        {
            foreach (var type in typeof(Internal.WeaverCore).Assembly.GetTypes())
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

    class WeaverMod<T> : IMod where T : IWeaverMod
    {
        protected T Mod { get; private set; }
        protected List<Registry> ModRegistries { get; private set; }

        internal WeaverMod(T mod)
        {
            Mod = mod;
        }

        public static WeaverMod<T> CreateMod(T mod)
        {
            //Debugger.Log("Mod Name = " + mod.Name);
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
            ModRegistries = RegistryLoader.GetModRegistries<T>().ToList();
            foreach (var registry in ModRegistries)
            {
                registry.RegistryEnabled = true;
            }
            Mod.Load();
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

    class UnloadableWeaverMod<T> : WeaverMod<T>, ITogglableMod where T : IWeaverMod
    {
        internal UnloadableWeaverMod(T mod) : base(mod) { }

        public void Unload()
        {
            foreach (var registry in ModRegistries)
            {
                registry.RegistryEnabled = false;
            }
            Mod.Unload();
        }
    }
}