using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	[CustomPropertyDrawer(typeof(TMP_Glyph))]
	public class GlyphInfoDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty serializedProperty = property.FindPropertyRelative("id");
			SerializedProperty property2 = property.FindPropertyRelative("x");
			SerializedProperty property3 = property.FindPropertyRelative("y");
			SerializedProperty property4 = property.FindPropertyRelative("width");
			SerializedProperty property5 = property.FindPropertyRelative("height");
			SerializedProperty property6 = property.FindPropertyRelative("xOffset");
			SerializedProperty property7 = property.FindPropertyRelative("yOffset");
			SerializedProperty property8 = property.FindPropertyRelative("xAdvance");
			SerializedProperty property9 = property.FindPropertyRelative("scale");
			Rect rect = GUILayoutUtility.GetRect(position.width, 48f);
			rect.y -= 15f;
			EditorGUIUtility.labelWidth = 40f;
			EditorGUIUtility.fieldWidth = 45f;
			EditorGUI.LabelField(new Rect(rect.x + 5f, rect.y, 80f, 18f), new GUIContent("Ascii: <color=#FFFF80>" + serializedProperty.intValue + "</color>"), TMP_UIStyleManager.Label);
			EditorGUI.LabelField(new Rect(rect.x + 90f, rect.y, 80f, 18f), new GUIContent("Hex: <color=#FFFF80>" + serializedProperty.intValue.ToString("X") + "</color>"), TMP_UIStyleManager.Label);
			EditorGUI.LabelField(new Rect(rect.x + 170f, rect.y, 80f, 18f), "Char: [ <color=#FFFF80>" + (char)serializedProperty.intValue + "</color> ]", TMP_UIStyleManager.Label);
			EditorGUIUtility.labelWidth = 35f;
			EditorGUIUtility.fieldWidth = 10f;
			float num = (rect.width - 5f) / 4f;
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 0f, rect.y + 22f, num - 5f, 18f), property2, new GUIContent("X:"));
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 1f, rect.y + 22f, num - 5f, 18f), property3, new GUIContent("Y:"));
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 2f, rect.y + 22f, num - 5f, 18f), property4, new GUIContent("W:"));
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 3f, rect.y + 22f, num - 5f, 18f), property5, new GUIContent("H:"));
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 0f, rect.y + 44f, num - 5f, 18f), property6, new GUIContent("OX:"));
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 1f, rect.y + 44f, num - 5f, 18f), property7, new GUIContent("OY:"));
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 2f, rect.y + 44f, num - 5f, 18f), property8, new GUIContent("ADV:"));
			EditorGUI.PropertyField(new Rect(rect.x + 5f + num * 3f, rect.y + 44f, num - 5f, 18f), property9, new GUIContent("SF:"));
		}
	}
}
