using UnityEditor;
using UnityEngine;
using WeaverCore;

[CustomEditor(typeof(PreloadRegistry))]
public class PreloadRegistryEditor : Editor
{
    SerializedProperty sceneNamesProperty;
    SerializedProperty objectPathsProperty;

    void OnEnable()
    {
        sceneNamesProperty = serializedObject.FindProperty("sceneNames");
        objectPathsProperty = serializedObject.FindProperty("objectPaths");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.HelpBox("Add scene/object path pairs to preload. Object paths should be relative to the scene root.", MessageType.Info);

        for (int i = 0; i < sceneNamesProperty.arraySize; i++)
        {
            DrawEntry(i);
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add Preload Entry"))
        {
            AddEntry();
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawEntry(int index)
    {
        var sceneProperty = sceneNamesProperty.GetArrayElementAtIndex(index);
        var pathProperty = objectPathsProperty.GetArrayElementAtIndex(index);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Entry {index + 1}", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Remove", GUILayout.Width(70f)))
        {
            RemoveEntry(index);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        EditorGUILayout.EndHorizontal();

        sceneProperty.stringValue = EditorGUILayout.TextField("Scene Name", sceneProperty.stringValue);
        pathProperty.stringValue = EditorGUILayout.TextField("Object Path", pathProperty.stringValue);

        EditorGUILayout.EndVertical();
    }

    void AddEntry()
    {
        int index = sceneNamesProperty.arraySize;
        sceneNamesProperty.InsertArrayElementAtIndex(index);
        objectPathsProperty.InsertArrayElementAtIndex(index);

        sceneNamesProperty.GetArrayElementAtIndex(index).stringValue = string.Empty;
        objectPathsProperty.GetArrayElementAtIndex(index).stringValue = string.Empty;
    }

    void RemoveEntry(int index)
    {
        sceneNamesProperty.DeleteArrayElementAtIndex(index);
        objectPathsProperty.DeleteArrayElementAtIndex(index);
    }
}
