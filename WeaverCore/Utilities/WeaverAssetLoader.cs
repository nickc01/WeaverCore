using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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


		public static bool WeaverAssetBundleIsSet => weaverAssetBundle != null;

		static AssetBundle weaverAssetBundle = null;
		public static AssetBundle WeaverAssetBundle
		{
			get
			{
				if (weaverAssetBundle == null)
				{
					LoadWeaverAssetBundle();
				}
				return weaverAssetBundle;
			}
		}

		static void LoadWeaverAssetBundle()
		{
			foreach (var loadedBundle in AssetBundle.GetAllLoadedAssetBundles())
			{
				if (loadedBundle.name == "weavercore")
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
						throw new WeaverAssetsException("Failed to load the WeaverAssets Asset Bundle",e);
					}
				}
			}
			if (weaverAssetBundle == null)
			{
				throw new WeaverAssetsException("Unable to find the WeaverAssets Asset Bundle");
			}

		}

		internal static void SetWeaverAssetBundle(AssetBundle bundle)
		{
			weaverAssetBundle = bundle;
		}

		public static string[] AssetNames => WeaverAssetBundle.GetAllAssetNames();

		public static T LoadWeaverAsset<T>(string name) where T : UnityEngine.Object
		{
			return WeaverAssetBundle.LoadAsset<T>(name);
		}
	}
}
