using UnityEditor;
using WeaverCore.Features;

namespace WeaverCore.Editor
{
    [CustomEditor(typeof(InventoryItemBasic))]
    [CanEditMultipleObjects]
    public class InventoryItemBasicEditor : UnityEditor.Editor
    {
        SerializedProperty itemID;
        SerializedProperty itemType;
        SerializedProperty sprite;
        SerializedProperty alternateSprite;
        SerializedProperty bothSprite;
        SerializedProperty itemName;
        SerializedProperty description;
        SerializedProperty alternateName;
        SerializedProperty alternateDescription;
        SerializedProperty bothName;
        SerializedProperty bothDescription;
        SerializedProperty flowerOtherSourceDescription;
        SerializedProperty flowerBrokenOtherSourceDescription;
        SerializedProperty acquiredByDefault;
        SerializedProperty secondAcquiredByDefault;
        SerializedProperty countByDefault;
        SerializedProperty flowerFromOtherSourceByDefault;
        SerializedProperty flowerBrokenByDefault;
        SerializedProperty saveSettings;
        SerializedProperty acquiredSaveFieldName;
        SerializedProperty secondAcquiredSaveFieldName;
        SerializedProperty countSaveFieldName;
        SerializedProperty flowerFromOtherSourceSaveFieldName;
        SerializedProperty flowerBrokenSaveFieldName;

        void OnEnable()
        {
            itemID = serializedObject.FindProperty("itemID");
            itemType = serializedObject.FindProperty("itemType");
            sprite = serializedObject.FindProperty("sprite");
            alternateSprite = serializedObject.FindProperty("alternateSprite");
            bothSprite = serializedObject.FindProperty("bothSprite");
            itemName = serializedObject.FindProperty("itemName");
            description = serializedObject.FindProperty("description");
            alternateName = serializedObject.FindProperty("alternateName");
            alternateDescription = serializedObject.FindProperty("alternateDescription");
            bothName = serializedObject.FindProperty("bothName");
            bothDescription = serializedObject.FindProperty("bothDescription");
            flowerOtherSourceDescription = serializedObject.FindProperty("flowerOtherSourceDescription");
            flowerBrokenOtherSourceDescription = serializedObject.FindProperty("flowerBrokenOtherSourceDescription");
            acquiredByDefault = serializedObject.FindProperty("acquiredByDefault");
            secondAcquiredByDefault = serializedObject.FindProperty("secondAcquiredByDefault");
            countByDefault = serializedObject.FindProperty("countByDefault");
            flowerFromOtherSourceByDefault = serializedObject.FindProperty("flowerFromOtherSourceByDefault");
            flowerBrokenByDefault = serializedObject.FindProperty("flowerBrokenByDefault");
            saveSettings = serializedObject.FindProperty("saveSettings");
            acquiredSaveFieldName = serializedObject.FindProperty("acquiredSaveFieldName");
            secondAcquiredSaveFieldName = serializedObject.FindProperty("secondAcquiredSaveFieldName");
            countSaveFieldName = serializedObject.FindProperty("countSaveFieldName");
            flowerFromOtherSourceSaveFieldName = serializedObject.FindProperty("flowerFromOtherSourceSaveFieldName");
            flowerBrokenSaveFieldName = serializedObject.FindProperty("flowerBrokenSaveFieldName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            }

            var type = GetSelectedType();

            DrawIdentitySection();
            DrawDisplaySection(type);
            DrawDefaultStateSection(type);
            DrawPersistentSettingsSection(type);

            serializedObject.ApplyModifiedProperties();
        }

        InventoryItem.ItemType GetSelectedType()
        {
            return (InventoryItem.ItemType)itemType.enumValueIndex;
        }

        void DrawIdentitySection()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(itemID);
            EditorGUILayout.PropertyField(itemType);
        }

        void DrawDisplaySection(InventoryItem.ItemType type)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Display", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sprite);
            EditorGUILayout.PropertyField(itemName);
            EditorGUILayout.PropertyField(description);

            if (UsesAlternateDisplay(type))
            {
                EditorGUILayout.PropertyField(alternateSprite);
                EditorGUILayout.PropertyField(alternateName);
                EditorGUILayout.PropertyField(alternateDescription);
            }

            if (type == InventoryItem.ItemType.OneTwoBoth)
            {
                EditorGUILayout.PropertyField(bothSprite);
                EditorGUILayout.PropertyField(bothName);
                EditorGUILayout.PropertyField(bothDescription);
            }

            if (type == InventoryItem.ItemType.Flower)
            {
                EditorGUILayout.PropertyField(flowerOtherSourceDescription);
                EditorGUILayout.PropertyField(flowerBrokenOtherSourceDescription);
            }
        }

        void DrawDefaultStateSection(InventoryItem.ItemType type)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Default State", EditorStyles.boldLabel);

            switch (type)
            {
                case InventoryItem.ItemType.Counted:
                    EditorGUILayout.PropertyField(countByDefault);
                    break;
                case InventoryItem.ItemType.OneTwo:
                case InventoryItem.ItemType.OneTwoBoth:
                    EditorGUILayout.PropertyField(acquiredByDefault);
                    EditorGUILayout.PropertyField(secondAcquiredByDefault);
                    break;
                case InventoryItem.ItemType.Flower:
                    EditorGUILayout.PropertyField(acquiredByDefault);
                    EditorGUILayout.PropertyField(flowerFromOtherSourceByDefault);
                    EditorGUILayout.PropertyField(flowerBrokenByDefault);
                    break;
                default:
                    EditorGUILayout.PropertyField(acquiredByDefault);
                    break;
            }
        }

        void DrawPersistentSettingsSection(InventoryItem.ItemType type)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Persistent Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(saveSettings);

            switch (type)
            {
                case InventoryItem.ItemType.Counted:
                    EditorGUILayout.PropertyField(countSaveFieldName);
                    break;
                case InventoryItem.ItemType.OneTwo:
                case InventoryItem.ItemType.OneTwoBoth:
                    EditorGUILayout.PropertyField(acquiredSaveFieldName);
                    EditorGUILayout.PropertyField(secondAcquiredSaveFieldName);
                    break;
                case InventoryItem.ItemType.Flower:
                    EditorGUILayout.PropertyField(acquiredSaveFieldName);
                    EditorGUILayout.PropertyField(flowerFromOtherSourceSaveFieldName);
                    EditorGUILayout.PropertyField(flowerBrokenSaveFieldName);
                    break;
                default:
                    EditorGUILayout.PropertyField(acquiredSaveFieldName);
                    break;
            }
        }

        static bool UsesAlternateDisplay(InventoryItem.ItemType type)
        {
            return type == InventoryItem.ItemType.OneTwo ||
                type == InventoryItem.ItemType.OneTwoBoth ||
                type == InventoryItem.ItemType.Flower;
        }
    }
}
