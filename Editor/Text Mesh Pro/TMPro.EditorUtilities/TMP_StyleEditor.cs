using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(TMP_StyleSheet))]
	public class TMP_StyleEditor : Editor
	{
		private SerializedProperty m_styleList_prop;

		private int m_selectedElement = -1;

		private Rect m_selectionRect;

		private int m_page = 0;

		private void OnEnable()
		{
			TMP_UIStyleManager.GetUIStyles();
			m_styleList_prop = base.serializedObject.FindProperty("m_StyleList");
		}

		public override void OnInspectorGUI()
		{
			Event current = Event.current;
			base.serializedObject.Update();
			GUILayout.Label("<b>TextMeshPro - Style Sheet</b>", TMP_UIStyleManager.Section_Label);
			int arraySize = m_styleList_prop.arraySize;
			int num = (Screen.height - 178) / 111;
			if (arraySize > 0)
			{
				for (int i = num * m_page; i < arraySize && i < num * (m_page + 1); i++)
				{
					if (m_selectedElement == i)
					{
						EditorGUI.DrawRect(m_selectionRect, new Color32(40, 192, byte.MaxValue, byte.MaxValue));
					}
					Rect rect = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
					EditorGUILayout.BeginVertical(TMP_UIStyleManager.Group_Label);
					SerializedProperty arrayElementAtIndex = m_styleList_prop.GetArrayElementAtIndex(i);
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(arrayElementAtIndex);
					EditorGUILayout.EndVertical();
					if (EditorGUI.EndChangeCheck())
					{
					}
					Rect rect2 = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
					Rect selectionArea = new Rect(rect.x, rect.y, rect2.width, rect2.y - rect.y);
					if (DoSelectionCheck(selectionArea))
					{
						m_selectedElement = i;
						m_selectionRect = new Rect(selectionArea.x - 2f, selectionArea.y + 2f, selectionArea.width + 4f, selectionArea.height - 4f);
						Repaint();
					}
				}
			}
			int num2 = (!current.shift) ? 1 : 10;
			GUILayout.Space(-3f);
			Rect controlRect = EditorGUILayout.GetControlRect(false, 20f);
			controlRect.width /= 6f;
			if (num == 0)
			{
				return;
			}
			controlRect.x += controlRect.width * 4f;
			if (GUI.Button(controlRect, "+"))
			{
				m_styleList_prop.arraySize++;
				base.serializedObject.ApplyModifiedProperties();
				TMP_StyleSheet.RefreshStyles();
			}
			controlRect.x += controlRect.width;
			if (m_selectedElement == -1)
			{
				GUI.enabled = false;
			}
			if (GUI.Button(controlRect, "-"))
			{
				if (m_selectedElement != -1)
				{
					m_styleList_prop.DeleteArrayElementAtIndex(m_selectedElement);
				}
				m_selectedElement = -1;
				base.serializedObject.ApplyModifiedProperties();
				TMP_StyleSheet.RefreshStyles();
			}
			GUILayout.Space(5f);
			controlRect = EditorGUILayout.GetControlRect(false, 20f);
			controlRect.width /= 3f;
			if (m_page > 0)
			{
				GUI.enabled = true;
			}
			else
			{
				GUI.enabled = false;
			}
			if (GUI.Button(controlRect, "Previous"))
			{
				m_page -= num2;
			}
			GUI.enabled = true;
			controlRect.x += controlRect.width;
			int num3 = (int)((float)arraySize / (float)num + 0.999f);
			GUI.Label(controlRect, "Page " + (m_page + 1) + " / " + num3, GUI.skin.button);
			controlRect.x += controlRect.width;
			if (num * (m_page + 1) < arraySize)
			{
				GUI.enabled = true;
			}
			else
			{
				GUI.enabled = false;
			}
			if (GUI.Button(controlRect, "Next"))
			{
				m_page += num2;
			}
			m_page = Mathf.Clamp(m_page, 0, arraySize / num);
			if (base.serializedObject.ApplyModifiedProperties())
			{
				TMPro_EventManager.ON_TEXT_STYLE_PROPERTY_CHANGED(true);
			}
			GUI.enabled = true;
			if (current.type == EventType.MouseDown && current.button == 0)
			{
				m_selectedElement = -1;
			}
		}

		private bool DoSelectionCheck(Rect selectionArea)
		{
			Event current = Event.current;
			if (current.type == EventType.MouseDown && selectionArea.Contains(current.mousePosition) && current.button == 0)
			{
				current.Use();
				return true;
			}
			return false;
		}
	}
}
