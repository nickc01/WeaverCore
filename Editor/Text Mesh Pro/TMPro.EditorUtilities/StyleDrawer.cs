using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	[CustomPropertyDrawer(typeof(TMP_Style))]
	public class StyleDrawer : PropertyDrawer
	{
		public static readonly float height = 95f;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty serializedProperty = property.FindPropertyRelative("m_Name");
			SerializedProperty serializedProperty2 = property.FindPropertyRelative("m_HashCode");
			SerializedProperty serializedProperty3 = property.FindPropertyRelative("m_OpeningDefinition");
			SerializedProperty serializedProperty4 = property.FindPropertyRelative("m_ClosingDefinition");
			SerializedProperty serializedProperty5 = property.FindPropertyRelative("m_OpeningTagArray");
			SerializedProperty serializedProperty6 = property.FindPropertyRelative("m_ClosingTagArray");
			EditorGUIUtility.labelWidth = 90f;
			position.height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			float num = position.height + 2f;
			EditorGUI.BeginChangeCheck();
			Rect position2 = new Rect(position.x, position.y, position.width / 2f + 5f, position.height);
			EditorGUI.PropertyField(position2, serializedProperty);
			if (EditorGUI.EndChangeCheck())
			{
				serializedProperty2.intValue = TMP_TextUtilities.GetSimpleHashCode(serializedProperty.stringValue);
				property.serializedObject.ApplyModifiedProperties();
				TMP_StyleSheet.RefreshStyles();
			}
			Rect position3 = new Rect(position2.x + position2.width + 5f, position.y, 65f, position.height);
			GUI.Label(position3, "HashCode");
			GUI.enabled = false;
			position3.x += 65f;
			position3.width = position.width / 2f - 75f;
			EditorGUI.PropertyField(position3, serializedProperty2, GUIContent.none);
			GUI.enabled = true;
			EditorGUI.BeginChangeCheck();
			position.y += num;
			GUI.Label(position, "Opening Tags");
			Rect position4 = new Rect(108f, position.y, position.width - 86f, 35f);
			serializedProperty3.stringValue = EditorGUI.TextArea(position4, serializedProperty3.stringValue);
			if (EditorGUI.EndChangeCheck())
			{
				int length = serializedProperty3.stringValue.Length;
				if (serializedProperty5.arraySize != length)
				{
					serializedProperty5.arraySize = length;
				}
				for (int i = 0; i < length; i++)
				{
					SerializedProperty arrayElementAtIndex = serializedProperty5.GetArrayElementAtIndex(i);
					arrayElementAtIndex.intValue = serializedProperty3.stringValue[i];
				}
			}
			EditorGUI.BeginChangeCheck();
			position.y += 38f;
			GUI.Label(position, "Closing Tags");
			Rect position5 = new Rect(108f, position.y, position.width - 86f, 35f);
			serializedProperty4.stringValue = EditorGUI.TextArea(position5, serializedProperty4.stringValue);
			if (EditorGUI.EndChangeCheck())
			{
				int length2 = serializedProperty4.stringValue.Length;
				if (serializedProperty6.arraySize != length2)
				{
					serializedProperty6.arraySize = length2;
				}
				for (int j = 0; j < length2; j++)
				{
					SerializedProperty arrayElementAtIndex2 = serializedProperty6.GetArrayElementAtIndex(j);
					arrayElementAtIndex2.intValue = serializedProperty4.stringValue[j];
				}
			}
		}
	}
}
