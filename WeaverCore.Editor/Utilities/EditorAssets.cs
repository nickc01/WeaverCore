using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor.Utilities
{
	public static class EditorAssets
	{
		public static T LoadEditorAsset<T>(string name) where T : UnityEngine.Object
		{
			var assetIDs = AssetDatabase.FindAssets(name);

			for (int i = 0; i < assetIDs.GetLength(0); i++)
			{
				var path = AssetDatabase.GUIDToAssetPath(assetIDs[i]);
				if (path.Contains("WeaverCore.Editor"))
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

		public static UnityEngine.Object LoadEditorAsset(string name, Type type)
		{
			var assetPaths = AssetDatabase.FindAssets(name);

			for (int i = 0; i < assetPaths.GetLength(0); i++)
			{
				var path = assetPaths[i];
				if (path.Contains("WeaverCore.Editor"))
				{
					var asset = AssetDatabase.LoadAssetAtPath(path,type);
					if (asset != null)
					{
						return asset;
					}
				}
			}

			return null;
		}
	}
}
