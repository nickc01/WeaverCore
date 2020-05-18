using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Helpers
{
	public static class WeaverAssetLoader
	{
		public static AssetBundle WeaverAssetBundle { get; private set; }

		internal static void SetWeaverAssetBundle(AssetBundle bundle)
		{
			WeaverAssetBundle = bundle;
		}

		public static string[] AssetNames => WeaverAssetBundle.GetAllAssetNames();

		public static T LoadWeaverAsset<T>(string name) where T : UnityEngine.Object
		{
			return WeaverAssetBundle.LoadAsset<T>(name);
		}
	}
}
