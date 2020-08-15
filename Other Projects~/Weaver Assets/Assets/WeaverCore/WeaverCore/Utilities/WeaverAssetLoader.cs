using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Enums;

namespace WeaverCore.Utilities
{
	public static class WeaverAssetLoader
	{
		[Serializable]
		public class WeaverAssetsException : Exception
		{
			public WeaverAssetsException() { }
			public WeaverAssetsException(string message) : base(message) { }
			public WeaverAssetsException(string message, Exception inner) : base(message, inner) { }
			protected WeaverAssetsException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}


		public static bool WeaverAssetBundleIsSet
		{
			get
			{
				return weaverAssetBundle != null;
			}
		}

		static AssetBundle weaverAssetBundle = null;
		public static AssetBundle WeaverAssetBundle
		{
			get
			{
				if (weaverAssetBundle == null)
				{
					LoadWeaverAssetBundle();
					LoadRegistries();
				}
				return weaverAssetBundle;
			}
		}

		static void LoadRegistries()
		{
			foreach (var registry in weaverAssetBundle.LoadAllAssets<Registry>())
			{
				registry.Initialize();
			}
		}

		static void LoadWeaverAssetBundle()
		{
			foreach (var loadedBundle in AssetBundle.GetAllLoadedAssetBundles())
			{
				if (loadedBundle.name == "weavercore_bundle")
				{
					weaverAssetBundle = loadedBundle;
					return;
				}
			}
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

			if (CoreInfo.LoadState == RunningState.Game)
			{
				var weaverAssembly = typeof(WeaverAssetLoader).Assembly;

				foreach (var resourceName in weaverAssembly.GetManifestResourceNames())
				{
					if (resourceName.EndsWith(extension) && resourceName.ToUpper().Contains("WEAVERCORE"))
					{
						try
						{
							var bundle = AssetBundle.LoadFromStream(weaverAssembly.GetManifestResourceStream(resourceName));
							weaverAssetBundle = bundle;
							break;
						}
						catch (Exception e)
						{
							throw new WeaverAssetsException("Failed to load the WeaverAssets Asset Bundle", e);
						}
					}
				}
				if (weaverAssetBundle == null)
				{
					throw new WeaverAssetsException("Unable to find the WeaverAssets Asset Bundle");
				}
			}
			else
			{
				var bundleFileLocation = new FileInfo("Assets\\WeaverCore\\WeaverAssets\\Bundles\\weavercore_bundle_editor" + extension);
				weaverAssetBundle = AssetBundle.LoadFromFile(bundleFileLocation.FullName);

				
			}

		}

		public static string[] AssetNames
		{
			get
			{
				return WeaverAssetBundle.GetAllAssetNames();
			}
		}

		public static T LoadWeaverAsset<T>(string name) where T : UnityEngine.Object
		{
			return WeaverAssetBundle.LoadAsset<T>(name);
		}
	}
}
