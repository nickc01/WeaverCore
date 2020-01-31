using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using ViridianLink.Core;
using ViridianLink.Helpers;

namespace ViridianLink.Editor.Visual
{
    [CustomEditor(typeof(Registry))]
    public class RegistryEditorScript : UnityEditor.Editor
    {
        List<Type> ValidMods;
        string[] ModNames;
        Assembly[] assemblies;
        Dictionary<string, Assembly> assemblyTable;

        List<Type> Features;
        string[] FeatureNames;

        void OnEnable()
        {
            ValidMods = new List<Type>();
            Features = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(IViridianMod).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsInterface && !typeof(ViridianLinkMod).IsAssignableFrom(type))
                    {
                        ValidMods.Add(type);
                    }
                    if (typeof(Feature).IsAssignableFrom(type) && type != typeof(Feature) && !type.IsGenericTypeDefinition && !type.IsInterface && type.IsAbstract)
                    {
                        Debug.Log("Feature = " + type.Name);
                        Features.Add(type);
                    }
                }
            }
            ModNames = new string[ValidMods.Count];
            for (int i = 0; i < ValidMods.Count; i++)
            {
                ModNames[i] = ValidMods[i].Name;
            }
            Features.Sort();
            FeatureNames = new string[Features.Count];
            for (int i = 0; i < Features.Count; i++)
            {
                FeatureNames[i] = ObjectNames.NicifyVariableName(Features[i].Name);
            }

            assemblies = AppDomain.CurrentDomain.GetAssemblies();
            assemblyTable = new Dictionary<string, Assembly>();
            foreach (var assembly in assemblies)
            {
                assemblyTable.Add(assembly.FullName, assembly);
            }
        }

        //Gets the index of where the string is in ModNames. Returns -1 if not found
        int GetIndex(string value)
        {
            for (int i = 0; i < ModNames.GetLength(0); i++)
            {
                if (ModNames[i] == value)
                {
                    return i;
                }
            }
            return -1;
        }

        void SetBundleForObject(UnityEngine.Object obj,string bundleName)
        {
            //FIX by making it not dependent on registry target
            Debug.Log("OBJ = " + obj);
            Debug.Log("Bundle Name = " + bundleName);
            if (obj != null)
            {
                var registry = (Registry)target;
                var path = AssetDatabase.GetAssetPath(obj);
                Debug.Log("Path = " + path);
                if (path != null && path != "")
                {
                    Debug.Log("Bundle Set");
                    AssetImporter.GetAtPath(path).SetAssetBundleNameAndVariant(bundleName, "");
                }
            }
        }

        void RenderFeatures(SerializedProperty features)
        {
            var featuresRaw = serializedObject.FindProperty("featuresRaw");
            EditorGUILayout.BeginVertical("Button");
            for (int i = 0; i < features.arraySize; i++)
            {
                var currentProp = features.GetArrayElementAtIndex(i);
                var objectProp = currentProp.FindPropertyRelative("feature");

                EditorGUILayout.BeginHorizontal();
                var assemblyName = currentProp.FindPropertyRelative("FullAssemblyName").stringValue;
                var typeName = currentProp.FindPropertyRelative("FullTypeName").stringValue;
                var type = assemblyTable[assemblyName].GetType(typeName);
                if (type == null)
                {
                    type = typeof(Feature);
                }
                var oldValue = objectProp.objectReferenceValue;
                objectProp.objectReferenceValue = EditorGUILayout.ObjectField(oldValue,type, false);
                if (objectProp.objectReferenceValue != oldValue)
                {
                    featuresRaw.GetArrayElementAtIndex(i).objectReferenceValue = objectProp.objectReferenceValue;
                    var modTypeProp = serializedObject.FindProperty("modTypeName");
                    Debug.Log("Updating Bundles");
                    SetBundleForObject((objectProp.objectReferenceValue as Feature)?.gameObject, Regex.Match(modTypeProp.stringValue, @"([^.]+?)\.?$").Groups[0].Value + "_bundle");
                }
                if (GUILayout.Button("X",GUILayout.MaxWidth(25)))
                {
                    features.DeleteArrayElementAtIndex(i);
                    featuresRaw.DeleteArrayElementAtIndex(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();

            var index = serializedObject.FindProperty("selectedFeatureIndex");

            var changedIndex = EditorGUILayout.Popup("Feature to Add", index.intValue, FeatureNames);
            index.intValue = changedIndex;
            if (GUILayout.Button("Add Feature"))
            {
                features.InsertArrayElementAtIndex(features.arraySize);
                featuresRaw.InsertArrayElementAtIndex(featuresRaw.arraySize);
                var last = features.GetArrayElementAtIndex(features.arraySize - 1);
                last.FindPropertyRelative("feature").objectReferenceValue = null;
                featuresRaw.GetArrayElementAtIndex(featuresRaw.arraySize - 1).objectReferenceValue = null;
                last.FindPropertyRelative("FullTypeName").stringValue = Features[changedIndex].FullName;
                last.FindPropertyRelative("FullAssemblyName").stringValue = Features[changedIndex].Assembly.FullName;
            }
            EditorGUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty iter = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iter.NextVisible(enterChildren))
            {
                if (iter.name == "mod")
                {
                    int selection = GetIndex(iter.stringValue);
                    if (selection == -1)
                    {
                        if (ModNames.GetLength(0) == 0)
                        {
                            EditorGUILayout.LabelField("NO MODS FOUND");
                            return;
                        }
                        else
                        {
                            selection = 0;
                        }
                    }
                    var newSelection = EditorGUILayout.Popup(iter.displayName, selection, ModNames);
                    iter.stringValue = ModNames[newSelection];

                    var modAssemblyProp = serializedObject.FindProperty("modAssemblyName");
                    var modTypeProp = serializedObject.FindProperty("modTypeName");

                    if (selection != newSelection || modAssemblyProp.stringValue == "")
                    {
                        modAssemblyProp.stringValue = ValidMods[newSelection].Assembly.FullName;
                        modTypeProp.stringValue = ValidMods[newSelection].FullName;
                        Debug.Log("Updating Bundles2");
                        var features = serializedObject.FindProperty("features");
                        for (int i = 0; i < features.arraySize; i++)
                        {
                            var obj = features.GetArrayElementAtIndex(i).FindPropertyRelative("feature").objectReferenceValue;
                            SetBundleForObject((obj as Feature)?.gameObject,Regex.Match(modTypeProp.stringValue, @"([^.]+?)\.?$").Groups[0].Value + "_bundle");
                        }
                    }
                }
                else if (iter.name == "features")
                {
                    RenderFeatures(iter);
                }
                else if (iter.name == "selectedFeatureIndex" || iter.name == "modAssemblyName" || iter.name == "modTypeName" || iter.name == "featuresRaw")
                {

                }
                else
                {
                    using (new EditorGUI.DisabledScope("m_Script" == iter.propertyPath))
                    {
                        EditorGUILayout.PropertyField(iter, true, new GUILayoutOption[0]);
                    }
                }
                enterChildren = false;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
