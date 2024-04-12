using GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WeaverCore;
using WeaverCore.Features;

[CustomPropertyDrawer(typeof(GlobalEnums.MapZone))]
public class MapZonePropertyDrawer : PropertyDrawer
{
    int[] _mapZoneIndexes = null;
    string[] _mapZoneNames = null;

    void GetAllMapZones(out int[] mapZoneIndexes, out string[] mapZoneNames)
    {
        if (_mapZoneIndexes == null)
        {
            var mapZoneIndexList = new System.Collections.Generic.List<int>();
            var mapZoneNameList = new System.Collections.Generic.List<string>();

            foreach (var val in Enum.GetValues(typeof(MapZone)))
            {
                var enumVal = (MapZone)val;

                mapZoneIndexList.Add((int)enumVal);
                mapZoneNameList.Add(enumVal.ToString());
            }

            var registryGUIDs = AssetDatabase.FindAssets("t:Registry");

            foreach (var registry in registryGUIDs.Select(guid => AssetDatabase.LoadAssetAtPath<Registry>(AssetDatabase.GUIDToAssetPath(guid))))
            {
                foreach (var customZone in registry.GetFeatures<CustomMapZone>())
                {
                    var existingIndex = mapZoneIndexList.IndexOf(customZone.MapZoneID);

                    if (existingIndex >= 0)
                    {
                        mapZoneIndexList[existingIndex] = customZone.MapZoneID;
                        mapZoneNameList[existingIndex] = customZone.GetInternalName();
                    }
                    else
                    {
                        mapZoneIndexList.Add(customZone.MapZoneID);
                        mapZoneNameList.Add(customZone.GetInternalName());
                    }
                }
            }

            _mapZoneIndexes = mapZoneIndexList.ToArray();
            _mapZoneNames = mapZoneNameList.ToArray();
        }

        mapZoneIndexes = _mapZoneIndexes;
        mapZoneNames = _mapZoneNames;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        GetAllMapZones(out var indexes, out var names);

        property.intValue = EditorGUI.IntPopup(new Rect(position.x, position.y, position.width, position.height), property.intValue, names, indexes);

        EditorGUI.EndProperty();
    }
}
