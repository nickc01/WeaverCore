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
			var loader = ImplInfo.GetImplementation<RegistryLoaderImplementation>();

			var registry = loader.GetRegistry(ModType);


			return null;
			/*if (registry != null)
			{
				
			}*/
		}
	}
}
