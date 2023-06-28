using GlobalEnums;
using UnityEditor;
using WeaverCore.Components;

[CustomEditor(typeof(ForcePlayerDamager))]
public class ForcePlayerDamagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("damageDealt"));

        var hazardType = serializedObject.FindProperty("hazardType");

        hazardType.intValue = (int)(HazardType)EditorGUILayout.EnumPopup("Hazard Type", (HazardType)hazardType.intValue);

        //EditorGUILayout.PropertyField(serializedObject.FindProperty("shadowDashHazard"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("resetOnEnable"));
        serializedObject.ApplyModifiedProperties();
    }
}
