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
		static HashSet<Assembly> LoadedAssemblies = new HashSet<Assembly>();

		//static bool loaded = false;

		public static void LoadAllRegistries<Mod>() where Mod : WeaverMod
		{
			LoadAllRegistries(typeof(Mod));
		}

		public static void LoadAllRegistries(Type modType)
		{
			WeaverLog.Log("Loading Registries for " + modType.FullName);
			LoadAllRegistries(modType.Assembly);
		}

		public static void LoadAllRegistries(Assembly assembly)
		{
			if (!LoadedAssemblies.Contains(assembly))
			{
				LoadedAssemblies.Add(assembly);
				var loader = ImplFinder.GetImplementation<RegistryLoader_I>();
				loader.LoadRegistries(assembly);
			}
		}


		/*public static IEnumerable<Registry> GetModRegistries<Mod>() where Mod : WeaverMod
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

			var registries = loader.LoadRegistries(ModType);

			foreach (var registry in registries)
			{
				registry.Start();
				yield return registry;
			}
		}*/

		/// <summary>
		/// Loads any registries that are embedded inside of the assembly as an embedded resource asset bundle
		/// </summary>
		/// <param name="assembly">The assembly to load from</param>
		public static void LoadEmbeddedRegistries(Assembly assembly)
		{
			var assemblyName = assembly.GetName().Name;
			//WeaverLog.Log("Loading Embedded Registries for mod [" + modType.FullName + "]");
			//if (!loaded)
			//{
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

			//foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			//{
			try
			{
				if (assembly != typeof(WeaverMod).Assembly)
				{
					foreach (var name in assembly.GetManifestResourceNames())
					{
						if (name.EndsWith(extension))
						{
							WeaverLog.Log("Loading Mod Bundle = " + name);
							var bundle = AssetBundle.LoadFromStream(assembly.GetManifestResourceStream(name));
							foreach (var registry in bundle.LoadAllAssets<Registry>())
							{
								if (registry.ModAssemblyName == assemblyName)
								{
									registry.Initialize();
								}
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
			//}
			//loaded = true;
			//}
			/*foreach (var bundle in AssetBundle.GetAllLoadedAssetBundles())
			{
				foreach (var registry in bundle.LoadAllAssets<Registry>())
				{
					if (modType.IsAssignableFrom(registry.ModType))
					{
						yield return registry;
					}
				}
			}*/
		}
	}
}
