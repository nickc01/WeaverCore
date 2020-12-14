using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	[CustomPropertyDrawer(typeof(TMP_Dropdown.OptionDataList), true)]
	internal class DropdownOptionListDrawer : PropertyDrawer
	{
		private ReorderableList m_ReorderableList;

		private void Init(SerializedProperty property)
		{
			if (m_ReorderableList == null)
			{
				SerializedProperty elements = property.FindPropertyRelative("m_Options");
				m_ReorderableList = new ReorderableList(property.serializedObject, elements);
				m_ReorderableList.drawElementCallback = DrawOptionData;
				m_ReorderableList.drawHeaderCallback = DrawHeader;
				m_ReorderableList.elementHeight += 16f;
			}
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Init(property);
			m_ReorderableList.DoList(position);
		}

		private void DrawHeader(Rect rect)
		{
			GUI.Label(rect, "Options");
		}

		private void DrawOptionData(Rect rect, int index, bool isActive, bool isFocused)
		{
			SerializedProperty arrayElementAtIndex = m_ReorderableList.serializedProperty.GetArrayElementAtIndex(index);
			SerializedProperty property = arrayElementAtIndex.FindPropertyRelative("m_Text");
			SerializedProperty property2 = arrayElementAtIndex.FindPropertyRelative("m_Image");
			RectOffset rectOffset = new RectOffset(0, 0, -1, -3);
			rect = rectOffset.Add(rect);
			rect.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(rect, property, GUIContent.none);
			rect.y += EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(rect, property2, GUIContent.none);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			Init(property);
			return m_ReorderableList.GetHeight();
		}
	}
}
