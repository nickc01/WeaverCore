using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;
using WeaverCore.Implementations;

namespace WeaverCore.Internal
{
	public static class ModLoader
	{
		static bool loadedAllAssemblies = false;

		static void LoadAllModAssemblies()
		{
			if (!loadedAllAssemblies)
			{
				loadedAllAssemblies = true;
				string path = "Assets";
				if (ImplementationFinder.State == RunningState.Game)
				{
					path = ModLoader.GetModFolderPath();
				}
				foreach (var file in Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories))
				{
					if (!file.Contains("Assets\\Editor") && !file.Contains($"{nameof(WeaverCore)}\\Editor"))
					{
						Assembly.LoadFile(file);
					}
				}
			}
		}

		public static IEnumerable<IWeaverMod> LoadAllModsIter()
		{
			LoadAllModAssemblies();
			var impl = ImplementationFinder.GetImplementation<ModLoaderImplementation>();
			foreach (var mod in impl.LoadAllMods())
			{
				var registry = RegistryLoader.GetModRegistries(mod.GetType()).ToList();
				if (registry.Count == 0 && !(mod is WeaverCoreMod))
				{
					Debugger.LogWarning($"No registry found for mod : {mod.Name}");
				}
				yield return mod;
			}
		}

		public static void LoadAllMods()
		{
			foreach (var mod in LoadAllModsIter()) { }
		}

		public static string GetModFolderPath()
		{
			if (ImplementationFinder.State == RunningState.Game)
			{
				if (SystemInfo.operatingSystem.Contains("Windows"))
				{
					return Application.dataPath + "\\Managed\\Mods";
				}
				else if (SystemInfo.operatingSystem.Contains("Mac"))
				{
					return Application.dataPath + "/Resources/Data/Managed/Mods/";
				}
				else if (SystemInfo.operatingSystem.Contains("Linux"))
				{
					return Application.dataPath + "/Managed/Mods";
				}
				else
				{
					return null;
				}
			}
			return null;
		}
	}
}
