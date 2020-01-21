using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ViridianLink.Core;
using ViridianLink.Helpers;

namespace ViridianLink.Editor.Helpers
{
    public static class ModLoader
    {
        static Type VModType = typeof(IViridianMod);
        static List<IViridianMod> LoadedMods = new List<IViridianMod>();

        public static void LoadAllMods()
        {
            LoadedMods.Clear();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (VModType.IsAssignableFrom(type) && !type.ContainsGenericParameters && !type.IsAbstract)
                        {
                            LoadMod(type);
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {

                }
                catch (Exception e)
                {
                    Debugger.LogError("ModLoader Error : " + e);
                }
            }
            LoadedMods.Sort((v1, v2) => v1.LoadPriority - v2.LoadPriority);
            foreach (var mod in LoadedMods)
            {
                Debugger.Log($"Loading Viridian Mod {mod.Name}, Version: {mod.Version}");
                mod.Load();
            }
        }

        public static void LoadMod(Type ModType)
        {
            try
            {
                var mod = (IViridianMod)Activator.CreateInstance(ModType);
                LoadedMods.Add(mod);
            }
            catch (Exception e)
            {
                Debugger.LogError($"Failed to load mod : {ModType} : {e}");
            }
        }
    }
}
