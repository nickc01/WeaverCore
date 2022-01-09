using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor.Utilities
{
	/// <summary>
	/// Contains some utility functions regarding assets
	/// </summary>
	public static class AssetUtilities
	{
		/// <summary>
		/// Creates a scriptable object from a type and puts it in the "Assets" folder
		/// </summary>
		/// <typeparam name="T">The type of scriptable object to create</typeparam>
		/// <param name="assetLocation">The location to place the scriptable object</param>
		/// <returns>Returns an instance to the created scriptable object</returns>
		public static T CreateScriptableObject<T>(string assetLocation = null) where T : ScriptableObject
		{
			return (T)CreateScriptableObject(typeof(T), assetLocation);
		}

		/// <summary>
		/// Creates a scriptable object from a type and puts it in the "Assets" folder
		/// </summary>
		/// <param name="type">The type of scriptable object to create</param>
		/// <param name="assetLocation">The location to place the scriptable object</param>
		/// <returns>Returns an instance to the created scriptable object</returns>
		public static ScriptableObject CreateScriptableObject(Type type, string assetLocation = null)
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
