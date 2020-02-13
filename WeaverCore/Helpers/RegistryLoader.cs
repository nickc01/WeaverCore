using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Implementations;

namespace WeaverCore.Helpers
{
	public static class RegistryLoader
	{
		public static Registry GetModRegistry<Mod>() where Mod : IWeaverMod
		{
			return GetModRegistry(typeof(Mod));
		}

		public static Registry GetModRegistry(Type ModType)
		{
			var finding = Registry.Find(ModType);
			if (finding != null)
			{
				return finding;
			}
			var loader = ImplementationFinder.GetImplementation<RegistryLoaderImplementation>();

			var registry = loader.GetRegistry(ModType);

			if (registry != null)
			{
				registry.Start();
				//Debugger.Log("Loading Registry for Mod: " + registry.ModType.Name);
				return registry;
			}
			return null;
		}
	}
}
