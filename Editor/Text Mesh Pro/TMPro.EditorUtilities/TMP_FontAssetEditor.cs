using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	[CustomEditor(typeof(WeaverCore.Assets.TMPro.TMP_FontAsset))]
	public class TMP_FontAssetEditor_WEAVERCORE : TMP_FontAssetEditor
	{

	}


	[CustomEditor(typeof(TMP_FontAsset))]
	public class TMP_FontAssetEditor : Editor
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct UI_PanelState
		{
			public static bool fontInfoPanel = true;

			public static bool fontWeightPanel = true;

			public static bool fallbackFontAssetPanel = true;

			public static bool glyphInfoPanel = false;

			public static bool kerningInfoPanel = false;
		}

		private struct Warning
		{
			public bool isEnabled;

			public double expirationTime;
		}

		private int m_GlyphPage = 0;

		private int m_KerningPage = 0;

		private int m_selectedElement = -1;

		private string m_dstGlyphID;

		private const string k_placeholderUnicodeHex = "<i>Unicode Hex ID</i>";

		private string m_unicodeHexLabel = "<i>Unicode Hex ID</i>";

		private Warning m_AddGlyphWarning;

		private string m_searchPattern;

		private List<int> m_searchList;

		private bool m_isSearchDirty;

		private const string k_UndoRedo = "UndoRedoPerformed";

		private SerializedProperty font_atlas_prop;

		private SerializedProperty font_material_prop;

		private SerializedProperty fontWeights_prop;

		private ReorderableList m_list;

		private SerializedProperty font_normalStyle_prop;

		private SerializedProperty font_normalSpacing_prop;

		private SerializedProperty font_boldStyle_prop;

		private SerializedProperty font_boldSpacing_prop;

		private SerializedProperty font_italicStyle_prop;

		private SerializedProperty font_tabSize_prop;

		private SerializedProperty m_fontInfo_prop;

		private SerializedProperty m_glyphInfoList_prop;

		private SerializedProperty m_kerningInfo_prop;

		private KerningTable m_kerningTable;

		private SerializedProperty m_kerningPair_prop;

		private TMP_FontAsset m_fontAsset;

		private Material[] m_materialPresets;

		private bool isAssetDirty = false;

		private int errorCode;

		private DateTime timeStamp;

		private string[] uiStateLabel = new string[2]
		{
			"<i>(Click to expand)</i>",
			"<i>(Click to collapse)</i>"
		};

		public void OnEnable()
		{
			font_atlas_prop = base.serializedObject.FindProperty("atlas");
			font_material_prop = base.serializedObject.FindProperty("material");
			fontWeights_prop = base.serializedObject.FindProperty("fontWeights");
			m_list = new ReorderableList(base.serializedObject, base.serializedObject.FindProperty("fallbackFontAssets"), true, true, true, true);
			m_list.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused)
			{
				SerializedProperty arrayElementAtIndex = m_list.serializedProperty.GetArrayElementAtIndex(index);
				rect.y += 2f;
				EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), arrayElementAtIndex, GUIContent.none);
			};
			m_list.drawHeaderCallback = delegate(Rect rect)
			{
				EditorGUI.LabelField(rect, "<b>Fallback Font Asset List</b>", TMP_UIStyleManager.Label);
			};
			font_normalStyle_prop = base.serializedObject.FindProperty("normalStyle");
			font_normalSpacing_prop = base.serializedObject.FindProperty("normalSpacingOffset");
			font_boldStyle_prop = base.serializedObject.FindProperty("boldStyle");
			font_boldSpacing_prop = base.serializedObject.FindProperty("boldSpacing");
			font_italicStyle_prop = base.serializedObject.FindProperty("italicStyle");
			font_tabSize_prop = base.serializedObject.FindProperty("tabSize");
			m_fontInfo_prop = base.serializedObject.FindProperty("m_fontInfo");
			m_glyphInfoList_prop = base.serializedObject.FindProperty("m_glyphInfoList");
			m_kerningInfo_prop = base.serializedObject.FindProperty("m_kerningInfo");
			m_kerningPair_prop = base.serializedObject.FindProperty("m_kerningPair");
			m_fontAsset = (base.target as TMP_FontAsset);
			m_kerningTable = m_fontAsset.kerningInfo;
			m_materialPresets = TMP_EditorUtility.FindMaterialReferences(m_fontAsset);
			TMP_UIStyleManager.GetUIStyles();
			m_searchList = new List<int>();
		}

		public override void OnInspectorGUI()
		{
			Event current = Event.current;
			base.serializedObject.Update();
			GUILayout.Label("<b>TextMesh Pro! Font Asset</b>", TMP_UIStyleManager.Section_Label);
			GUILayout.Label("Face Info", TMP_UIStyleManager.Section_Label);
			EditorGUI.indentLevel = 1;
			GUI.enabled = false;
			float labelWidth = EditorGUIUtility.labelWidth = 150f;
			float fieldWidth = EditorGUIUtility.fieldWidth;
			EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("Name"), new GUIContent("Font Source"));
			EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("PointSize"));
			GUI.enabled = true;
			EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("Scale"));
			EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("LineHeight"));
			EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("Ascender"));
			EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("CapHeight"));
			EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("Baseline"));
			EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("Descender"));
			EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("Underline"), new GUIContent("Underline Offset"));
			EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("strikethrough"), new GUIContent("Strikethrough Offset"));
			EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("SuperscriptOffset"));
			EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("SubscriptOffset"));
			SerializedProperty serializedProperty = m_fontInfo_prop.FindPropertyRelative("SubSize");
			EditorGUILayout.PropertyField(serializedProperty, new GUIContent("Super / Subscript Size"));
			serializedProperty.floatValue = Mathf.Clamp(serializedProperty.floatValue, 0.25f, 1f);
			GUI.enabled = false;
			EditorGUI.indentLevel = 1;
			GUILayout.Space(18f);
			EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("Padding"));
			EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("AtlasWidth"), new GUIContent("Width"));
			EditorGUILayout.PropertyField(m_fontInfo_prop.FindPropertyRelative("AtlasHeight"), new GUIContent("Height"));
			GUI.enabled = true;
			EditorGUI.indentLevel = 0;
			GUILayout.Space(20f);
			GUILayout.Label("Font Sub-Assets", TMP_UIStyleManager.Section_Label);
			GUI.enabled = false;
			EditorGUI.indentLevel = 1;
			EditorGUILayout.PropertyField(font_atlas_prop, new GUIContent("Font Atlas:"));
			EditorGUILayout.PropertyField(font_material_prop, new GUIContent("Font Material:"));
			GUI.enabled = true;
			string commandName = Event.current.commandName;
			EditorGUI.indentLevel = 0;
			if (GUILayout.Button("Font Weights\t" + (UI_PanelState.fontWeightPanel ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label))
			{
				UI_PanelState.fontWeightPanel = !UI_PanelState.fontWeightPanel;
			}
			if (UI_PanelState.fontWeightPanel)
			{
				EditorGUIUtility.labelWidth = 120f;
				EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
				EditorGUI.indentLevel = 0;
				GUILayout.Label("Select the Font Assets that will be used for the following font weights.", TMP_UIStyleManager.Label);
				GUILayout.Space(10f);
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("<b>Font Weight</b>", TMP_UIStyleManager.Label, GUILayout.Width(117f));
				GUILayout.Label("<b>Normal Style</b>", TMP_UIStyleManager.Label);
				GUILayout.Label("<b>Italic Style</b>", TMP_UIStyleManager.Label);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(4), new GUIContent("400 - Regular"));
				EditorGUILayout.PropertyField(fontWeights_prop.GetArrayElementAtIndex(7), new GUIContent("700 - Bold"));
				EditorGUILayout.EndVertical();
				EditorGUIUtility.labelWidth = 120f;
				EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
				GUILayout.Label("Settings used to simulate a typeface when no font asset is available.", TMP_UIStyleManager.Label);
				GUILayout.Space(5f);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(font_normalStyle_prop, new GUIContent("Normal Weight"));
				font_normalStyle_prop.floatValue = Mathf.Clamp(font_normalStyle_prop.floatValue, -3f, 3f);
				if (GUI.changed || commandName == "UndoRedoPerformed")
				{
					GUI.changed = false;
					for (int i = 0; i < m_materialPresets.Length; i++)
					{
						m_materialPresets[i].SetFloat("_WeightNormal", font_normalStyle_prop.floatValue);
					}
				}
				EditorGUILayout.PropertyField(font_boldStyle_prop, new GUIContent("Bold Weight"), GUILayout.MinWidth(100f));
				font_boldStyle_prop.floatValue = Mathf.Clamp(font_boldStyle_prop.floatValue, -3f, 3f);
				if (GUI.changed || commandName == "UndoRedoPerformed")
				{
					GUI.changed = false;
					for (int j = 0; j < m_materialPresets.Length; j++)
					{
						m_materialPresets[j].SetFloat("_WeightBold", font_boldStyle_prop.floatValue);
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(font_normalSpacing_prop, new GUIContent("Spacing Offset"));
				font_normalSpacing_prop.floatValue = Mathf.Clamp(font_normalSpacing_prop.floatValue, -100f, 100f);
				if (GUI.changed || commandName == "UndoRedoPerformed")
				{
					GUI.changed = false;
				}
				EditorGUILayout.PropertyField(font_boldSpacing_prop, new GUIContent("Bold Spacing"));
				font_boldSpacing_prop.floatValue = Mathf.Clamp(font_boldSpacing_prop.floatValue, 0f, 100f);
				if (GUI.changed || commandName == "UndoRedoPerformed")
				{
					GUI.changed = false;
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(font_italicStyle_prop, new GUIContent("Italic Style: "));
				font_italicStyle_prop.intValue = Mathf.Clamp(font_italicStyle_prop.intValue, 15, 60);
				EditorGUILayout.PropertyField(font_tabSize_prop, new GUIContent("Tab Multiple: "));
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			}
			GUILayout.Space(5f);
			EditorGUI.indentLevel = 0;
			if (GUILayout.Button("Fallback Font Assets\t" + (UI_PanelState.fallbackFontAssetPanel ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label))
			{
				UI_PanelState.fallbackFontAssetPanel = !UI_PanelState.fallbackFontAssetPanel;
			}
			if (UI_PanelState.fallbackFontAssetPanel)
			{
				EditorGUIUtility.labelWidth = 120f;
				EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
				EditorGUI.indentLevel = 0;
				GUILayout.Label("Select the Font Assets that will be searched and used as fallback when characters are missing from this font asset.", TMP_UIStyleManager.Label);
				GUILayout.Space(10f);
				m_list.DoLayoutList();
				EditorGUILayout.EndVertical();
			}
			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUIUtility.fieldWidth = fieldWidth;
			GUILayout.Space(5f);
			EditorGUI.indentLevel = 0;
			if (GUILayout.Button("Glyph Info\t" + (UI_PanelState.glyphInfoPanel ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label))
			{
				UI_PanelState.glyphInfoPanel = !UI_PanelState.glyphInfoPanel;
			}
			if (UI_PanelState.glyphInfoPanel)
			{
				int num2 = m_glyphInfoList_prop.arraySize;
				int num3 = 15;
				EditorGUILayout.BeginVertical(TMP_UIStyleManager.Group_Label, GUILayout.ExpandWidth(true));
				EditorGUILayout.BeginHorizontal();
				EditorGUIUtility.labelWidth = 110f;
				EditorGUI.BeginChangeCheck();
				string text = EditorGUILayout.TextField("Glyph Search", m_searchPattern, "SearchTextField");
				if (EditorGUI.EndChangeCheck() || m_isSearchDirty)
				{
					if (!string.IsNullOrEmpty(text))
					{
						m_searchPattern = text;
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
					num2 = m_searchList.Count;
				}
				DisplayGlyphPageNavigation(num2, num3);
				EditorGUILayout.EndVertical();
				if (num2 > 0)
				{
					for (int k = num3 * m_GlyphPage; k < num2 && k < num3 * (m_GlyphPage + 1); k++)
					{
						Rect rect = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
						int num4 = k;
						if (!string.IsNullOrEmpty(m_searchPattern))
						{
							num4 = m_searchList[k];
						}
						SerializedProperty arrayElementAtIndex = m_glyphInfoList_prop.GetArrayElementAtIndex(num4);
						EditorGUI.BeginDisabledGroup(k != m_selectedElement);
						EditorGUILayout.BeginVertical(TMP_UIStyleManager.Group_Label);
						EditorGUILayout.PropertyField(arrayElementAtIndex);
						EditorGUILayout.EndVertical();
						EditorGUI.EndDisabledGroup();
						Rect rect2 = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
						Rect rect3 = new Rect(rect.x, rect.y, rect2.width, rect2.y - rect.y);
						if (DoSelectionCheck(rect3))
						{
							m_selectedElement = k;
							m_AddGlyphWarning.isEnabled = false;
							m_unicodeHexLabel = "<i>Unicode Hex ID</i>";
							GUIUtility.keyboardControl = 0;
						}
						if (m_selectedElement != k)
						{
							continue;
						}
						TMP_EditorUtility.DrawBox(rect3, 2f, new Color32(40, 192, byte.MaxValue, byte.MaxValue));
						Rect controlRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * 1f);
						float num5 = controlRect.width * 0.6f;
						float num6 = num5 / 3f;
						Rect position = new Rect(controlRect.x + controlRect.width * 0.4f, controlRect.y, num6, controlRect.height);
						GUI.enabled = !string.IsNullOrEmpty(m_dstGlyphID);
						if (GUI.Button(position, new GUIContent("Copy to")))
						{
							GUIUtility.keyboardControl = 0;
							int dstGlyphID = TMP_TextUtilities.StringToInt(m_dstGlyphID);
							if (!AddNewGlyph(num4, dstGlyphID))
							{
								m_AddGlyphWarning.isEnabled = true;
								m_AddGlyphWarning.expirationTime = EditorApplication.timeSinceStartup + 1.0;
							}
							m_dstGlyphID = string.Empty;
							m_isSearchDirty = true;
							TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, m_fontAsset);
						}
						GUI.enabled = true;
						position.x += num6;
						GUI.SetNextControlName("GlyphID_Input");
						m_dstGlyphID = EditorGUI.TextField(position, m_dstGlyphID);
						EditorGUI.LabelField(position, new GUIContent(m_unicodeHexLabel, "The Unicode (Hex) ID of the duplicated Glyph"), TMP_UIStyleManager.Label);
						if (GUI.GetNameOfFocusedControl() == "GlyphID_Input")
						{
							m_unicodeHexLabel = string.Empty;
							char character = Event.current.character;
							if ((character < '0' || character > '9') && (character < 'a' || character > 'f') && (character < 'A' || character > 'F'))
							{
								Event.current.character = '\0';
							}
						}
						else
						{
							m_unicodeHexLabel = "<i>Unicode Hex ID</i>";
						}
						position.x += num6;
						if (GUI.Button(position, "Remove"))
						{
							GUIUtility.keyboardControl = 0;
							RemoveGlyphFromList(num4);
							m_selectedElement = -1;
							m_isSearchDirty = true;
							TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, m_fontAsset);
							return;
						}
						if (m_AddGlyphWarning.isEnabled && EditorApplication.timeSinceStartup < m_AddGlyphWarning.expirationTime)
						{
							EditorGUILayout.HelpBox("The Destination Glyph ID already exists", MessageType.Warning);
						}
					}
				}
				DisplayGlyphPageNavigation(num2, num3);
			}
			GUILayout.Space(5f);
			if (GUILayout.Button("Kerning Table Info\t" + (UI_PanelState.kerningInfoPanel ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label))
			{
				UI_PanelState.kerningInfoPanel = !UI_PanelState.kerningInfoPanel;
			}
			if (UI_PanelState.kerningInfoPanel)
			{
				SerializedProperty serializedProperty2 = m_kerningInfo_prop.FindPropertyRelative("kerningPairs");
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Left Char", TMP_UIStyleManager.TMP_GUISkin.label);
				GUILayout.Label("Right Char", TMP_UIStyleManager.TMP_GUISkin.label);
				GUILayout.Label("Offset Value", TMP_UIStyleManager.TMP_GUISkin.label);
				GUILayout.Label(GUIContent.none, GUILayout.Width(20f));
				EditorGUILayout.EndHorizontal();
				GUILayout.BeginVertical(TMP_UIStyleManager.TMP_GUISkin.label);
				int arraySize = serializedProperty2.arraySize;
				int num7 = 25;
				Rect rect4;
				if (arraySize > 0)
				{
					for (int l = num7 * m_KerningPage; l < arraySize && l < num7 * (m_KerningPage + 1); l++)
					{
						SerializedProperty arrayElementAtIndex2 = serializedProperty2.GetArrayElementAtIndex(l);
						rect4 = EditorGUILayout.BeginHorizontal();
						EditorGUI.PropertyField(new Rect(rect4.x, rect4.y, rect4.width - 20f, rect4.height), arrayElementAtIndex2, GUIContent.none);
						if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
						{
							m_kerningTable.RemoveKerningPair(l);
							m_fontAsset.ReadFontDefinition();
							base.serializedObject.Update();
							isAssetDirty = true;
							break;
						}
						EditorGUILayout.EndHorizontal();
					}
				}
				Rect controlRect2 = EditorGUILayout.GetControlRect(false, 20f);
				controlRect2.width /= 3f;
				int num8 = (!current.shift) ? 1 : 10;
				if (m_KerningPage > 0)
				{
					GUI.enabled = true;
				}
				else
				{
					GUI.enabled = false;
				}
				if (GUI.Button(controlRect2, "Previous Page"))
				{
					m_KerningPage -= num8;
				}
				GUI.enabled = true;
				controlRect2.x += controlRect2.width;
				int num9 = (int)((float)arraySize / (float)num7 + 0.999f);
				GUI.Label(controlRect2, "Page " + (m_KerningPage + 1) + " / " + num9, GUI.skin.button);
				controlRect2.x += controlRect2.width;
				if (num7 * (m_GlyphPage + 1) < arraySize)
				{
					GUI.enabled = true;
				}
				else
				{
					GUI.enabled = false;
				}
				if (GUI.Button(controlRect2, "Next Page"))
				{
					m_KerningPage += num8;
				}
				m_KerningPage = Mathf.Clamp(m_KerningPage, 0, arraySize / num7);
				GUILayout.EndVertical();
				GUILayout.Space(10f);
				GUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
				rect4 = EditorGUILayout.BeginHorizontal();
				EditorGUI.PropertyField(new Rect(rect4.x, rect4.y, rect4.width - 20f, rect4.height), m_kerningPair_prop);
				GUILayout.Label(GUIContent.none, GUILayout.Height(19f));
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(5f);
				if (GUILayout.Button("Add New Kerning Pair"))
				{
					int intValue = m_kerningPair_prop.FindPropertyRelative("AscII_Left").intValue;
					int intValue2 = m_kerningPair_prop.FindPropertyRelative("AscII_Right").intValue;
					float floatValue = m_kerningPair_prop.FindPropertyRelative("XadvanceOffset").floatValue;
					errorCode = m_kerningTable.AddKerningPair(intValue, intValue2, floatValue);
					if (errorCode != -1)
					{
						m_kerningTable.SortKerningPairs();
						m_fontAsset.ReadFontDefinition();
						base.serializedObject.Update();
						isAssetDirty = true;
					}
					else
					{
						timeStamp = DateTime.Now.AddSeconds(5.0);
					}
				}
				if (errorCode == -1)
				{
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label("Kerning Pair already <color=#ffff00>exists!</color>", TMP_UIStyleManager.Label);
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					if (DateTime.Now > timeStamp)
					{
						errorCode = 0;
					}
				}
				GUILayout.EndVertical();
			}
			if (base.serializedObject.ApplyModifiedProperties() || commandName == "UndoRedoPerformed" || isAssetDirty)
			{
				TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, m_fontAsset);
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
			GUI.enabled = (m_GlyphPage > 0);
			if (GUI.Button(controlRect, "Previous Page"))
			{
				m_GlyphPage -= num;
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
			GUI.Button(controlRect, "Page " + (m_GlyphPage + 1) + " / " + num2, style);
			controlRect.x += controlRect.width;
			GUI.enabled = (itemsPerPage * (m_GlyphPage + 1) < arraySize);
			if (GUI.Button(controlRect, "Next Page"))
			{
				m_GlyphPage += num;
			}
			m_GlyphPage = Mathf.Clamp(m_GlyphPage, 0, arraySize / itemsPerPage);
			GUI.enabled = true;
		}

		private bool AddNewGlyph(int srcIndex, int dstGlyphID)
		{
			if (m_fontAsset.characterDictionary.ContainsKey(dstGlyphID))
			{
				return false;
			}
			m_glyphInfoList_prop.arraySize++;
			SerializedProperty arrayElementAtIndex = m_glyphInfoList_prop.GetArrayElementAtIndex(srcIndex);
			int index = m_glyphInfoList_prop.arraySize - 1;
			SerializedProperty target = m_glyphInfoList_prop.GetArrayElementAtIndex(index);
			CopySerializedProperty(arrayElementAtIndex, ref target);
			target.FindPropertyRelative("id").intValue = dstGlyphID;
			base.serializedObject.ApplyModifiedProperties();
			m_fontAsset.SortGlyphs();
			m_fontAsset.ReadFontDefinition();
			return true;
		}

		private void RemoveGlyphFromList(int index)
		{
			if (index <= m_glyphInfoList_prop.arraySize)
			{
				m_glyphInfoList_prop.DeleteArrayElementAtIndex(index);
				base.serializedObject.ApplyModifiedProperties();
				m_fontAsset.ReadFontDefinition();
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

		private void CopySerializedProperty(SerializedProperty source, ref SerializedProperty target)
		{
			target.FindPropertyRelative("id").intValue = source.FindPropertyRelative("id").intValue;
			target.FindPropertyRelative("x").floatValue = source.FindPropertyRelative("x").floatValue;
			target.FindPropertyRelative("y").floatValue = source.FindPropertyRelative("y").floatValue;
			target.FindPropertyRelative("width").floatValue = source.FindPropertyRelative("width").floatValue;
			target.FindPropertyRelative("height").floatValue = source.FindPropertyRelative("height").floatValue;
			target.FindPropertyRelative("xOffset").floatValue = source.FindPropertyRelative("xOffset").floatValue;
			target.FindPropertyRelative("yOffset").floatValue = source.FindPropertyRelative("yOffset").floatValue;
			target.FindPropertyRelative("xAdvance").floatValue = source.FindPropertyRelative("xAdvance").floatValue;
			target.FindPropertyRelative("scale").floatValue = source.FindPropertyRelative("scale").floatValue;
		}

		private void SearchGlyphTable(string searchPattern, ref List<int> searchResults)
		{
			if (searchResults == null)
			{
				searchResults = new List<int>();
			}
			searchResults.Clear();
			int arraySize = m_glyphInfoList_prop.arraySize;
			for (int i = 0; i < arraySize; i++)
			{
				SerializedProperty arrayElementAtIndex = m_glyphInfoList_prop.GetArrayElementAtIndex(i);
				int intValue = arrayElementAtIndex.FindPropertyRelative("id").intValue;
				if (searchPattern.Length == 1 && intValue == searchPattern[0])
				{
					searchResults.Add(i);
				}
				if (intValue.ToString().Contains(searchPattern))
				{
					searchResults.Add(i);
				}
				if (intValue.ToString("x").Contains(searchPattern))
				{
					searchResults.Add(i);
				}
				if (intValue.ToString("X").Contains(searchPattern))
				{
					searchResults.Add(i);
				}
			}
		}
	}
}
