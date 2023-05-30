/*using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using WeaverCore.Editor.Utilities;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
    [CustomEditor(typeof(CustomCharm))]
	class CustomCharmEditor : UnityEditor.Editor 
	{
        static Type[] charmTypes = null;
        static string[] noCharmsArray = new string[]
        {
            "Error: No Charms Found. Create a class that inherits from IWeaverCharm"
        };
        static string[] charmNames = null;
        int currentCharmIndex = -1;


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var charmClassName = serializedObject.GetString("charmClassName");
            var charmAssemblyName = serializedObject.GetString("__charmClassAssemblyName");

            if (charmTypes == null)
            {
                charmTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
                {
                    return a.GetTypes().Where(t => typeof(IWeaverCharm).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericTypeDefinition);
                }).ToArray();

                charmNames = charmTypes.Select(type => StringUtilities.Prettify(type.FullName)).ToArray();
            }

            if (currentCharmIndex == -1)
            {
                for (int i = 0; i < charmTypes.Length; i++)
                {
                    var type = charmTypes[i];
                    if (type.FullName == charmClassName && type.Assembly.FullName == charmAssemblyName)
                    {
                        currentCharmIndex = i;
                        break;
                    }
                }
                if (currentCharmIndex == -1)
                {
                    currentCharmIndex = 0;
                }
                UpdateCharmType();
            }

            if (charmTypes.Length == 0)
            {
                EditorGUILayout.Popup("Charm", currentCharmIndex, noCharmsArray);
            }
            else
            {
                var newIndex = EditorGUILayout.Popup("Charm", currentCharmIndex, charmNames);

                if (currentCharmIndex != newIndex)
                {
                    currentCharmIndex = newIndex;
                    UpdateCharmType();
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_charmSprite"));

            serializedObject.ApplyModifiedProperties();
        }

        void UpdateCharmType()
        {
            serializedObject.SetString("charmClassName", charmTypes[currentCharmIndex].FullName);
            serializedObject.SetString("__charmClassAssemblyName", charmTypes[currentCharmIndex].Assembly.GetName().Name);
        }
    }
}*/
