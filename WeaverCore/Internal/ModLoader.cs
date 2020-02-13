using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Helpers;
using WeaverCore.Implementations;

namespace WeaverCore.Internal
{
	public static class ModLoader
	{
		public static IEnumerable<IWeaverMod> LoadAllModsIter()
		{
			//Debugger.Log("Loading Weaver Mods");
			var impl = ImplementationFinder.GetImplementation<ModLoaderImplementation>();
			foreach (var mod in impl.LoadAllMods())
			{
				var registry = RegistryLoader.GetModRegistry(mod.GetType());
				if (registry == null && !(mod is WeaverCoreMod))
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
	}
}
