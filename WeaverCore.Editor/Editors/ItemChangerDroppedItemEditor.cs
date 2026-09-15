using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Utilities;

[CustomEditor(typeof(ItemChangerDroppedItem))]
public class ItemChangerDroppedItemEditor : Editor
{
    static string[] cachedFallbackItemNames;

    SerializedProperty itemNameProperty;
    SerializedProperty containerTypeProperty;
    SerializedProperty messageTypeProperty;
    SerializedProperty flingTypeProperty;

    void OnEnable()
    {
        itemNameProperty = serializedObject.FindProperty("itemName");
        containerTypeProperty = serializedObject.FindProperty("containerType");
        messageTypeProperty = serializedObject.FindProperty("messageType");
        flingTypeProperty = serializedObject.FindProperty("flingType");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script", "itemName", "containerType", "messageType", "flingType");

        EditorGUILayout.Space();
        DrawItemNameField();
        DrawStringOptionsField(containerTypeProperty, "Container Type", new[] { "Unknown", "Shiny", "Enemy", "Chest", "GrubJar", "GeoRock", "Tablet", "Shop", "Mimic", "Totem", "Bluggsac" });
        DrawStringOptionsField(messageTypeProperty, "Message Type", new[] { "None", "Corner", "Big", "Lore", "Any" });
        DrawStringOptionsField(flingTypeProperty, "Fling Type", new[] { "Everywhere", "StraightUp", "DirectDeposit" });

        serializedObject.ApplyModifiedProperties();
    }

    void DrawItemNameField()
    {
        var itemNames = ItemChangerUtilities.IsAvailable ? ItemChangerUtilities.GetAllItemNames() : GetFallbackItemNames();
        Array.Sort(itemNames, StringComparer.Ordinal);

        if (itemNames.Length > 0)
        {
            int selectedIndex = Array.IndexOf(itemNames, itemNameProperty.stringValue);
            int newIndex = EditorGUILayout.Popup("Item Name", Mathf.Max(selectedIndex, 0), itemNames);
            itemNameProperty.stringValue = itemNames[newIndex];
        }
        else
        {
            EditorGUILayout.HelpBox("No ItemChanger item names were available from the live assembly or the copied JSON fallback.", MessageType.Warning);
        }

        itemNameProperty.stringValue = EditorGUILayout.TextField("Manual Item Name", itemNameProperty.stringValue);
    }

    static string[] GetFallbackItemNames()
    {
        if (cachedFallbackItemNames != null)
        {
            return cachedFallbackItemNames;
        }

        try
        {
            string jsonPath = Path.Combine(Application.dataPath, "WeaverCore", "WeaverCore.Editor", "ItemChanger", "items.json");
            if (!File.Exists(jsonPath))
            {
                return cachedFallbackItemNames = Array.Empty<string>();
            }

            var root = JObject.Parse(File.ReadAllText(jsonPath));
            cachedFallbackItemNames = root.Properties().Select(property => property.Name).ToArray();
            return cachedFallbackItemNames;
        }
        catch
        {
            return cachedFallbackItemNames = Array.Empty<string>();
        }
    }

    static void DrawStringOptionsField(SerializedProperty property, string label, string[] options)
    {
        int selectedIndex = Array.IndexOf(options, property.stringValue);
        int newIndex = EditorGUILayout.Popup(label, Mathf.Max(selectedIndex, 0), options);
        if (options.Length > 0)
        {
            property.stringValue = options[newIndex];
        }
    }
}
