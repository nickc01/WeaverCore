using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor
{
	static class RegistryCreator
	{
		[MenuItem("WeaverCore/Create Registry")]
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
