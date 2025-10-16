using UnityEngine;
using UnityEditor;
using WeaverCore.Features;
using System.Linq;

namespace WeaverCore.Editor
{
    /// <summary>
    /// Custom editor for CustomMapData to provide better UI and validation
    /// </summary>
    [CustomEditor(typeof(CustomMapData))]
    public class CustomMapDataEditor : UnityEditor.Editor
    {
        private SerializedProperty mapZoneProp;
        private SerializedProperty mapBackgroundProp;
        private SerializedProperty backgroundOffsetProp;
        private SerializedProperty backgroundScaleProp;
        private SerializedProperty roomsProp;
        private SerializedProperty autoDiscoverMapProp;
        private SerializedProperty corniferLocationProp;
        private SerializedProperty mapCostProp;
        private SerializedProperty mapBoundsProp;

        private bool showRoomsFoldout = true;
        private bool showAdvancedSettings = false;

        private void OnEnable()
        {
            mapZoneProp = serializedObject.FindProperty("mapZone");
            mapBackgroundProp = serializedObject.FindProperty("mapBackground");
            backgroundOffsetProp = serializedObject.FindProperty("backgroundOffset");
            backgroundScaleProp = serializedObject.FindProperty("backgroundScale");
            roomsProp = serializedObject.FindProperty("rooms");
            autoDiscoverMapProp = serializedObject.FindProperty("autoDiscoverMap");
            corniferLocationProp = serializedObject.FindProperty("corniferLocation");
            mapCostProp = serializedObject.FindProperty("mapCost");
            mapBoundsProp = serializedObject.FindProperty("mapBounds");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            CustomMapData mapData = (CustomMapData)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Custom Map Data", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Map Zone Reference
            EditorGUILayout.LabelField("Map Zone Reference", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(mapZoneProp, new GUIContent("Custom Map Zone", "The custom map zone this map data belongs to"));
            
            if (mapZoneProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("You must assign a CustomMapZone for this map to work properly.", MessageType.Error);
            }
            else
            {
                var customZone = mapZoneProp.objectReferenceValue as CustomMapZone;
                if (customZone != null)
                {
                    EditorGUILayout.LabelField($"Map Zone ID: {customZone.MapZoneID}");
                    EditorGUILayout.LabelField($"Internal Name: {customZone.GetInternalName()}");
                }
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            // Map Visual Settings
            EditorGUILayout.LabelField("Map Visual Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(mapBackgroundProp, new GUIContent("Map Background", "The background sprite used for the map when opened"));
            EditorGUILayout.PropertyField(backgroundOffsetProp, new GUIContent("Background Offset", "The position offset for the map background"));
            EditorGUILayout.PropertyField(backgroundScaleProp, new GUIContent("Background Scale", "The scale for the map background"));
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            // Map Bounds
            EditorGUILayout.LabelField("Map Bounds", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(mapBoundsProp, new GUIContent("Map Bounds", "The bounds of this custom map for positioning rooms"));
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            // Room Layout
            showRoomsFoldout = EditorGUILayout.Foldout(showRoomsFoldout, $"Rooms ({roomsProp.arraySize})", true);
            if (showRoomsFoldout)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Room"))
                {
                    roomsProp.arraySize++;
                    SerializedProperty newRoom = roomsProp.GetArrayElementAtIndex(roomsProp.arraySize - 1);
                    // Initialize new room with default values
                    newRoom.FindPropertyRelative("sceneName").stringValue = "";
                    newRoom.FindPropertyRelative("displayName").stringValue = "";
                    newRoom.FindPropertyRelative("mapPosition").vector2Value = Vector2.zero;
                    newRoom.FindPropertyRelative("spriteScale").vector2Value = Vector2.one;
                    newRoom.FindPropertyRelative("autoMap").boolValue = true;
                    newRoom.FindPropertyRelative("alwaysVisible").boolValue = false;
                    newRoom.FindPropertyRelative("roomType").enumValueIndex = 0;
                }
                
                if (GUILayout.Button("Clear All Rooms"))
                {
                    if (EditorUtility.DisplayDialog("Clear All Rooms", "Are you sure you want to remove all rooms from this map?", "Yes", "No"))
                    {
                        roomsProp.arraySize = 0;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                for (int i = 0; i < roomsProp.arraySize; i++)
                {
                    SerializedProperty room = roomsProp.GetArrayElementAtIndex(i);
                    DrawRoomProperty(room, i);
                }
                
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Map Discovery Settings
            showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Advanced Settings", true);
            if (showAdvancedSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(autoDiscoverMapProp, new GUIContent("Auto Discover Map", "Whether this map should be automatically discovered when the zone is entered"));
                
                if (!autoDiscoverMapProp.boolValue)
                {
                    EditorGUILayout.PropertyField(corniferLocationProp, new GUIContent("Cornifer Location", "The scene name where Cornifer appears to sell this map (optional)"));
                    EditorGUILayout.PropertyField(mapCostProp, new GUIContent("Map Cost", "The cost to purchase this map from Cornifer"));
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Validation and Info
            if (mapData.mapZone != null)
            {
                int roomCount = mapData.rooms.Count;
                int validRooms = mapData.rooms.Count(r => !string.IsNullOrEmpty(r.sceneName));
                
                EditorGUILayout.HelpBox($"Map Info:\n• Map Zone: {mapData.mapZone.MapZoneName}\n• Total Rooms: {roomCount}\n• Valid Rooms: {validRooms}", MessageType.Info);
                
                if (roomCount == 0)
                {
                    EditorGUILayout.HelpBox("This map has no rooms. Add some rooms to make it functional.", MessageType.Warning);
                }
                else if (validRooms < roomCount)
                {
                    EditorGUILayout.HelpBox($"{roomCount - validRooms} room(s) have empty scene names and will not work properly.", MessageType.Warning);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRoomProperty(SerializedProperty room, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            
            SerializedProperty sceneNameProp = room.FindPropertyRelative("sceneName");
            string sceneName = sceneNameProp.stringValue;
            string displayName = string.IsNullOrEmpty(sceneName) ? $"Room {index}" : sceneName;
            
            bool foldout = EditorGUILayout.Foldout(true, displayName, true);
            
            if (GUILayout.Button("×", GUILayout.Width(20)))
            {
                if (EditorUtility.DisplayDialog("Remove Room", $"Remove room '{displayName}'?", "Yes", "No"))
                {
                    roomsProp.DeleteArrayElementAtIndex(index);
                    serializedObject.ApplyModifiedProperties();
                    return;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (foldout)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(sceneNameProp, new GUIContent("Scene Name"));
                EditorGUILayout.PropertyField(room.FindPropertyRelative("displayName"), new GUIContent("Display Name (Optional)"));
                EditorGUILayout.PropertyField(room.FindPropertyRelative("roomSprite"), new GUIContent("Room Sprite"));
                EditorGUILayout.PropertyField(room.FindPropertyRelative("mapPosition"), new GUIContent("Map Position"));
                EditorGUILayout.PropertyField(room.FindPropertyRelative("spriteScale"), new GUIContent("Sprite Scale"));
                EditorGUILayout.PropertyField(room.FindPropertyRelative("autoMap"), new GUIContent("Auto Map"));
                EditorGUILayout.PropertyField(room.FindPropertyRelative("alwaysVisible"), new GUIContent("Always Visible"));
                EditorGUILayout.PropertyField(room.FindPropertyRelative("roomType"), new GUIContent("Room Type"));
                
                if (string.IsNullOrEmpty(sceneNameProp.stringValue))
                {
                    EditorGUILayout.HelpBox("Scene Name is required for this room to function.", MessageType.Error);
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
    }
}