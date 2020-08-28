using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor.Utilities
{
	public static class AssetUtilities
	{
		public static T CreateScriptableObject<T>(string assetLocation = null,bool makeActive = true) where T : ScriptableObject
		{
			return (T)CreateScriptableObject(typeof(T), assetLocation, makeActive);
		}

		public static ScriptableObject CreateScriptableObject(Type type, string assetLocation = null, bool makeActive = true)
		{
			if (!typeof(ScriptableObject).IsAssignableFrom(type))
			{
				throw new Exception("The type " + type.FullName + " is not a ScriptableObject");
			}
			if (assetLocation == null)
			{
				assetLocation = "New" + type.Name;
			}

			if (!assetLocation.StartsWith("Assets"))
			{
				assetLocation = "Assets/" + assetLocation;
			}

			if (!assetLocation.EndsWith(".asset"))
			{
				assetLocation += ".asset";
			}
			ScriptableObject asset = ScriptableObject.CreateInstance(type);

			AssetDatabase.CreateAsset(asset, assetLocation);
			AssetDatabase.SaveAssets();

			EditorUtility.FocusProjectWindow();

			Selection.activeObject = asset;

			return asset;
		}

	}
}
