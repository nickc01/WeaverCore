using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	[CustomPropertyDrawer(typeof(TMP_FontWeights))]
	public class FontWeightDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty property2 = property.FindPropertyRelative("regularTypeface");
			SerializedProperty property3 = property.FindPropertyRelative("italicTypeface");
			float width = position.width;
			position.width = 125f;
			EditorGUI.LabelField(position, label);
			if (label.text[0] == '4')
			{
				GUI.enabled = false;
			}
			position.x = 140f;
			position.width = (width - 140f) / 2f;
			EditorGUI.PropertyField(position, property2, GUIContent.none);
			GUI.enabled = true;
			position.x += position.width + 17f;
			EditorGUI.PropertyField(position, property3, GUIContent.none);
		}
	}
}
