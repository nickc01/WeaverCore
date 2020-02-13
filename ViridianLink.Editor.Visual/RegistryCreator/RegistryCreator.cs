using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using ViridianLink.Core;

namespace ViridianLink.Editor.Visual
{
	static class RegistryCreator
	{
		[MenuItem("ViridianLink/Create Registry")]
		static void CreateRegistry()
		{
			Registry asset = ScriptableObject.CreateInstance<Registry>();

			AssetDatabase.CreateAsset(asset, "Assets/NewRegistry.asset");
			AssetDatabase.SaveAssets();

			EditorUtility.FocusProjectWindow();

			Selection.activeObject = asset;
		}
	}
}
