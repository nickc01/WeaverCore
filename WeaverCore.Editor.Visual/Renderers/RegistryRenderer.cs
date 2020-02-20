using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using WeaverCore.Editor.Visual.Internal;
using WeaverCore.Editor.Visual.Helpers;
using UnityEngine;
using WeaverCore.Helpers;

namespace WeaverCore.Editor.Visual.Renderers
{
	[CustomEditor(typeof(Registry))]
	public class RegistryRenderer : UnityEditor.Editor
	{
		RegistryChecker checker;

		void OnEnable()
		{
			checker = new RegistryChecker(serializedObject);
			checker.Check();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			foreach (var property in serializedObject.Iterator("selectedModIndex","selectedFeatureIndex","modAssemblyName","modTypeName","featuresRaw","modListHashCode"))
			{
				if (property.name == "modName")
				{
					var name = serializedObject.GetString("modName");
					if (name == "")
					{
						EditorGUILayout.LabelField("NO MODS FOUND");
						return;
					}
					else
					{
						var mod = checker.GetMod();
						var modIndex = checker.GetModIndex(mod);
						var newIndex = EditorGUILayout.Popup("Mod", modIndex, checker.ModNames);
						if (newIndex != modIndex)
						{
							checker.SetMod(newIndex);
						}
					}
				}
				else if (property.name == "features")
				{
					EditorGUILayout.BeginVertical("Button");

					foreach (var feature in checker.GetAllFeatures())
					{
						EditorGUILayout.BeginHorizontal();

						var featureType = Helpers.Features.FindFeatureType(feature.Value, checker.FeatureList);
						if (featureType == null)
						{
							checker.DeleteFeature(feature.Index);
							continue;
						}

						var newFeature = EditorGUILayout.ObjectField(feature.Value.feature, featureType, false);
						if (newFeature != feature.Value.feature)
						{
							checker.SetFeature(feature.Index,newFeature as Feature,featureType);
						}

						if (GUILayout.Button("X", GUILayout.MaxWidth(25)))
						{
							checker.DeleteFeature(feature.Index);
						}

						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.Space();

					var index = serializedObject.SetInt("selectedFeatureIndex", EditorGUILayout.Popup("Feature to Add",serializedObject.GetInt("selectedFeatureIndex"),checker.FeatureNames));

					if (GUILayout.Button("Add Feature"))
					{
						checker.AddFeature(null, checker.FeatureList[index]);
					}

					EditorGUILayout.EndVertical();
				}
				else
				{
					using (new EditorGUI.DisabledScope("m_Script" == property.propertyPath))
					{
						EditorGUILayout.PropertyField(property, true, new GUILayoutOption[0]);
					}
				}
			}
			serializedObject.ApplyModifiedProperties();
		}
	}
}
