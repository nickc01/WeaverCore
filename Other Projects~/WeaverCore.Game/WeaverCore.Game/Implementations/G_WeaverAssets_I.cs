using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
	public class G_WeaverAssets_I : WeaverAssets_I
	{
		static AssetBundle weaverAssetBundle;

		public override IEnumerable<string> AllAssetBundles
		{
			get
			{
				foreach (var bundle in AssetBundle.GetAllLoadedAssetBundles())
				{
					yield return bundle.name;
				}
			}
		}

		public override void Initialize()
		{
			LoadWeaverAssetBundle();
		}

		public override T LoadAsset<T>(string assetName)
		{
			var finalAsset = weaverAssetBundle.LoadAsset<T>(assetName);
			if (finalAsset == null)
			{
				var assetNames = weaverAssetBundle.GetAllAssetNames();
				foreach (var name in assetNames)
				{
					if (name.Contains(assetName))
					{
						finalAsset = weaverAssetBundle.LoadAsset<T>(name);
						if (finalAsset != null)
						{
							return finalAsset;
						}
					}
				}
			}
			return finalAsset;
		}

		static void LoadRegistries(AssetBundle bundle)
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

			var weaverAssembly = typeof(WeaverAssets).Assembly;

			foreach (var resourceName in weaverAssembly.GetManifestResourceNames())
			{
				if (resourceName.EndsWith(extension) && resourceName.ToUpper().Contains("WEAVERCORE_BUNDLE"))
				{
					try
					{
						var bundle = AssetBundle.LoadFromStream(weaverAssembly.GetManifestResourceStream(resourceName));
						weaverAssetBundle = bundle;
						break;
					}
					catch (Exception e)
					{
						throw new WeaverAssets.WeaverAssetsException("Failed to load the WeaverAssets Asset Bundle", e);
					}
				}
			}
			if (weaverAssetBundle == null)
			{
				throw new WeaverAssets.WeaverAssetsException("Unable to find the WeaverAssets Asset Bundle");
			}
			LoadRegistries(weaverAssetBundle);

		}

		public override T LoadAssetFromBundle<T>(string bundleName, string name)
		{
			foreach (var bundle in AssetBundle.GetAllLoadedAssetBundles())
			{
				if (bundle.name.Contains(bundleName))
				{
					return LoadAssetFromBundle<T>(bundle, name);
				}
			}
			return default(T);
		}

		public T LoadAssetFromBundle<T>(AssetBundle bundleName, string name) where T : UnityEngine.Object
		{
			var lowerName = name.ToLower();
			var assets = bundleName.GetAllAssetNames();
			var asset = assets.FirstOrDefault(a => a.Contains(lowerName));
			if (asset != null)
			{
				var instance = bundleName.LoadAsset<T>(asset);
				return instance;
			}
			return default(T);
		}
	}
}
