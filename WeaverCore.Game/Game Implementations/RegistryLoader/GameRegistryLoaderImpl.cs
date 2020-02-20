using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	public class GameRegistryLoaderImplementation : RegistryLoaderImplementation
	{
		public override IEnumerable<Registry> GetRegistries(Type ModType)
		{
			return RegistryLoader.GetEmbeddedRegistries(ModType);
		}
	}
}
