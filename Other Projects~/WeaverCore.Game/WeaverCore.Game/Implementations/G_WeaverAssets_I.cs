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
			WeaverLog.Log("Asset To Load = " + assetName);
			var finalAsset = weaverAssetBundle.LoadAsset<T>(assetName);
			WeaverLog.Log("Final Asset = " + finalAsset);
			WeaverLog.Log("Is Null = " + (finalAsset == null));
			if (finalAsset == null)
			{
				var assetNames = weaverAssetBundle.GetAllAssetNames();
				Debug.Log("All Asset Names Count = " + assetNames.GetLength(0));
                foreach (var name in assetNames)
                {
					Debug.Log("All Asset Names = " + name);
                }
				foreach (var name in assetNames)
				{
					if (name.Contains(assetName))
					{
						WeaverLog.Log("Next Asset = " + assetName);
						finalAsset = weaverAssetBundle.LoadAsset<T>(name);
						WeaverLog.Log("Final Asset = " + finalAsset);
						if (finalAsset != null)
						{
							return finalAsset;
						}
					}
				}
			}
			WeaverLog.Log("DONE");
			return finalAsset;
		}

		static void LoadRegistries(AssetBundle bundle)
		{
			foreach (var registry in weaverAssetBundle.LoadAllAssets<Registry>())
			{
				registry.EnableRegistry();
			}
		}

		static void LoadWeaverAssetBundle()
		{
			foreach (var loadedBundle in AssetBundle.GetAllLoadedAssetBundles())
			{
				Debug.LogError("LOADED BUNDLE = " + loadedBundle.name);
				if (loadedBundle.name == "weavercore_modclass_bundle")
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
				Debug.LogError("resource name = " + resourceName);
				if (resourceName.EndsWith(extension) && resourceName.ToUpper().Contains("WEAVERCORE_MODCLASS_BUNDLE"))
				{
					try
					{
						Debug.LogError("bundle name = " + resourceName);
						var bundle = AssetBundle.LoadFromStream(weaverAssembly.GetManifestResourceStream(resourceName));
						weaverAssetBundle = bundle;
						break;
					}
					catch (Exception e)
					{
						throw new Exception("Failed to load the WeaverAssets Asset Bundle", e);
					}
				}
			}
			if (weaverAssetBundle == null)
			{
				throw new Exception("Unable to find the WeaverAssets Asset Bundle");
			}
			LoadRegistries(weaverAssetBundle);

		}

		public override T LoadAssetFromBundle<T>(string bundleName, string name)
		{
			foreach (var bundle in AssetBundle.GetAllLoadedAssetBundles())
			{
				WeaverLog.Log("Found Bundle = " + bundle.name);
				if (bundle.name.Contains(bundleName))
				{
					WeaverLog.Log("Loading Asset from bundle " + bundle.name);
					return LoadAssetFromBundle<T>(bundle, name);
				}
			}
			return default(T);
		}

		public T LoadAssetFromBundle<T>(AssetBundle bundleName, string name) where T : UnityEngine.Object
		{
			var lowerName = name.ToLower();
			var assetNames = bundleName.GetAllAssetNames();
			foreach (var assetName in assetNames)
			{
				//WeaverLog.Log("Name = " + assetName);
				if (assetName.ToLower().Contains(lowerName))
				{
					//WeaverLog.Log("Contains Name!");
					var asset = bundleName.LoadAsset<T>(assetName);
					if (asset != null)
					{
						//WeaverLog.Log("Found Asset Name = " + assetName);
						return asset;
					}
					/*else
					{
						WeaverLog.Log("Actual Type = " + bundleName.LoadAsset<UnityEngine.Object>(assetName)?.GetType().FullName);
					}*/
				}
			}
			//WeaverLog.Log("Returning Null");
			return default(T);
			/*var asset = assets.FirstOrDefault(a => a.Contains(lowerName));
			if (asset != null)
			{
				var instance = bundleName.LoadAsset<T>(asset);
				return instance;
			}
			return default(T);*/
		}
	}
}
