using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using WeaverCore.Features;

[CustomPropertyDrawer(typeof(LanguageTable.Entry))]
public class LanguageTableEntriesPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var keyProp = property.FindPropertyRelative(nameof(LanguageTable.Entry.key));
        var valueProp = property.FindPropertyRelative(nameof(LanguageTable.Entry.value));

        var left = new Rect(position.x,position.y,position.width / 2,position.height);
        var right = new Rect(position.x + (position.width / 2),position.y,position.width / 2,position.height);

        var keyDimensions = GUI.skin.label.CalcSize(new GUIContent("Key "));

        var keyLabelRect = new Rect(left.x,left.y,keyDimensions.x,left.height);
        var keyPropRect = new Rect(keyLabelRect.x + keyLabelRect.width, keyLabelRect.y,left.width - keyLabelRect.width,keyLabelRect.height);

        EditorGUI.LabelField(keyLabelRect, new GUIContent("Key "));

        keyProp.stringValue = EditorGUI.TextField(keyPropRect, keyProp.stringValue);


        var valueDimensions = GUI.skin.label.CalcSize(new GUIContent("  Value "));

        var valueLabelRect = new Rect(right.x, right.y, valueDimensions.x, right.height);
        var valuePropRect = new Rect(valueLabelRect.x + valueLabelRect.width, valueLabelRect.y, right.width - valueLabelRect.width, valueLabelRect.height);

        EditorGUI.LabelField(valueLabelRect, new GUIContent("  Value "));

        valueProp.stringValue = EditorGUI.TextField(valuePropRect, valueProp.stringValue);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(SerializedPropertyType.String, GUIContent.none);
    }
}
