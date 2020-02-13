using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using ViridianCore.Helpers;
using ViridianLink.Core;
using ViridianLink.Extras;

namespace ViridianLink.Game.Implementations
{
    public class GameModLoaderImplementation : ViridianLink.Implementations.ModLoaderImplementation
    {
        static Type ModLoaderType = typeof(IMod).Assembly.GetType("Modding.ModLoader");

        static FieldInfo LoadedMods = ModLoaderType.GetField("LoadedMods", BindingFlags.Public | BindingFlags.Static);

        public override IEnumerable<IViridianMod> LoadAllMods()
        {
            Modding.Logger.Log("LOADING ALL THE MODS!!!");
            yield break;
            /*var fileLocation = typeof(ViridianCore.ViridianCore).Assembly.Location;

            var modDirectory = new FileInfo(fileLocation).Directory.FullName;

            foreach (var file in Directory.GetFiles(modDirectory,"*.dll"))
            {
                var assembly = Assembly.LoadFile(file);
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(IViridianMod).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericTypeDefinition)
                    {
                        Modding.Logger.Log("New Mod = " + type.Name);
                        Modding.Logger.Log("BB");
                        IViridianMod newMod = (IViridianMod)Activator.CreateInstance(type);
                        try
                        {
                            Modding.Logger.Log("CC");
                            var modHandle = typeof(ViridianMod<>).MakeGenericType(type);
                            Modding.Logger.Log("DD");
                            IMod finalMod = (IMod)modHandle.GetMethod("CreateMod", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { newMod });
                            Debugger.Log("Final Mod = " + finalMod);
                            ((List<IMod>)LoadedMods.GetValue(null)).Add(finalMod);
                            Modding.Logger.Log("EE");
                        }
                        catch (Exception e)
                        {
                            Modding.Logger.Log("MOD LOAD ERROR = " + e);
                            throw;
                        }

                        yield return newMod;
                    }
                }
            }*/
        }

        
    }


    public class ViridianMod<T> : IMod where T : IViridianMod
    {
        protected T Mod { get; private set; }
        protected Registry ModRegistry { get; private set; }

        internal ViridianMod(T mod)
        {
            Mod = mod;
        }

        public static ViridianMod<T> CreateMod(T mod)
        {
            if (mod.Unloadable)
            {
                return new UnloadableViridianMod<T>(mod);
            }
            else
            {
                return new ViridianMod<T>(mod);
            }
        }

        public string GetName() => Mod.Name;

        public List<(string, string)> GetPreloadNames() => null;

        public string GetVersion() => Mod.Version;

        public void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Modding.Logger.Log("A");
            Modding.Logger.Log($"Loading Viridian Mod: {GetName()} Version: {GetVersion()}");
            Modding.Logger.Log("B");
            ModRegistry = RegistryLoader.GetModRegistry<T>();
            if (ModRegistry != null)
            {
                ModRegistry.RegistryEnabled = true;
            }
            Modding.Logger.Log("C");
            Mod.Load();
            Modding.Logger.Log("D");
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

    public class UnloadableViridianMod<T> : ViridianMod<T>, ITogglableMod where T : IViridianMod
    {
        internal UnloadableViridianMod(T mod) : base(mod) { }

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