using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
	[CustomEditor(typeof(Registry))]
	class RegistryEditor : UnityEditor.Editor
	{
		static Type featureToAdd = null;

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var mods = RegistryTools.GetAllMods();

			if (mods.Count == 0)
			{
				EditorGUILayout.LabelField("No mods have been found. Please add a mod in order to configure this registry");
				return;
			}

			var modTypeName = serializedObject.GetString("modTypeName");
			var modAssemblyName = serializedObject.GetString("__modAssemblyName");

			var modIndex = GetIndexOfMod(mods, modTypeName, modAssemblyName);

			var newIndex = EditorGUILayout.Popup("Mod", modIndex, RegistryTools.GetAllModNames());

			if (newIndex >= 0 && modTypeName != mods[newIndex].FullName)
			{
				UpdateMod(mods[newIndex]);
				modIndex = newIndex;
				modTypeName = mods[newIndex].FullName;
				modAssemblyName = mods[newIndex].Assembly.GetName().Name;
			}

			if (!string.IsNullOrEmpty(modTypeName))
			{
				var allFeatureTypes = RegistryTools.GetAllFeatures();
				EditorGUILayout.BeginVertical("Button");

				var addedFeatureCount = GetFeatureCount();

				for (int i = 0; i < addedFeatureCount; i++)
				{
					EditorGUILayout.BeginHorizontal();

					var feature = GetFeature(i);
					var featureType = GetFeatureType(i);

					if (featureType == null)
					{
						RemoveFeature(i);
						i--;
						addedFeatureCount--;
						continue;
					}

					var newFeature = EditorGUILayout.ObjectField(feature, featureType, false);
					if (newFeature != feature)
					{
						if (newFeature == null)
						{
							SetFeature(null, featureType, i);
						}
						else
						{
							SetFeature(newFeature, featureType, i);
						}
					}

					if (GUILayout.Button("X", GUILayout.MaxWidth(25)))
					{
						RemoveFeature(i);
						i--;
						addedFeatureCount--;
						continue;
					}

					EditorGUILayout.EndHorizontal();
				}

				EditorGUILayout.Space();

				if (allFeatureTypes.Count > 0)
				{
					var allFeatureNames = RegistryTools.GetAllFeatureNames();

					if (featureToAdd == null)
					{
						featureToAdd = allFeatureTypes[0];
					}

					var featureIndex = EditorGUILayout.Popup("Feature to Add", allFeatureTypes.IndexOf(featureToAdd), allFeatureNames);
					featureToAdd = allFeatureTypes[featureIndex];

					if (GUILayout.Button("Add Feature"))
					{
						AddFeature(null, featureToAdd);
					}
				}

			   EditorGUILayout.EndVertical();
			}

			serializedObject.ApplyModifiedProperties();
		}

		void UpdateMod(Type modType)
		{
			serializedObject.SetString("modTypeName", modType.FullName);
			serializedObject.SetString("__modAssemblyName", modType.Assembly.GetName().Name);

			var bundleName = $"{modType.Name.ToLower()}_bundle";

			SetAssetBundleName(bundleName, target);

			var count = GetFeatureCount();
			for (int i = 0; i < count; i++)
			{
				SetAssetBundleName(bundleName, GetFeature(i));
			}
		}

		int GetFeatureCount()
		{
			var features = serializedObject.FindProperty("features");
			return features.arraySize;
		}

		UnityEngine.Object GetFeature(int index)
		{
			var features = serializedObject.FindProperty("features");
			return features.GetArrayElementAtIndex(index).objectReferenceValue;
		}

		Type GetFeatureType(int index)
		{
			var featureTypes = serializedObject.FindProperty("featureTypeNames");
			var typeName = featureTypes.GetArrayElementAtIndex(index).stringValue;

			var featureAssembies = serializedObject.FindProperty("featureAssemblyNames");
			var assemblyName = featureAssembies.GetArrayElementAtIndex(index).stringValue;

			return TypeUtilities.NameToType(typeName, assemblyName);
		}

		void SetFeature(UnityEngine.Object feature, Type type, int index)
		{
			if (type == null)
			{
				feature = null;
				type = typeof(UnityEngine.Object);
			}
			var features = serializedObject.FindProperty("features");
			var oldFeature = features.GetArrayElementAtIndex(index).objectReferenceValue;
			if (oldFeature != null)
			{
				SetAssetBundleName("", oldFeature);
			}
			features.GetArrayElementAtIndex(index).objectReferenceValue = feature;

			var featureTypes = serializedObject.FindProperty("featureTypeNames");
			featureTypes.GetArrayElementAtIndex(index).stringValue = type.FullName;

			var featureAssembies = serializedObject.FindProperty("featureAssemblyNames");
			featureAssembies.GetArrayElementAtIndex(index).stringValue = type.Assembly.GetName().Name;

			if (feature != null)
			{
				SetAssetBundleName(GetAssetBundleName(target), feature);
			}
		}

		void AddFeature(UnityEngine.Object feature, Type type)
		{
			var features = serializedObject.FindProperty("features");
			features.arraySize++;
			features.GetArrayElementAtIndex(features.arraySize - 1).objectReferenceValue = feature;

			var featureTypes = serializedObject.FindProperty("featureTypeNames");
			featureTypes.arraySize++;
			featureTypes.GetArrayElementAtIndex(featureTypes.arraySize - 1).stringValue = type?.FullName ?? "";

			var featureAssembies = serializedObject.FindProperty("featureAssemblyNames");
			featureAssembies.arraySize++;
			featureAssembies.GetArrayElementAtIndex(featureAssembies.arraySize - 1).stringValue = type?.Assembly.GetName().Name ?? "";

			if (feature != null)
			{
				SetAssetBundleName(GetAssetBundleName(target), feature);
			}
		}

		void RemoveFeature(int index)
		{
			var feature = GetFeature(index);
			SetAssetBundleName("", feature);

			var count = GetFeatureCount();
			for (int i = index; i < count - 1; i++)
			{
				var nextFeature = GetFeature(i + 1);
				var nextFeatureType = GetFeatureType(i + 1);

				SetFeature(nextFeature, nextFeatureType, i);
			}
			serializedObject.FindProperty("features").arraySize--;
			serializedObject.FindProperty("featureTypeNames").arraySize--;
			serializedObject.FindProperty("featureAssemblyNames").arraySize--;
		}

		static void SetAssetBundleName(string bundleName, UnityEngine.Object obj)
		{
			if (obj == null)
			{
				return;
			}
			var path = AssetDatabase.GetAssetPath(obj);
			if (path != null && path != "")
			{
				var import = AssetImporter.GetAtPath(path);
				import.SetAssetBundleNameAndVariant(bundleName, import.assetBundleVariant);
			}
		}

		static string GetAssetBundleName(UnityEngine.Object obj)
		{
			var path = AssetDatabase.GetAssetPath(obj);
			if (path != null && path != "")
			{
				var import = AssetImporter.GetAtPath(path);
				return import.assetBundleName;
			}
			return "";
		}

		//Attempts to find the index of a mod in the mods list. Returns -1 if not found
		static int GetIndexOfMod(List<Type> mods, string modTypeName, string modAssemblyName)
		{
			if (string.IsNullOrEmpty(modTypeName))
			{
				return -1;
			}

			//Test 1
			if (!string.IsNullOrEmpty(modAssemblyName))
			{
				for (int i = 0; i < mods.Count; i++)
				{
					if (mods[i].Assembly.GetName().Name == modAssemblyName && mods[i].FullName == modTypeName)
					{
						return i;
					}
				}
			}

			//Test 2
			for (int i = 0; i < mods.Count; i++)
			{
				if (mods[i].FullName == modTypeName)
				{
					return i;
				}
			}

			return -1;
		}
	}
}
