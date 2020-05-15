using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace WeaverCore.Editor.Helpers
{
    public static class LayerChanger
    {
		public struct LayerIndex
		{
			public readonly int Index;
			public readonly string Name;

			public LayerIndex(int index, string name)
			{
				Index = index;
				Name = name;
			}
		}

		public static string GetLayerName(int layerIndex)
		{
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty layers = tagManager.FindProperty("layers");
			return layers.GetArrayElementAtIndex(layerIndex).stringValue;
		}

		public static void SetLayerName(int layerIndex, string name)
		{
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty layers = tagManager.FindProperty("layers");
			layers.GetArrayElementAtIndex(layerIndex).stringValue = name;
			tagManager.ApplyModifiedProperties();
		}

		public static IEnumerable<LayerIndex> GetAllLayers()
		{
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty layers = tagManager.FindProperty("layers");
			for (int i = 0; i < layers.arraySize; i++)
			{
				yield return new LayerIndex(i, layers.GetArrayElementAtIndex(i).stringValue);
			}
		}

		public static bool ContainsTag(string tag)
		{
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty tagsProp = tagManager.FindProperty("tags");

			for (int i = 0; i < tagsProp.arraySize; i++)
			{
				SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
				if (t.stringValue.Equals(tag)) 
				{ 
					return true; 
				}
			}
			return false;
		}

		public static void AddTag(string tag)
		{
			SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty tagsProp = tagManager.FindProperty("tags");

			tagsProp.InsertArrayElementAtIndex(0);

			SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
			n.stringValue = tag;

			tagManager.ApplyModifiedProperties();
		}

		public static void AddIfUnique(string tag)
		{
			if (!ContainsTag(tag))
			{
				AddTag(tag);
			}
		}
	}
}
