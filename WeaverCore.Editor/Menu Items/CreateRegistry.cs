using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Utilities;

namespace WeaverCore.Editor
{
	static class CreateRegistry
	{
		[MenuItem("WeaverCore/Create/Registry")]
		static void CreateRegistryMenuItem()
		{
			AssetUtilities.CreateScriptableObject<Registry>();
			/*Registry asset = ScriptableObject.CreateInstance<Registry>();

			AssetDatabase.CreateAsset(asset, "Assets/NewRegistry.asset");
			AssetDatabase.SaveAssets();

			EditorUtility.FocusProjectWindow();

			Selection.activeObject = asset;*/
		}
	}
}
