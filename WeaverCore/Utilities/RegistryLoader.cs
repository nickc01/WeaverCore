using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using WeaverCore.Internal;

namespace WeaverCore.Utilities
{
	public static class RegistryLoader
	{
		static bool loaded = false;

		public static IEnumerable<Registry> GetModRegistries<Mod>() where Mod : IWeaverMod
		{
			return GetModRegistries(typeof(Mod));
		}

		public static IEnumerable<Registry> GetModRegistries(Type ModType)
		{
			var findings = Registry.FindAll(ModType);
			if (findings != null)
			{
				foreach (var registry in findings)
				{
					yield return registry;
				}
			}
			var loader = ImplFinder.GetImplementation<RegistryLoader_I>();

			var registries = loader.GetRegistries(ModType);

			foreach (var registry in registries)
			{
				registry.Start();
				yield return registry;
			}
		}

		public static IEnumerable<Registry> GetEmbeddedRegistries(Type modType)
		{
			WeaverLog.Log($"Loading Embedded Registries for mod [{modType.FullName}]");
			if (!loaded)
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

				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					try
					{
						foreach (var name in assembly.GetManifestResourceNames())
						{
							if (name.EndsWith(extension))
							{
								AssetBundle bundle = null;
								bool isWeaverCore = name.ToUpper().Contains("WEAVERCORE");

								if (!isWeaverCore || (isWeaverCore && WeaverAssetLoader.WeaverAssetBundle == null && assembly == typeof(WeaverMod).Assembly))
								{
									//Debugger.LogError("LOADING BEFORE");
									bundle = AssetBundle.LoadFromStream(assembly.GetManifestResourceStream(name));
									//Debugger.LogError("LOADING AFTER");
									if (isWeaverCore)
									{
										WeaverAssetLoader.SetWeaverAssetBundle(bundle);
									}
								}
							}
						}
					}
					catch (NotSupportedException error)
					{
						if (!error.Message.Contains("not supported in a dynamic module"))
						{
							throw;
						}
					}
				}
				loaded = true;
			}
			foreach (var bundle in AssetBundle.GetAllLoadedAssetBundles())
			{
				foreach (var registry in bundle.LoadAllAssets<Registry>())
				{
					if (modType.IsAssignableFrom(registry.ModType))
					{
						yield return registry;
					}
				}
			}
		}
	}
}
