using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace WeaverCore.Editor.Implementations
{
	public class E_Modloader_I : WeaverCore.Implementations.ModLoader_I
    {
        static Type VModType = typeof(WeaverMod);
        static List<WeaverMod> LoadedMods = new List<WeaverMod>();

        static void LoadMod(Type ModType)
        {
            try
            {
                var mod = (WeaverMod)Activator.CreateInstance(ModType);
                LoadedMods.Add(mod);
            }
            catch (Exception e)
            {
                WeaverLog.LogError("Failed to load mod : " + ModType + " : " + e);
            }
        }

        public override IEnumerable<WeaverMod> LoadAllMods()
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
                    WeaverLog.LogError("ModLoader Error : " + e);
                }
            }
            LoadedMods.Sort((v1, v2) => v1.LoadPriority() - v2.LoadPriority());
            foreach (var mod in LoadedMods)
            {
                WeaverLog.Log("Loading Weaver Mod <b>" + mod.Name + "</b>, Version: " + mod.GetVersion());
                mod.Initialize();
                yield return mod;
            }
        }
    }
}