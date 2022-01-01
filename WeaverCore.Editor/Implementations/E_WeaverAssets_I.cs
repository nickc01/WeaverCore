using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Implementations
{
	public class E_WeaverAssets_I : WeaverAssets_I
	{
		public override IEnumerable<string> AllAssetBundles
		{
			get
			{
				return AssetDatabase.GetAllAssetBundleNames();
			}
		}

		public override void Initialize()
		{
			
		}

		public override T LoadAsset<T>(string assetName)
		{
			var possibleGUIDs = AssetDatabase.FindAssets(assetName);

			foreach (var guid in possibleGUIDs)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));
				var importer = AssetImporter.GetAtPath(path);
				if (asset is T && importer.assetBundleName == WeaverAssets.WeaverAssetBundleName)
				{
					return (T)asset;
				}
			}

			return null;
		}

		public override T LoadAssetFromBundle<T>(string bundleName, string name)
		{
			var ids = AssetDatabase.FindAssets(name);

			for (int i = 0; i < ids.GetLength(0); i++)
			{
				var path = AssetDatabase.GUIDToAssetPath(ids[i]);
				var importer = AssetImporter.GetAtPath(path);
				if (importer.assetBundleName.Contains(bundleName))
				{
					var asset = AssetDatabase.LoadAssetAtPath<T>(path);
					if (asset != null)
					{
						return asset;
					}
				}
			}

			return default(T);
		}

        public override IEnumerable<T> LoadAssets<T>(string assetName)
        {
			var possibleGUIDs = AssetDatabase.FindAssets(assetName);

			foreach (var guid in possibleGUIDs)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));
				var importer = AssetImporter.GetAtPath(path);
				if (asset is T && importer.assetBundleName == WeaverAssets.WeaverAssetBundleName)
				{
					yield return (T)asset;
				}
			}
		}

        public override IEnumerable<T> LoadAssetsFromBundle<T>(string bundleName, string assetName)
        {
			var ids = AssetDatabase.FindAssets(assetName);

			for (int i = 0; i < ids.GetLength(0); i++)
			{
				var path = AssetDatabase.GUIDToAssetPath(ids[i]);
				var importer = AssetImporter.GetAtPath(path);
				if (importer.assetBundleName.Contains(bundleName))
				{
					var asset = AssetDatabase.LoadAssetAtPath<T>(path);
					if (asset != null)
					{
						yield return asset;
					}
				}
			}
		}
    }
}
