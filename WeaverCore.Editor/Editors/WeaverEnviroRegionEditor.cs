using UnityEditor;
using WeaverCore;

[CustomEditor(typeof(WeaverEnviroRegion))]
[CanEditMultipleObjects]
public class WeaverEnviroRegionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        var iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            using (new EditorGUI.DisabledScope(iterator.propertyPath == "m_Script"))
            {
                if (iterator.name == "environmentType")
                {
                    continue;
                }

                if (iterator.name == "EnvironmentTypeName")
                {
                    EditorGUI.showMixedValue = iterator.hasMultipleDifferentValues;
                    EditorGUI.BeginChangeCheck();
                    var newValue = EnvironmentTypeEditorUtilities.DrawEnvironmentTypePopup("Environment Type", iterator.intValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        iterator.intValue = newValue;
                    }
                    EditorGUI.showMixedValue = false;
                    continue;
                }

                EditorGUILayout.PropertyField(iterator, true);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
