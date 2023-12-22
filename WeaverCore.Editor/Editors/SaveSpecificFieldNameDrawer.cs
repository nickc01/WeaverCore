using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Editor.Compilation;
using WeaverCore.Settings;
using WeaverCore.Utilities;

// Tell the MyRangeDrawer that it is a drawer for properties with the MyRangeAttribute.
[CustomPropertyDrawer(typeof(SaveSpecificFieldName))]
public class SaveSpecificFieldNameDrawer : PropertyDrawer
{
    static Dictionary<string, Type> nameToTypeDictionary = new Dictionary<string, Type>();
    //static Dictionary<Type, string[]> fieldNamesCache = new Dictionary<Type, string[]>();
    static Dictionary<Type, FieldInfo[]> fieldsCache = new Dictionary<Type, FieldInfo[]>();
    static Dictionary<string, TooltipAttribute> tooltipCache = new Dictionary<string, TooltipAttribute>();
    // Draw the property inside the given rect
    /*void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // First get the attribute since it contains the range for the slider
        MyRangeAttribute range = (MyRangeAttribute)attribute;

        // Now draw the property as a Slider or an IntSlider based on whether it's a float or integer.
        if (property.propertyType == SerializedPropertyType.Float)
            EditorGUI.Slider(position, property, range.min, range.max, label);
        else if (property.propertyType == SerializedPropertyType.Integer)
            EditorGUI.IntSlider(position, property, (int)range.min, (int)range.max, label);
        else
            EditorGUI.LabelField(position, label.text, "Use MyRange with float or int.");
    }*/

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // First get the attribute since it contains the range for the slider
        SaveSpecificFieldName fieldNameAttribute = (SaveSpecificFieldName)attribute;

        var scriptProp = property.serializedObject.FindProperty("m_Script");

        var script = scriptProp.objectReferenceValue as MonoScript;

        if (script == null)
        {
            return;
        }
        var typeName = script.name;
        var typeNamespace = MonoScriptExtensions.GetScriptNamespace(script);
        var typeAssemblyName = MonoScriptExtensions.GetScriptAssemblyName(script).Replace(".dll", "");

        Type scriptType;

        if (!nameToTypeDictionary.TryGetValue($"{typeAssemblyName}:{typeNamespace}.{typeName}", out scriptType))
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == typeAssemblyName)
                {
                    scriptType = assembly.GetType($"{typeNamespace}.{typeName}");
                    if (scriptType != null)
                    {
                        nameToTypeDictionary.Add($"{typeAssemblyName}:{typeNamespace}.{typeName}", scriptType);
                        break;
                    }
                }
            }
        }

        if (!tooltipCache.TryGetValue($"{typeAssemblyName}:{typeNamespace}.{typeName}:{property.name}", out var tooltip))
        {
            var propField = scriptType.GetField(property.name,BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        if (property.propertyType != SerializedPropertyType.String)
        {
            WeaverLog.LogError($"Error: The attribute \"SaveSpecificFieldName\" can only be applied to string fields. The field \"{property.name}\" in type \"{typeName}\" is not a string field");
            return;
        }

        /*FieldInfo FindField(Type type, string saveSettingsName)
        {
            var settingsStorageField = type.GetField(saveSettingsName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            if (settingsStorageField != null && typeof(SaveSpecificSettings).IsAssignableFrom(settingsStorageField.FieldType))
            {
                return settingsStorageField;
            }
            else
            {
                foreach (var subType in type.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public))
                {
                    WeaverLog.Log("SUB TYPE = " + subType.FullName);
                    settingsStorageField = FindField(subType, saveSettingsName);
                    if (settingsStorageField != null)
                    {
                        return settingsStorageField;
                    }
                }
                return null;
            }
        }*/

        var settingsStorageProp = property.serializedObject.FindProperty(fieldNameAttribute.SaveSettingsName);

        Type settingsFieldHostType = scriptType;


        FieldInfo settingsStorageField = null;

        int counter = 10;

        while (settingsStorageField == null)
        {
            settingsStorageField = settingsFieldHostType.GetField(fieldNameAttribute.SaveSettingsName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (settingsStorageField == null)
            {
                settingsFieldHostType = settingsFieldHostType.BaseType;

                if (settingsFieldHostType == typeof(UnityEngine.Object))
                {
                    break;
                }
                
            }

            counter--;
            if (counter == 0)
            {
                break;
            }
        }

        if (settingsStorageProp == null)
        {
            WeaverLog.LogError($"Error: The serialized field \"{fieldNameAttribute.SaveSettingsName}\" could not be found on the script \"{scriptType.FullName}\"");
            WeaverLog.LogError($"The above error is likely a misconfigured \"SaveSpecificFieldName\" attribute on the \"{property.name}\" field in type \"{scriptType.FullName}\"");
            return;
        }

        if (!typeof(SaveSpecificSettings).IsAssignableFrom(settingsStorageField.FieldType))
        {
            WeaverLog.LogError($"Error: The serialized field \"{fieldNameAttribute.SaveSettingsName}\" is not of type \"{typeof(SaveSpecificSettings).Name}\" or doesn't derive from it");
        }

        var saveSpecificSettings = settingsStorageProp.objectReferenceValue as SaveSpecificSettings;

        if (saveSpecificSettings == null)
        {
            return;
        }

        var saveSpecificSettingsType = saveSpecificSettings.GetType();

        if (!fieldsCache.TryGetValue(saveSpecificSettingsType, out var fields))
        {
            fields = saveSpecificSettingsType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            fieldsCache.Add(saveSpecificSettingsType, fields);
        }

        /*if (!fieldNamesCache.TryGetValue(saveSpecificSettingsType, out var fieldNames))
        {
            fieldNames = fields.Select(f => f.Name).ToArray();
            fieldNamesCache.Add(saveSpecificSettingsType, fieldNames);
        }*/

        var validFields = fields.Where(f => fieldNameAttribute.ExpectedFieldType.IsAssignableFrom(f.FieldType)).ToArray();
        var validNames = validFields.Select(f => f.Name).Prepend("Not Specified").ToArray();

        var oldValue = property.stringValue;
        if (string.IsNullOrEmpty(oldValue))
        {
            oldValue = "Not Specified";
        }
        int index = validNames.IndexOf(oldValue);

        if (index == -1)
        {
            validNames = validNames.Prepend(oldValue).ToArray();
            index = 0;
        }

        index = EditorGUI.Popup(position, property.displayName, index, validNames);

        if (validNames[index] == "Not Specified")
        {
            property.stringValue = "";
        }
        else
        {
            property.stringValue = validNames[index];
        }
    }
}