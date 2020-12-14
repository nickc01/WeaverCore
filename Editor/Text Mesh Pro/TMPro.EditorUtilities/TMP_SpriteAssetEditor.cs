using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	[CustomEditor(typeof(TMP_SpriteAsset))]
	public class TMP_SpriteAssetEditor : Editor
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct UI_PanelState
		{
			public static bool spriteAssetInfoPanel = true;

			public static bool fallbackSpriteAssetPanel = true;

			public static bool spriteInfoPanel = false;
		}

		private int m_moveToIndex = 0;

		private int m_selectedElement = -1;

		private int m_page = 0;

		private const string k_UndoRedo = "UndoRedoPerformed";

		private string m_searchPattern;

		private List<int> m_searchList;

		private bool m_isSearchDirty;

		private SerializedProperty m_spriteAtlas_prop;

		private SerializedProperty m_material_prop;

		private SerializedProperty m_spriteInfoList_prop;

		private ReorderableList m_fallbackSpriteAssetList;

		private bool isAssetDirty = false;

		private string[] uiStateLabel = new string[2]
		{
			"<i>(Click to expand)</i>",
			"<i>(Click to collapse)</i>"
		};

		private float m_xOffset;

		private float m_yOffset;

		private float m_xAdvance;

		private float m_scale;

		public void OnEnable()
		{
			m_spriteAtlas_prop = base.serializedObject.FindProperty("spriteSheet");
			m_material_prop = base.serializedObject.FindProperty("material");
			m_spriteInfoList_prop = base.serializedObject.FindProperty("spriteInfoList");
			m_fallbackSpriteAssetList = new ReorderableList(base.serializedObject, base.serializedObject.FindProperty("fallbackSpriteAssets"), true, true, true, true);
			m_fallbackSpriteAssetList.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused)
			{
				SerializedProperty arrayElementAtIndex = m_fallbackSpriteAssetList.serializedProperty.GetArrayElementAtIndex(index);
				rect.y += 2f;
				EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), arrayElementAtIndex, GUIContent.none);
			};
			m_fallbackSpriteAssetList.drawHeaderCallback = delegate(Rect rect)
			{
				EditorGUI.LabelField(rect, "<b>Fallback Sprite Asset List</b>", TMP_UIStyleManager.Label);
			};
			TMP_UIStyleManager.GetUIStyles();
		}

		public override void OnInspectorGUI()
		{
			Event current = Event.current;
			string commandName = current.commandName;
			base.serializedObject.Update();
			EditorGUIUtility.labelWidth = 135f;
			GUILayout.Label("<b>TextMeshPro - Sprite Asset</b>", TMP_UIStyleManager.Section_Label);
			GUILayout.Label("Sprite Info", TMP_UIStyleManager.Section_Label);
			EditorGUI.indentLevel = 1;
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(m_spriteAtlas_prop, new GUIContent("Sprite Atlas"));
			if (EditorGUI.EndChangeCheck())
			{
				Texture2D texture2D = m_spriteAtlas_prop.objectReferenceValue as Texture2D;
				if (texture2D != null)
				{
					Material material = m_material_prop.objectReferenceValue as Material;
					if (material != null)
					{
						material.mainTexture = texture2D;
					}
				}
			}
			GUI.enabled = true;
			EditorGUILayout.PropertyField(m_material_prop, new GUIContent("Default Material"));
			EditorGUI.indentLevel = 0;
			if (GUILayout.Button("Fallback Sprite Assets\t" + (UI_PanelState.fallbackSpriteAssetPanel ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label))
			{
				UI_PanelState.fallbackSpriteAssetPanel = !UI_PanelState.fallbackSpriteAssetPanel;
			}
			if (UI_PanelState.fallbackSpriteAssetPanel)
			{
				EditorGUIUtility.labelWidth = 120f;
				EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
				EditorGUI.indentLevel = 0;
				GUILayout.Label("Select the Sprite Assets that will be searched and used as fallback when a given sprite is missing from this sprite asset.", TMP_UIStyleManager.Label);
				GUILayout.Space(10f);
				m_fallbackSpriteAssetList.DoLayoutList();
				EditorGUILayout.EndVertical();
			}
			GUI.enabled = true;
			GUILayout.Space(10f);
			EditorGUI.indentLevel = 0;
			if (GUILayout.Button("Sprite List\t\t" + (UI_PanelState.spriteInfoPanel ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label))
			{
				UI_PanelState.spriteInfoPanel = !UI_PanelState.spriteInfoPanel;
			}
			if (UI_PanelState.spriteInfoPanel)
			{
				int num = m_spriteInfoList_prop.arraySize;
				int num2 = 10;
				EditorGUILayout.BeginVertical(TMP_UIStyleManager.Group_Label, GUILayout.ExpandWidth(true));
				EditorGUILayout.BeginHorizontal();
				EditorGUIUtility.labelWidth = 110f;
				EditorGUI.BeginChangeCheck();
				string text = EditorGUILayout.TextField("Sprite Search", m_searchPattern, "SearchTextField");
				if (EditorGUI.EndChangeCheck() || m_isSearchDirty)
				{
					if (!string.IsNullOrEmpty(text))
					{
						m_searchPattern = text.ToLower(CultureInfo.InvariantCulture).Trim();
						SearchGlyphTable(m_searchPattern, ref m_searchList);
					}
					m_isSearchDirty = false;
				}
				string str = string.IsNullOrEmpty(m_searchPattern) ? "SearchCancelButtonEmpty" : "SearchCancelButton";
				if (GUILayout.Button(GUIContent.none, str))
				{
					GUIUtility.keyboardControl = 0;
					m_searchPattern = string.Empty;
				}
				EditorGUILayout.EndHorizontal();
				if (!string.IsNullOrEmpty(m_searchPattern))
				{
					num = m_searchList.Count;
				}
				DisplayGlyphPageNavigation(num, num2);
				EditorGUILayout.EndVertical();
				if (num > 0)
				{
					for (int i = num2 * m_page; i < num && i < num2 * (m_page + 1); i++)
					{
						Rect rect = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
						int index = i;
						if (!string.IsNullOrEmpty(m_searchPattern))
						{
							index = m_searchList[i];
						}
						SerializedProperty arrayElementAtIndex = m_spriteInfoList_prop.GetArrayElementAtIndex(index);
						EditorGUI.BeginDisabledGroup(i != m_selectedElement);
						EditorGUILayout.BeginVertical(TMP_UIStyleManager.Group_Label, GUILayout.Height(75f));
						EditorGUILayout.PropertyField(arrayElementAtIndex);
						EditorGUILayout.EndVertical();
						EditorGUI.EndDisabledGroup();
						Rect rect2 = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
						Rect rect3 = new Rect(rect.x, rect.y, rect2.width, rect2.y - rect.y);
						if (DoSelectionCheck(rect3))
						{
							m_selectedElement = i;
							GUIUtility.keyboardControl = 0;
						}
						if (m_selectedElement == i)
						{
							TMP_EditorUtility.DrawBox(rect3, 2f, new Color32(40, 192, byte.MaxValue, byte.MaxValue));
							Rect controlRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * 1f);
							controlRect.width /= 8f;
							bool enabled = GUI.enabled;
							if (i == 0)
							{
								GUI.enabled = false;
							}
							if (GUI.Button(controlRect, "Up"))
							{
								SwapSpriteElement(i, i - 1);
							}
							GUI.enabled = enabled;
							controlRect.x += controlRect.width;
							if (i == num - 1)
							{
								GUI.enabled = false;
							}
							if (GUI.Button(controlRect, "Down"))
							{
								SwapSpriteElement(i, i + 1);
							}
							GUI.enabled = enabled;
							controlRect.x += controlRect.width * 2f;
							m_moveToIndex = EditorGUI.IntField(controlRect, m_moveToIndex);
							controlRect.x -= controlRect.width;
							if (GUI.Button(controlRect, "Goto"))
							{
								MoveSpriteElement(i, m_moveToIndex);
							}
							GUI.enabled = enabled;
							controlRect.x += controlRect.width * 4f;
							if (GUI.Button(controlRect, "+"))
							{
								m_spriteInfoList_prop.arraySize++;
								int num3 = m_spriteInfoList_prop.arraySize - 1;
								SerializedProperty target = m_spriteInfoList_prop.GetArrayElementAtIndex(num3);
								CopySerializedProperty(m_spriteInfoList_prop.GetArrayElementAtIndex(index), ref target);
								target.FindPropertyRelative("id").intValue = num3;
								base.serializedObject.ApplyModifiedProperties();
								m_isSearchDirty = true;
							}
							controlRect.x += controlRect.width;
							if (m_selectedElement == -1)
							{
								GUI.enabled = false;
							}
							if (GUI.Button(controlRect, "-"))
							{
								m_spriteInfoList_prop.DeleteArrayElementAtIndex(index);
								m_selectedElement = -1;
								base.serializedObject.ApplyModifiedProperties();
								m_isSearchDirty = true;
								return;
							}
						}
					}
				}
				DisplayGlyphPageNavigation(num, num2);
				EditorGUIUtility.labelWidth = 40f;
				EditorGUIUtility.fieldWidth = 20f;
				GUILayout.Space(5f);
				GUI.enabled = true;
				EditorGUILayout.BeginVertical(TMP_UIStyleManager.Group_Label);
				Rect controlRect2 = EditorGUILayout.GetControlRect(false, 40f);
				float num4 = (controlRect2.width - 75f) / 4f;
				EditorGUI.LabelField(controlRect2, "Global Offsets & Scale", EditorStyles.boldLabel);
				controlRect2.x += 70f;
				bool changed = GUI.changed;
				GUI.changed = false;
				m_xOffset = EditorGUI.FloatField(new Rect(controlRect2.x + 5f + num4 * 0f, controlRect2.y + 20f, num4 - 5f, 18f), new GUIContent("OX:"), m_xOffset);
				if (GUI.changed)
				{
					UpdateGlobalProperty("xOffset", m_xOffset);
				}
				m_yOffset = EditorGUI.FloatField(new Rect(controlRect2.x + 5f + num4 * 1f, controlRect2.y + 20f, num4 - 5f, 18f), new GUIContent("OY:"), m_yOffset);
				if (GUI.changed)
				{
					UpdateGlobalProperty("yOffset", m_yOffset);
				}
				m_xAdvance = EditorGUI.FloatField(new Rect(controlRect2.x + 5f + num4 * 2f, controlRect2.y + 20f, num4 - 5f, 18f), new GUIContent("ADV."), m_xAdvance);
				if (GUI.changed)
				{
					UpdateGlobalProperty("xAdvance", m_xAdvance);
				}
				m_scale = EditorGUI.FloatField(new Rect(controlRect2.x + 5f + num4 * 3f, controlRect2.y + 20f, num4 - 5f, 18f), new GUIContent("SF."), m_scale);
				if (GUI.changed)
				{
					UpdateGlobalProperty("scale", m_scale);
				}
				EditorGUILayout.EndVertical();
				GUI.changed = changed;
			}
			if (base.serializedObject.ApplyModifiedProperties() || commandName == "UndoRedoPerformed" || isAssetDirty)
			{
				isAssetDirty = false;
				EditorUtility.SetDirty(base.target);
			}
			GUI.enabled = true;
			if (current.type == EventType.MouseDown && current.button == 0)
			{
				m_selectedElement = -1;
			}
		}

		private void DisplayGlyphPageNavigation(int arraySize, int itemsPerPage)
		{
			Rect controlRect = EditorGUILayout.GetControlRect(false, 20f);
			controlRect.width /= 3f;
			int num = (!Event.current.shift) ? 1 : 10;
			GUI.enabled = (m_page > 0);
			if (GUI.Button(controlRect, "Previous Page"))
			{
				m_page -= num;
			}
			GUIStyle style = new GUIStyle(GUI.skin.button)
			{
				normal = 
				{
					background = null
				}
			};
			GUI.enabled = true;
			controlRect.x += controlRect.width;
			int num2 = (int)((float)arraySize / (float)itemsPerPage + 0.999f);
			GUI.Button(controlRect, "Page " + (m_page + 1) + " / " + num2, style);
			controlRect.x += controlRect.width;
			GUI.enabled = (itemsPerPage * (m_page + 1) < arraySize);
			if (GUI.Button(controlRect, "Next Page"))
			{
				m_page += num;
			}
			m_page = Mathf.Clamp(m_page, 0, arraySize / itemsPerPage);
			GUI.enabled = true;
		}

		private void UpdateGlobalProperty(string property, float value)
		{
			int arraySize = m_spriteInfoList_prop.arraySize;
			for (int i = 0; i < arraySize; i++)
			{
				SerializedProperty arrayElementAtIndex = m_spriteInfoList_prop.GetArrayElementAtIndex(i);
				arrayElementAtIndex.FindPropertyRelative(property).floatValue = value;
			}
			GUI.changed = false;
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

		private void SwapSpriteElement(int selectedIndex, int newIndex)
		{
			m_spriteInfoList_prop.MoveArrayElement(selectedIndex, newIndex);
			m_spriteInfoList_prop.GetArrayElementAtIndex(selectedIndex).FindPropertyRelative("id").intValue = selectedIndex;
			m_spriteInfoList_prop.GetArrayElementAtIndex(newIndex).FindPropertyRelative("id").intValue = newIndex;
			m_selectedElement = newIndex;
			m_isSearchDirty = true;
		}

		private void MoveSpriteElement(int selectedIndex, int newIndex)
		{
			m_spriteInfoList_prop.MoveArrayElement(selectedIndex, newIndex);
			for (int i = 0; i < m_spriteInfoList_prop.arraySize; i++)
			{
				m_spriteInfoList_prop.GetArrayElementAtIndex(i).FindPropertyRelative("id").intValue = i;
			}
			m_selectedElement = newIndex;
			m_isSearchDirty = true;
		}

		private void CopySerializedProperty(SerializedProperty source, ref SerializedProperty target)
		{
			target.FindPropertyRelative("name").stringValue = source.FindPropertyRelative("name").stringValue;
			target.FindPropertyRelative("hashCode").intValue = source.FindPropertyRelative("hashCode").intValue;
			target.FindPropertyRelative("x").floatValue = source.FindPropertyRelative("x").floatValue;
			target.FindPropertyRelative("y").floatValue = source.FindPropertyRelative("y").floatValue;
			target.FindPropertyRelative("width").floatValue = source.FindPropertyRelative("width").floatValue;
			target.FindPropertyRelative("height").floatValue = source.FindPropertyRelative("height").floatValue;
			target.FindPropertyRelative("xOffset").floatValue = source.FindPropertyRelative("xOffset").floatValue;
			target.FindPropertyRelative("yOffset").floatValue = source.FindPropertyRelative("yOffset").floatValue;
			target.FindPropertyRelative("xAdvance").floatValue = source.FindPropertyRelative("xAdvance").floatValue;
			target.FindPropertyRelative("scale").floatValue = source.FindPropertyRelative("scale").floatValue;
			target.FindPropertyRelative("sprite").objectReferenceValue = source.FindPropertyRelative("sprite").objectReferenceValue;
		}

		private void SearchGlyphTable(string searchPattern, ref List<int> searchResults)
		{
			if (searchResults == null)
			{
				searchResults = new List<int>();
			}
			searchResults.Clear();
			int arraySize = m_spriteInfoList_prop.arraySize;
			for (int i = 0; i < arraySize; i++)
			{
				SerializedProperty arrayElementAtIndex = m_spriteInfoList_prop.GetArrayElementAtIndex(i);
				if (arrayElementAtIndex.FindPropertyRelative("id").intValue.ToString().Contains(searchPattern))
				{
					searchResults.Add(i);
				}
				string text = arrayElementAtIndex.FindPropertyRelative("name").stringValue.ToLower(CultureInfo.InvariantCulture).Trim();
				if (text.Contains(searchPattern))
				{
					searchResults.Add(i);
				}
			}
		}
	}
}
