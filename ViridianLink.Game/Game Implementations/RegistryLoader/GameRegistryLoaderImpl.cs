using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using ViridianLink.Core;
using ViridianLink.Helpers;
using ViridianLink.Implementations;

namespace ViridianLink.Game.Implementations
{
	public class GameRegistryLoaderImplementation : RegistryLoaderImplementation
	{

		public override Registry GetRegistry(Type ModType)
		{
			string extension = null;

			if (SystemInfo.operatingSystem.Contains("Windows"))
			{
				extension = ".bundle.win";
			}
			else if (SystemInfo.operatingSystem.Contains("Mac"))
			{
				extension = ".bundle.mac";
			}
			else if (SystemInfo.operatingSystem.Contains("Linux"))
			{
				extension = ".bundle.unix";
			}

			Assembly assembly = ModType.Assembly;

			foreach (var name in assembly.GetManifestResourceNames())
			{
				Debugger.Log("Resource Name = " + name);
				if (name.EndsWith(extension))
				{
					Debugger.Log("Loading Resource " + name);
					AssetBundle.LoadFromStream(assembly.GetManifestResourceStream(name));
				}
			}

			foreach (var bundle in AssetBundle.GetAllLoadedAssetBundles())
			{
				Debugger.Log("Bundle = " + bundle.name);
				foreach (var registry in bundle.LoadAllAssets<Registry>())
				{
					Debugger.Log("Asset = " + registry.name);
					Debugger.Log("Mod Type = " + ModType.FullName);
					Debugger.Log("Registry Mod Type = " + registry.ModType);
					if (ModType.IsAssignableFrom(registry.ModType))
					{
						return registry;
					}
				}
			}
			return null;
		}
	}
}
