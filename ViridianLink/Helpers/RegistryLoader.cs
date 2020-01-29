using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViridianLink.Core;
using ViridianLink.Implementations;

namespace ViridianLink.Helpers
{
	public static class RegistryLoader
	{
		public static Registry LoadModRegistry<Mod>() where Mod : IViridianMod
		{
			return LoadModRegistry(typeof(Mod));
		}

		public static Registry LoadModRegistry(Type ModType)
		{
			var finding = Registry.Find(ModType);
			if (finding != null)
			{
				return finding;
			}
			var loader = ImplInfo.GetImplementation<RegistryLoaderImplementation>();

			var registry = loader.GetRegistry(ModType);

			if (registry != null)
			{
				registry.Start();
				Debugger.Log("Loading Registry for Mod: " + registry.ModType.Name);
				return registry;
			}
			return null;
		}
	}
}
