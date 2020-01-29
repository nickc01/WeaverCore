using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViridianLink.Core;
using ViridianLink.Implementations;

namespace ViridianLink.Helpers
{
	public static class ModLoader
	{
		public static IEnumerable<IViridianMod> LoadAllModsIter()
		{
			Debugger.Log("Loading Viridian Mods");
			var impl = ImplInfo.GetImplementation<ModLoaderImplementation>();
			foreach (var mod in impl.LoadAllMods())
			{
				var registry = RegistryLoader.LoadModRegistry(mod.GetType());
				if (registry == null && !(mod is ViridianLinkMod))
				{
					Debugger.LogWarning($"No registry found for mod : {mod.Name}");
				}
				else if (registry != null)
				{
					foreach (var feature in registry.AllFeatures)
					{
						Debugger.Log("D");
						Debugger.Log("Feature = " + feature?.name);
					}
				}
				yield return mod;
			}
		}

		public static void LoadAllMods()
		{
			foreach (var mod in LoadAllModsIter()) { }
		}
	}
}
