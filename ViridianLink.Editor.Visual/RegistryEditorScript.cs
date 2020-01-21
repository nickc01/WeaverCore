using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


        void OnEnable()
        {
            ValidMods = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(IViridianMod).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsInterface)
                    {
                        ValidMods.Add(type);
                    }
                }
            }
            ModNames = new string[ValidMods.Count];
            for (int i = 0; i < ValidMods.Count; i++)
            {
                ModNames[i] = ValidMods[i].Name;
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

        void RenderFeatures(SerializedProperty features)
        {
            EditorGUILayout.BeginVertical("Button");
            for (int i = 0; i < features.arraySize; i++)
            {
                var prop = features.GetArrayElementAtIndex(i);
                Debug.Log("Prop = " + prop.name);
                using (new EditorGUI.DisabledScope("m_Script" == features.propertyPath))
                {
                    EditorGUILayout.PropertyField(features, true, new GUILayoutOption[0]);
                }
            }
            EditorGUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            Debug.Log("IN INSPECTOR");
            serializedObject.Update();
            SerializedProperty iter = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iter.NextVisible(enterChildren))
            {
                Debug.Log("Drawing = " + iter.name);
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
                    selection = EditorGUILayout.Popup(iter.displayName, selection, ModNames);
                    iter.stringValue = ModNames[selection];
                }
                else if (iter.name == "features")
                {
                    RenderFeatures(iter);
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
