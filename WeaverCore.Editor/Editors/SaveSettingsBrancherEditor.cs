using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Settings;

[CustomEditor(typeof(SaveSettingsBrancher))]
public class SaveSettingsBrancherEditor : Editor
{
    SerializedProperty saveSettingsProperty;
    SerializedProperty runOnStartProperty;
    SerializedProperty fieldNamesProperty;
    SerializedProperty fieldTypeNamesProperty;
    SerializedProperty expectedValueJsonProperty;
    SerializedProperty onExpectedValuesProperty;

    void OnEnable()
    {
        saveSettingsProperty = serializedObject.FindProperty("saveSettings");
        runOnStartProperty = serializedObject.FindProperty("runOnStart");
        fieldNamesProperty = serializedObject.FindProperty("fieldNames");
        fieldTypeNamesProperty = serializedObject.FindProperty("fieldTypeNames");
        expectedValueJsonProperty = serializedObject.FindProperty("expectedValueJson");
        onExpectedValuesProperty = serializedObject.FindProperty("onExpectedValues");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(saveSettingsProperty);
        EditorGUILayout.PropertyField(runOnStartProperty);

        var settings = saveSettingsProperty.objectReferenceValue as SaveSpecificSettings;
        var settingsType = settings != null ? settings.GetType() : null;

        EditorGUILayout.Space();
        DrawEvents(settings, settingsType);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Activate Now"))
            {
                foreach (var targetObject in targets)
                {
                    ((SaveSettingsBrancher)targetObject).Activate();
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawEvents(SaveSpecificSettings settings, Type settingsType)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Branch Events", EditorStyles.boldLabel);

        if (settingsType == null)
        {
            EditorGUILayout.HelpBox("Assign a SaveSpecificSettings asset to choose fields.", MessageType.Info);
        }

        for (int i = 0; i < fieldNamesProperty.arraySize; i++)
        {
            DrawBranchEvent(settings, settingsType, i);
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add Event"))
        {
            AddEvent(settings, settingsType);
        }

        EditorGUILayout.EndVertical();
    }

    void DrawBranchEvent(SaveSpecificSettings settings, Type settingsType, int index)
    {
        var fieldNameProperty = fieldNamesProperty.GetArrayElementAtIndex(index);
        var fieldTypeNameProperty = fieldTypeNamesProperty.GetArrayElementAtIndex(index);
        var expectedValueProperty = expectedValueJsonProperty.GetArrayElementAtIndex(index);
        var eventProperty = onExpectedValuesProperty.GetArrayElementAtIndex(index);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Event {index + 1}", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Remove", GUILayout.Width(70f)))
        {
            RemoveEvent(index);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        EditorGUILayout.EndHorizontal();

        if (settingsType == null)
        {
            EditorGUILayout.HelpBox("Assign a SaveSpecificSettings asset before configuring this event.", MessageType.Info);
            EditorGUILayout.PropertyField(eventProperty);
            EditorGUILayout.EndVertical();
            return;
        }

        var fields = SaveSettingsBrancher.GetSupportedFields(settingsType);
        if (fields.Count == 0)
        {
            EditorGUILayout.HelpBox("No supported serializable fields were found on this SaveSpecificSettings type.", MessageType.Warning);
            EditorGUILayout.PropertyField(eventProperty);
            EditorGUILayout.EndVertical();
            return;
        }

        var selectedField = SaveSettingsBrancher.GetSupportedField(settingsType, fieldNameProperty.stringValue);
        int currentIndex = GetFieldIndex(fields, selectedField ?? fields[0]);

        string[] fieldOptions = new string[fields.Count];
        for (int i = 0; i < fields.Count; i++)
        {
            fieldOptions[i] = $"{fields[i].Name} ({SaveSettingsBrancher.GetFriendlyTypeName(fields[i].FieldType)})";
        }

        EditorGUI.BeginChangeCheck();
        int newIndex = EditorGUILayout.Popup("Field", currentIndex, fieldOptions);
        if (EditorGUI.EndChangeCheck() || selectedField == null)
        {
            selectedField = fields[newIndex];
            ApplyFieldSelection(fieldNameProperty, fieldTypeNameProperty, expectedValueProperty, settings, selectedField);
        }

        if (selectedField != null)
        {
            fieldTypeNameProperty.stringValue = selectedField.FieldType.AssemblyQualifiedName;
            EditorGUILayout.LabelField("Field Type", SaveSettingsBrancher.GetFriendlyTypeName(selectedField.FieldType));
            DrawExpectedValueEditor(expectedValueProperty, settings, selectedField);
        }

        EditorGUILayout.PropertyField(eventProperty);
        EditorGUILayout.EndVertical();
    }

    void DrawExpectedValueEditor(SerializedProperty expectedValueProperty, SaveSpecificSettings settings, FieldInfo field)
    {
        var fieldType = field.FieldType;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Use Current Value"))
        {
            expectedValueProperty.stringValue = SaveSettingsBrancher.SerializeValue(field.GetValue(settings), fieldType);
        }
        if (GUILayout.Button("Reset Default"))
        {
            expectedValueProperty.stringValue = SaveSettingsBrancher.SerializeValue(SaveSettingsBrancher.GetDefaultValue(fieldType), fieldType);
        }
        EditorGUILayout.EndHorizontal();

        if (CanDrawInline(fieldType))
        {
            if (!SaveSettingsBrancher.TryDeserializeValue(expectedValueProperty.stringValue, fieldType, out var currentValue))
            {
                currentValue = SaveSettingsBrancher.GetDefaultValue(fieldType);
                EditorGUILayout.HelpBox("Expected value data was invalid. A default value is being shown.", MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();
            var newValue = DrawInlineField("Expected Value", currentValue, fieldType);
            if (EditorGUI.EndChangeCheck())
            {
                expectedValueProperty.stringValue = SaveSettingsBrancher.SerializeValue(newValue, fieldType);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("This is a complex serializable type. Edit the raw JSON value below.", MessageType.Info);
            expectedValueProperty.stringValue = EditorGUILayout.TextArea(expectedValueProperty.stringValue, GUILayout.MinHeight(80f));

            if (!string.IsNullOrWhiteSpace(expectedValueProperty.stringValue) &&
                !SaveSettingsBrancher.TryDeserializeValue(expectedValueProperty.stringValue, fieldType, out _))
            {
                EditorGUILayout.HelpBox("The JSON value could not be parsed for the selected field type.", MessageType.Error);
            }
        }
    }

    void AddEvent(SaveSpecificSettings settings, Type settingsType)
    {
        int index = fieldNamesProperty.arraySize;
        fieldNamesProperty.InsertArrayElementAtIndex(index);
        fieldTypeNamesProperty.InsertArrayElementAtIndex(index);
        expectedValueJsonProperty.InsertArrayElementAtIndex(index);
        onExpectedValuesProperty.InsertArrayElementAtIndex(index);

        fieldNamesProperty.GetArrayElementAtIndex(index).stringValue = string.Empty;
        fieldTypeNamesProperty.GetArrayElementAtIndex(index).stringValue = string.Empty;
        expectedValueJsonProperty.GetArrayElementAtIndex(index).stringValue = string.Empty;

        var eventProperty = onExpectedValuesProperty.GetArrayElementAtIndex(index);
        var callsProperty = eventProperty.FindPropertyRelative("m_PersistentCalls.m_Calls");
        if (callsProperty != null)
        {
            callsProperty.arraySize = 0;
        }

        if (settingsType != null)
        {
            var fields = SaveSettingsBrancher.GetSupportedFields(settingsType);
            if (fields.Count > 0)
            {
                ApplyFieldSelection(
                    fieldNamesProperty.GetArrayElementAtIndex(index),
                    fieldTypeNamesProperty.GetArrayElementAtIndex(index),
                    expectedValueJsonProperty.GetArrayElementAtIndex(index),
                    settings,
                    fields[0]);
            }
        }
    }

    void RemoveEvent(int index)
    {
        fieldNamesProperty.DeleteArrayElementAtIndex(index);
        fieldTypeNamesProperty.DeleteArrayElementAtIndex(index);
        expectedValueJsonProperty.DeleteArrayElementAtIndex(index);
        onExpectedValuesProperty.DeleteArrayElementAtIndex(index);
    }

    void ApplyFieldSelection(SerializedProperty fieldNameProperty, SerializedProperty fieldTypeNameProperty, SerializedProperty expectedValueJsonProperty, SaveSpecificSettings settings, FieldInfo field)
    {
        fieldNameProperty.stringValue = field.Name;
        fieldTypeNameProperty.stringValue = field.FieldType.AssemblyQualifiedName;
        expectedValueJsonProperty.stringValue = SaveSettingsBrancher.SerializeValue(field.GetValue(settings), field.FieldType);
    }

    int GetFieldIndex(System.Collections.Generic.IReadOnlyList<FieldInfo> fields, FieldInfo selectedField)
    {
        for (int i = 0; i < fields.Count; i++)
        {
            if (fields[i].Name == selectedField.Name)
            {
                return i;
            }
        }

        return 0;
    }

    static bool CanDrawInline(Type fieldType)
    {
        return fieldType == typeof(bool) ||
            fieldType == typeof(int) ||
            fieldType == typeof(float) ||
            fieldType == typeof(double) ||
            fieldType == typeof(long) ||
            fieldType == typeof(string) ||
            fieldType == typeof(Vector2) ||
            fieldType == typeof(Vector2Int) ||
            fieldType == typeof(Vector3) ||
            fieldType == typeof(Vector3Int) ||
            fieldType == typeof(Vector4) ||
            fieldType == typeof(Rect) ||
            fieldType == typeof(RectInt) ||
            fieldType == typeof(Bounds) ||
            fieldType == typeof(BoundsInt) ||
            fieldType == typeof(Color) ||
            typeof(AnimationCurve).IsAssignableFrom(fieldType) ||
            typeof(Gradient).IsAssignableFrom(fieldType) ||
            fieldType.IsEnum;
    }

    static object DrawInlineField(string label, object value, Type fieldType)
    {
        if (fieldType == typeof(bool))
        {
            return EditorGUILayout.Toggle(label, value is bool boolValue && boolValue);
        }
        if (fieldType == typeof(int))
        {
            return EditorGUILayout.IntField(label, value is int intValue ? intValue : default);
        }
        if (fieldType == typeof(float))
        {
            return EditorGUILayout.FloatField(label, value is float floatValue ? floatValue : default);
        }
        if (fieldType == typeof(double))
        {
            return EditorGUILayout.DoubleField(label, value is double doubleValue ? doubleValue : default);
        }
        if (fieldType == typeof(long))
        {
            return EditorGUILayout.LongField(label, value is long longValue ? longValue : default);
        }
        if (fieldType == typeof(string))
        {
            return EditorGUILayout.TextField(label, value as string);
        }
        if (fieldType == typeof(Vector2))
        {
            return EditorGUILayout.Vector2Field(label, value is Vector2 vectorValue ? vectorValue : default);
        }
        if (fieldType == typeof(Vector2Int))
        {
            return EditorGUILayout.Vector2IntField(label, value is Vector2Int vectorValue ? vectorValue : default);
        }
        if (fieldType == typeof(Vector3))
        {
            return EditorGUILayout.Vector3Field(label, value is Vector3 vectorValue ? vectorValue : default);
        }
        if (fieldType == typeof(Vector3Int))
        {
            return EditorGUILayout.Vector3IntField(label, value is Vector3Int vectorValue ? vectorValue : default);
        }
        if (fieldType == typeof(Vector4))
        {
            return EditorGUILayout.Vector4Field(label, value is Vector4 vectorValue ? vectorValue : default);
        }
        if (fieldType == typeof(Rect))
        {
            return EditorGUILayout.RectField(label, value is Rect rectValue ? rectValue : default);
        }
        if (fieldType == typeof(RectInt))
        {
            return EditorGUILayout.RectIntField(label, value is RectInt rectValue ? rectValue : default);
        }
        if (fieldType == typeof(Bounds))
        {
            return EditorGUILayout.BoundsField(label, value is Bounds boundsValue ? boundsValue : default);
        }
        if (fieldType == typeof(BoundsInt))
        {
            return EditorGUILayout.BoundsIntField(label, value is BoundsInt boundsValue ? boundsValue : default);
        }
        if (fieldType == typeof(Color))
        {
            return EditorGUILayout.ColorField(label, value is Color colorValue ? colorValue : default);
        }
        if (typeof(AnimationCurve).IsAssignableFrom(fieldType))
        {
            return EditorGUILayout.CurveField(label, value as AnimationCurve);
        }
        if (typeof(Gradient).IsAssignableFrom(fieldType))
        {
            return EditorGUILayout.GradientField(label, value as Gradient);
        }
        if (fieldType.IsEnum)
        {
            return EditorGUILayout.EnumPopup(label, value as Enum);
        }

        return value;
    }
}
