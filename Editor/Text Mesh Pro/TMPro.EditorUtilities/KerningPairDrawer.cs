using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	[CustomPropertyDrawer(typeof(KerningPair))]
	public class KerningPairDrawer : PropertyDrawer
	{
		private bool isEditingEnabled = false;

		private string char_left;

		private string char_right;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty serializedProperty = property.FindPropertyRelative("AscII_Left");
			SerializedProperty serializedProperty2 = property.FindPropertyRelative("AscII_Right");
			SerializedProperty property2 = property.FindPropertyRelative("XadvanceOffset");
			position.yMin += 4f;
			position.yMax += 4f;
			position.height -= 2f;
			float num = position.width / 3f;
			float num2 = 5f;
			isEditingEnabled = ((label != GUIContent.none) ? true : false);
			GUILayout.BeginHorizontal();
			GUI.enabled = isEditingEnabled;
			Rect position2 = new Rect(position.x, position.y, 25f, position.height);
			char_left = EditorGUI.TextArea(position2, ((char)serializedProperty.intValue).ToString() ?? "");
			if (GUI.changed && char_left != "")
			{
				GUI.changed = false;
				serializedProperty.intValue = char_left[0];
			}
			Rect position3 = new Rect(position.x + position2.width + num2, position.y, num - position2.width - 10f, position.height);
			EditorGUI.PropertyField(position3, serializedProperty, GUIContent.none);
			position2 = new Rect(position.x + num * 1f, position.y, 25f, position.height);
			char_right = EditorGUI.TextArea(position2, ((char)serializedProperty2.intValue).ToString() ?? "");
			if (GUI.changed && char_right != "")
			{
				GUI.changed = false;
				serializedProperty2.intValue = char_right[0];
			}
			position3 = new Rect(position.x + num * 1f + position2.width + num2, position.y, num - position2.width - 10f, position.height);
			EditorGUI.PropertyField(position3, serializedProperty2, GUIContent.none);
			GUI.enabled = true;
			position3 = new Rect(position.x + num * 2f, position.y, num, position.height);
			EditorGUIUtility.labelWidth = 40f;
			EditorGUIUtility.fieldWidth = 45f;
			EditorGUI.PropertyField(position3, property2, new GUIContent("Offset"));
			GUILayout.EndHorizontal();
		}
	}
}
