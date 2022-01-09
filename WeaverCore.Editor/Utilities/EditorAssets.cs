using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor.Utilities
{
	/// <summary>
	/// Used for loading assets that are located in the WeaverCore.Editor folder
	/// </summary>
	public static class EditorAssets
	{
		/// <summary>
		/// Loads an editor assets
		/// </summary>
		/// <typeparam name="T">The type of asset to load</typeparam>
		/// <param name="name">The name of the asset to load</param>
		/// <returns>Returns an instance to the asset, or null if it doesn't exist</returns>
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

		/// <summary>
		/// Loads an editor assets
		/// </summary>
		/// <param name="name">The name of the asset to load</param>
		/// <param name="type">The type of asset to load</param>
		/// <returns>Returns an instance to the asset, or null if it doesn't exist</returns>
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
