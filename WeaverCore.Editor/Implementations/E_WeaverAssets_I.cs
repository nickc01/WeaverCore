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
	}
}
