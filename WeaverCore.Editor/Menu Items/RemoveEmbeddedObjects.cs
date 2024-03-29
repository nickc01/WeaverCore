﻿using UnityEditor;
using UnityEngine;
using WeaverCore;

public class RemoveEmbeddedObjects : EditorWindow
{
	[MenuItem("WeaverCore/Tools/Remove Embedded Objects")]
	public static void Convert()
	{
		Display();
	}

	UnityEngine.Object obj;

	public static RemoveEmbeddedObjects Display()
	{
		var window = GetWindow<RemoveEmbeddedObjects>();
		window.titleContent = new GUIContent("Remove Embedded Objects");
		window.Show();

		return window;
	}

	private void OnGUI()
	{
		obj = (UnityEngine.Object)EditorGUILayout.ObjectField(new GUIContent("Object", "The object to remove all embedded assets from"), obj, typeof(UnityEngine.Object), true);

		if (GUILayout.Button("Remove Embedded Assets"))
		{
			Close();
			RemoveEmbeddedAssets();
		}
	}

	void RemoveEmbeddedAssets()
	{
		var path = AssetDatabase.GetAssetPath(obj);
		foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (asset != obj)
            {
				AssetDatabase.RemoveObjectFromAsset(asset);
            }
        }
	}
}
