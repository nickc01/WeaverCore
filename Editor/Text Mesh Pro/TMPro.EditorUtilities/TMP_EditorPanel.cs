using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	[CustomEditor(typeof(WeaverCore.Assets.TMPro.TextMeshPro))]
	public class Weaver_TMP_EditorPanel : TMP_EditorPanel
	{

	}


	[CustomEditor(typeof(TextMeshPro))]
	[CanEditMultipleObjects]
	public class TMP_EditorPanel : Editor
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct m_foldout
		{
			public static bool textInput = true;

			public static bool fontSettings = true;

			public static bool extraSettings = false;

			public static bool shadowSetting = false;

			public static bool materialEditor = true;
		}

		private static int m_eventID;

		private static string[] uiStateLabel = new string[2]
		{
			"\t- <i>Click to expand</i> -",
			"\t- <i>Click to collapse</i> -"
		};

		private const string k_UndoRedo = "UndoRedoPerformed";

		private GUIStyle toggleStyle;

		public int selAlignGrid_A = 0;

		public int selAlignGrid_B = 0;

		private SerializedProperty text_prop;

		private SerializedProperty isRightToLeft_prop;

		private string m_RTLText;

		private SerializedProperty fontAsset_prop;

		private SerializedProperty fontSharedMaterial_prop;

		private Material[] m_materialPresets;

		private string[] m_materialPresetNames;

		private int m_materialPresetSelectionIndex;

		private bool m_isPresetListDirty;

		private SerializedProperty fontStyle_prop;

		private SerializedProperty fontColor_prop;

		private SerializedProperty enableVertexGradient_prop;

		private SerializedProperty fontColorGradient_prop;

		private SerializedProperty fontColorGradientPreset_prop;

		private SerializedProperty overrideHtmlColor_prop;

		private SerializedProperty fontSize_prop;

		private SerializedProperty fontSizeBase_prop;

		private SerializedProperty autoSizing_prop;

		private SerializedProperty fontSizeMin_prop;

		private SerializedProperty fontSizeMax_prop;

		private SerializedProperty lineSpacingMax_prop;

		private SerializedProperty charWidthMaxAdj_prop;

		private SerializedProperty characterSpacing_prop;

		private SerializedProperty wordSpacing_prop;

		private SerializedProperty lineSpacing_prop;

		private SerializedProperty paragraphSpacing_prop;

		private SerializedProperty textAlignment_prop;

		private SerializedProperty horizontalMapping_prop;

		private SerializedProperty verticalMapping_prop;

		private SerializedProperty uvLineOffset_prop;

		private SerializedProperty enableWordWrapping_prop;

		private SerializedProperty wordWrappingRatios_prop;

		private SerializedProperty textOverflowMode_prop;

		private SerializedProperty pageToDisplay_prop;

		private SerializedProperty linkedTextComponent_prop;

		private SerializedProperty isLinkedTextComponent_prop;

		private SerializedProperty enableKerning_prop;

		private SerializedProperty inputSource_prop;

		private SerializedProperty havePropertiesChanged_prop;

		private SerializedProperty isInputPasingRequired_prop;

		private SerializedProperty isRichText_prop;

		private SerializedProperty hasFontAssetChanged_prop;

		private SerializedProperty enableExtraPadding_prop;

		private SerializedProperty checkPaddingRequired_prop;

		private SerializedProperty enableEscapeCharacterParsing_prop;

		private SerializedProperty useMaxVisibleDescender_prop;

		private SerializedProperty isVolumetricText_prop;

		private SerializedProperty geometrySortingOrder_prop;

		private SerializedProperty spriteAsset_prop;

		private SerializedProperty isOrthographic_prop;

		private bool havePropertiesChanged = false;

		private TextMeshPro m_textComponent;

		private RectTransform m_rectTransform;

		private Material m_targetMaterial;

		private SerializedProperty margin_prop;

		private Vector3[] m_rectCorners = new Vector3[4];

		private Vector3[] handlePoints = new Vector3[4];

		public void OnEnable()
		{
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(OnUndoRedo));
			text_prop = base.serializedObject.FindProperty("m_text");
			isRightToLeft_prop = base.serializedObject.FindProperty("m_isRightToLeft");
			fontAsset_prop = base.serializedObject.FindProperty("m_fontAsset");
			fontSharedMaterial_prop = base.serializedObject.FindProperty("m_sharedMaterial");
			fontStyle_prop = base.serializedObject.FindProperty("m_fontStyle");
			fontSize_prop = base.serializedObject.FindProperty("m_fontSize");
			fontSizeBase_prop = base.serializedObject.FindProperty("m_fontSizeBase");
			autoSizing_prop = base.serializedObject.FindProperty("m_enableAutoSizing");
			fontSizeMin_prop = base.serializedObject.FindProperty("m_fontSizeMin");
			fontSizeMax_prop = base.serializedObject.FindProperty("m_fontSizeMax");
			lineSpacingMax_prop = base.serializedObject.FindProperty("m_lineSpacingMax");
			charWidthMaxAdj_prop = base.serializedObject.FindProperty("m_charWidthMaxAdj");
			fontColor_prop = base.serializedObject.FindProperty("m_fontColor");
			enableVertexGradient_prop = base.serializedObject.FindProperty("m_enableVertexGradient");
			fontColorGradient_prop = base.serializedObject.FindProperty("m_fontColorGradient");
			fontColorGradientPreset_prop = base.serializedObject.FindProperty("m_fontColorGradientPreset");
			overrideHtmlColor_prop = base.serializedObject.FindProperty("m_overrideHtmlColors");
			characterSpacing_prop = base.serializedObject.FindProperty("m_characterSpacing");
			wordSpacing_prop = base.serializedObject.FindProperty("m_wordSpacing");
			lineSpacing_prop = base.serializedObject.FindProperty("m_lineSpacing");
			paragraphSpacing_prop = base.serializedObject.FindProperty("m_paragraphSpacing");
			textAlignment_prop = base.serializedObject.FindProperty("m_textAlignment");
			horizontalMapping_prop = base.serializedObject.FindProperty("m_horizontalMapping");
			verticalMapping_prop = base.serializedObject.FindProperty("m_verticalMapping");
			uvLineOffset_prop = base.serializedObject.FindProperty("m_uvLineOffset");
			enableWordWrapping_prop = base.serializedObject.FindProperty("m_enableWordWrapping");
			wordWrappingRatios_prop = base.serializedObject.FindProperty("m_wordWrappingRatios");
			textOverflowMode_prop = base.serializedObject.FindProperty("m_overflowMode");
			pageToDisplay_prop = base.serializedObject.FindProperty("m_pageToDisplay");
			linkedTextComponent_prop = base.serializedObject.FindProperty("m_linkedTextComponent");
			isLinkedTextComponent_prop = base.serializedObject.FindProperty("m_isLinkedTextComponent");
			enableKerning_prop = base.serializedObject.FindProperty("m_enableKerning");
			isOrthographic_prop = base.serializedObject.FindProperty("m_isOrthographic");
			havePropertiesChanged_prop = base.serializedObject.FindProperty("m_havePropertiesChanged");
			inputSource_prop = base.serializedObject.FindProperty("m_inputSource");
			isInputPasingRequired_prop = base.serializedObject.FindProperty("m_isInputParsingRequired");
			enableExtraPadding_prop = base.serializedObject.FindProperty("m_enableExtraPadding");
			isRichText_prop = base.serializedObject.FindProperty("m_isRichText");
			checkPaddingRequired_prop = base.serializedObject.FindProperty("checkPaddingRequired");
			enableEscapeCharacterParsing_prop = base.serializedObject.FindProperty("m_parseCtrlCharacters");
			useMaxVisibleDescender_prop = base.serializedObject.FindProperty("m_useMaxVisibleDescender");
			isVolumetricText_prop = base.serializedObject.FindProperty("m_isVolumetricText");
			geometrySortingOrder_prop = base.serializedObject.FindProperty("m_geometrySortingOrder");
			spriteAsset_prop = base.serializedObject.FindProperty("m_spriteAsset");
			margin_prop = base.serializedObject.FindProperty("m_margin");
			hasFontAssetChanged_prop = base.serializedObject.FindProperty("m_hasFontAssetChanged");
			TMP_UIStyleManager.GetUIStyles();
			m_textComponent = (base.target as TextMeshPro);
			m_rectTransform = m_textComponent.rectTransform;
			m_targetMaterial = m_textComponent.fontSharedMaterial;
			if (m_foldout.materialEditor)
			{
				InternalEditorUtility.SetIsInspectorExpanded(m_targetMaterial, true);
			}
			m_materialPresetNames = GetMaterialPresets();
		}

		public void OnDisable()
		{
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(OnUndoRedo));
		}

		public override void OnInspectorGUI()
		{
			if (toggleStyle == null)
			{
				toggleStyle = new GUIStyle(GUI.skin.label);
				toggleStyle.fontSize = 12;
				toggleStyle.normal.textColor = TMP_UIStyleManager.Section_Label.normal.textColor;
				toggleStyle.richText = true;
			}
			base.serializedObject.Update();
			Rect controlRect = EditorGUILayout.GetControlRect(false, 25f);
			float labelWidth = EditorGUIUtility.labelWidth = 130f;
			float fieldWidth = EditorGUIUtility.fieldWidth;
			controlRect.y += 2f;
			GUI.Label(controlRect, "<b>TEXT INPUT BOX</b>" + (m_foldout.textInput ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label);
			if (GUI.Button(new Rect(controlRect.x, controlRect.y, controlRect.width - 150f, controlRect.height), GUIContent.none, GUI.skin.label))
			{
				m_foldout.textInput = !m_foldout.textInput;
			}
			GUI.Label(new Rect(controlRect.width - 125f, controlRect.y + 4f, 125f, 24f), "<i>Enable RTL Editor</i>", toggleStyle);
			isRightToLeft_prop.boolValue = EditorGUI.Toggle(new Rect(controlRect.width - 10f, controlRect.y + 3f, 20f, 24f), GUIContent.none, isRightToLeft_prop.boolValue);
			if (m_foldout.textInput)
			{
				if (isLinkedTextComponent_prop.boolValue)
				{
					EditorGUILayout.HelpBox("The Text Input Box is disabled due to this text component being linked to another.", MessageType.Info);
				}
				else
				{
					EditorGUI.BeginChangeCheck();
					text_prop.stringValue = EditorGUILayout.TextArea(text_prop.stringValue, TMP_UIStyleManager.TextAreaBoxEditor, GUILayout.Height(125f), GUILayout.ExpandWidth(true));
					if (EditorGUI.EndChangeCheck() || (isRightToLeft_prop.boolValue && (m_RTLText == null || m_RTLText == string.Empty)))
					{
						inputSource_prop.enumValueIndex = 0;
						isInputPasingRequired_prop.boolValue = true;
						havePropertiesChanged = true;
						if (isRightToLeft_prop.boolValue)
						{
							m_RTLText = string.Empty;
							string stringValue = text_prop.stringValue;
							for (int i = 0; i < stringValue.Length; i++)
							{
								m_RTLText += stringValue[stringValue.Length - i - 1];
							}
						}
					}
					if (isRightToLeft_prop.boolValue)
					{
						EditorGUI.BeginChangeCheck();
						m_RTLText = EditorGUILayout.TextArea(m_RTLText, TMP_UIStyleManager.TextAreaBoxEditor, GUILayout.Height(125f), GUILayout.ExpandWidth(true));
						if (EditorGUI.EndChangeCheck())
						{
							string text = string.Empty;
							for (int j = 0; j < m_RTLText.Length; j++)
							{
								text += m_RTLText[m_RTLText.Length - j - 1];
							}
							text_prop.stringValue = text;
						}
					}
				}
			}
			if (GUILayout.Button("<b>FONT SETTINGS</b>" + (m_foldout.fontSettings ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label))
			{
				m_foldout.fontSettings = !m_foldout.fontSettings;
			}
			if (m_isPresetListDirty)
			{
				m_materialPresetNames = GetMaterialPresets();
			}
			if (m_foldout.fontSettings)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(fontAsset_prop);
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
					hasFontAssetChanged_prop.boolValue = true;
					m_isPresetListDirty = true;
					m_materialPresetSelectionIndex = 0;
				}
				if (m_materialPresetNames != null)
				{
					EditorGUI.BeginChangeCheck();
					controlRect = EditorGUILayout.GetControlRect(false, 18f);
					float fixedHeight = EditorStyles.popup.fixedHeight;
					EditorStyles.popup.fixedHeight = controlRect.height;
					int fontSize = EditorStyles.popup.fontSize;
					EditorStyles.popup.fontSize = 11;
					m_materialPresetSelectionIndex = EditorGUI.Popup(controlRect, "Material Preset", m_materialPresetSelectionIndex, m_materialPresetNames);
					if (EditorGUI.EndChangeCheck())
					{
						fontSharedMaterial_prop.objectReferenceValue = m_materialPresets[m_materialPresetSelectionIndex];
						havePropertiesChanged = true;
					}
					if (m_materialPresetSelectionIndex < m_materialPresetNames.Length && m_targetMaterial != m_materialPresets[m_materialPresetSelectionIndex] && !havePropertiesChanged)
					{
						m_isPresetListDirty = true;
					}
					EditorStyles.popup.fixedHeight = fixedHeight;
					EditorStyles.popup.fontSize = fontSize;
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Font Style");
				int intValue = fontStyle_prop.intValue;
				int num2 = GUILayout.Toggle((intValue & 1) == 1, "B", GUI.skin.button) ? 1 : 0;
				int num3 = GUILayout.Toggle((intValue & 2) == 2, "I", GUI.skin.button) ? 2 : 0;
				int num4 = GUILayout.Toggle((intValue & 4) == 4, "U", GUI.skin.button) ? 4 : 0;
				int num5 = GUILayout.Toggle((intValue & 0x40) == 64, "S", GUI.skin.button) ? 64 : 0;
				int num6 = GUILayout.Toggle((intValue & 8) == 8, "ab", GUI.skin.button) ? 8 : 0;
				int num7 = GUILayout.Toggle((intValue & 0x10) == 16, "AB", GUI.skin.button) ? 16 : 0;
				int num8 = GUILayout.Toggle((intValue & 0x20) == 32, "SC", GUI.skin.button) ? 32 : 0;
				EditorGUILayout.EndHorizontal();
				if (EditorGUI.EndChangeCheck())
				{
					fontStyle_prop.intValue = num2 + num3 + num4 + num6 + num7 + num8 + num5;
					havePropertiesChanged = true;
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(fontColor_prop, new GUIContent("Color (Vertex)"));
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(enableVertexGradient_prop, new GUIContent("Color Gradient"), GUILayout.MinWidth(140f), GUILayout.MaxWidth(200f));
				EditorGUIUtility.labelWidth = 95f;
				EditorGUILayout.PropertyField(overrideHtmlColor_prop, new GUIContent("Override Tags"));
				EditorGUIUtility.labelWidth = labelWidth;
				EditorGUILayout.EndHorizontal();
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
				if (enableVertexGradient_prop.boolValue)
				{
					EditorGUILayout.PropertyField(fontColorGradientPreset_prop, new GUIContent("Gradient (Preset)"));
					if (fontColorGradientPreset_prop.objectReferenceValue == null)
					{
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField(fontColorGradient_prop.FindPropertyRelative("topLeft"), new GUIContent("Top Left"));
						EditorGUILayout.PropertyField(fontColorGradient_prop.FindPropertyRelative("topRight"), new GUIContent("Top Right"));
						EditorGUILayout.PropertyField(fontColorGradient_prop.FindPropertyRelative("bottomLeft"), new GUIContent("Bottom Left"));
						EditorGUILayout.PropertyField(fontColorGradient_prop.FindPropertyRelative("bottomRight"), new GUIContent("Bottom Right"));
						if (EditorGUI.EndChangeCheck())
						{
							havePropertiesChanged = true;
						}
					}
					else
					{
						SerializedObject serializedObject = new SerializedObject(fontColorGradientPreset_prop.objectReferenceValue);
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("topLeft"), new GUIContent("Top Left"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("topRight"), new GUIContent("Top Right"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("bottomLeft"), new GUIContent("Bottom Left"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("bottomRight"), new GUIContent("Bottom Right"));
						if (EditorGUI.EndChangeCheck())
						{
							serializedObject.ApplyModifiedProperties();
							havePropertiesChanged = true;
							TMPro_EventManager.ON_COLOR_GRAIDENT_PROPERTY_CHANGED(fontColorGradientPreset_prop.objectReferenceValue as TMP_ColorGradient);
						}
					}
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(fontSize_prop, new GUIContent("Font Size"), GUILayout.MinWidth(168f), GUILayout.MaxWidth(200f));
				EditorGUIUtility.fieldWidth = fieldWidth;
				if (EditorGUI.EndChangeCheck())
				{
					fontSizeBase_prop.floatValue = fontSize_prop.floatValue;
					havePropertiesChanged = true;
				}
				EditorGUI.BeginChangeCheck();
				EditorGUIUtility.labelWidth = 70f;
				EditorGUILayout.PropertyField(autoSizing_prop, new GUIContent("Auto Size"));
				EditorGUILayout.EndHorizontal();
				EditorGUIUtility.labelWidth = labelWidth;
				if (EditorGUI.EndChangeCheck())
				{
					if (!autoSizing_prop.boolValue)
					{
						fontSize_prop.floatValue = fontSizeBase_prop.floatValue;
					}
					havePropertiesChanged = true;
				}
				if (autoSizing_prop.boolValue)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Auto Size Options");
					EditorGUIUtility.labelWidth = 24f;
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(fontSizeMin_prop, new GUIContent("Min"), GUILayout.MinWidth(46f));
					if (EditorGUI.EndChangeCheck())
					{
						fontSizeMin_prop.floatValue = Mathf.Min(fontSizeMin_prop.floatValue, fontSizeMax_prop.floatValue);
						havePropertiesChanged = true;
					}
					EditorGUIUtility.labelWidth = 27f;
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(fontSizeMax_prop, new GUIContent("Max"), GUILayout.MinWidth(49f));
					if (EditorGUI.EndChangeCheck())
					{
						fontSizeMax_prop.floatValue = Mathf.Max(fontSizeMin_prop.floatValue, fontSizeMax_prop.floatValue);
						havePropertiesChanged = true;
					}
					EditorGUI.BeginChangeCheck();
					EditorGUIUtility.labelWidth = 36f;
					EditorGUILayout.PropertyField(charWidthMaxAdj_prop, new GUIContent("WD%"), GUILayout.MinWidth(58f));
					EditorGUIUtility.labelWidth = 28f;
					EditorGUILayout.PropertyField(lineSpacingMax_prop, new GUIContent("Line"), GUILayout.MinWidth(50f));
					EditorGUIUtility.labelWidth = labelWidth;
					EditorGUILayout.EndHorizontal();
					if (EditorGUI.EndChangeCheck())
					{
						charWidthMaxAdj_prop.floatValue = Mathf.Clamp(charWidthMaxAdj_prop.floatValue, 0f, 50f);
						lineSpacingMax_prop.floatValue = Mathf.Min(0f, lineSpacingMax_prop.floatValue);
						havePropertiesChanged = true;
					}
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Spacing Options");
				EditorGUIUtility.labelWidth = 35f;
				EditorGUILayout.PropertyField(characterSpacing_prop, new GUIContent("Char"), GUILayout.MinWidth(50f));
				EditorGUILayout.PropertyField(wordSpacing_prop, new GUIContent("Word"), GUILayout.MinWidth(50f));
				EditorGUILayout.PropertyField(lineSpacing_prop, new GUIContent("Line"), GUILayout.MinWidth(50f));
				EditorGUILayout.PropertyField(paragraphSpacing_prop, new GUIContent(" Par."), GUILayout.MinWidth(50f));
				EditorGUIUtility.labelWidth = labelWidth;
				EditorGUILayout.EndHorizontal();
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
				EditorGUI.BeginChangeCheck();
				controlRect = EditorGUILayout.GetControlRect(false, 19f);
				GUIStyle gUIStyle = new GUIStyle(GUI.skin.button);
				gUIStyle.margin = new RectOffset(1, 1, 1, 1);
				gUIStyle.padding = new RectOffset(1, 1, 1, 0);
				selAlignGrid_A = TMP_EditorUtility.GetHorizontalAlignmentGridValue(textAlignment_prop.intValue);
				selAlignGrid_B = TMP_EditorUtility.GetVerticalAlignmentGridValue(textAlignment_prop.intValue);
				GUI.Label(new Rect(controlRect.x, controlRect.y + 2f, 100f, controlRect.height), "Alignment");
				float num9 = EditorGUIUtility.labelWidth + 15f;
				selAlignGrid_A = GUI.SelectionGrid(new Rect(num9, controlRect.y, 138f, controlRect.height), selAlignGrid_A, TMP_UIStyleManager.alignContent_A, 6, gUIStyle);
				selAlignGrid_B = GUI.SelectionGrid(new Rect(num9 + 138f + 20f, controlRect.y, 138f, controlRect.height), selAlignGrid_B, TMP_UIStyleManager.alignContent_B, 6, gUIStyle);
				if (EditorGUI.EndChangeCheck())
				{
					//int intValue2 = (1 << selAlignGrid_A) | (256 << selAlignGrid_B);
					int intValue2 = selAlignGrid_A + (selAlignGrid_B * 4);
					textAlignment_prop.intValue = intValue2;
					havePropertiesChanged = true;
				}
				EditorGUI.BeginChangeCheck();
				//if ((textAlignment_prop.intValue & 8) == 8 || (textAlignment_prop.intValue & 0x10) == 16)
				if (selAlignGrid_A == 3)
				{
					DrawPropertySlider("Wrap Mix (W <-> C)", wordWrappingRatios_prop);
				}
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
				EditorGUI.BeginChangeCheck();
				controlRect = EditorGUILayout.GetControlRect(false);
				EditorGUI.PrefixLabel(new Rect(controlRect.x, controlRect.y, 130f, controlRect.height), new GUIContent("Wrapping & Overflow"));
				controlRect.width = (controlRect.width - 130f) / 2f;
				controlRect.x += 130f;
				int num10 = EditorGUI.Popup(controlRect, enableWordWrapping_prop.boolValue ? 1 : 0, new string[2]
				{
					"Disabled",
					"Enabled"
				});
				if (EditorGUI.EndChangeCheck())
				{
					enableWordWrapping_prop.boolValue = ((num10 == 1) ? true : false);
					havePropertiesChanged = true;
					isInputPasingRequired_prop.boolValue = true;
				}
				EditorGUI.BeginChangeCheck();
				TMP_Text exists = linkedTextComponent_prop.objectReferenceValue as TMP_Text;
				if (textOverflowMode_prop.enumValueIndex == 6)
				{
					controlRect.x += controlRect.width + 5f;
					controlRect.width /= 3f;
					EditorGUI.PropertyField(controlRect, textOverflowMode_prop, GUIContent.none);
					controlRect.x += controlRect.width;
					controlRect.width = controlRect.width * 2f - 5f;
					EditorGUI.PropertyField(controlRect, linkedTextComponent_prop, GUIContent.none);
					if (GUI.changed)
					{
						TMP_Text tMP_Text = linkedTextComponent_prop.objectReferenceValue as TMP_Text;
						if ((bool)(UnityEngine.Object)(object)tMP_Text)
						{
							m_textComponent.linkedTextComponent = tMP_Text;
						}
					}
				}
				else if (textOverflowMode_prop.enumValueIndex == 5)
				{
					controlRect.x += controlRect.width + 5f;
					controlRect.width /= 2f;
					EditorGUI.PropertyField(controlRect, textOverflowMode_prop, GUIContent.none);
					controlRect.x += controlRect.width;
					controlRect.width -= 5f;
					EditorGUI.PropertyField(controlRect, pageToDisplay_prop, GUIContent.none);
					if ((bool)(UnityEngine.Object)(object)exists)
					{
						m_textComponent.linkedTextComponent = null;
					}
				}
				else
				{
					controlRect.x += controlRect.width + 5f;
					controlRect.width -= 5f;
					EditorGUI.PropertyField(controlRect, textOverflowMode_prop, GUIContent.none);
					if ((bool)(UnityEngine.Object)(object)exists)
					{
						m_textComponent.linkedTextComponent = null;
					}
				}
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
					isInputPasingRequired_prop.boolValue = true;
				}
				EditorGUI.BeginChangeCheck();
				controlRect = EditorGUILayout.GetControlRect(false);
				EditorGUI.PrefixLabel(new Rect(controlRect.x, controlRect.y, 130f, controlRect.height), new GUIContent("UV Mapping Options"));
				controlRect.width = (controlRect.width - 130f) / 2f;
				controlRect.x += 130f;
				EditorGUI.PropertyField(controlRect, horizontalMapping_prop, GUIContent.none);
				controlRect.x += controlRect.width + 5f;
				controlRect.width -= 5f;
				EditorGUI.PropertyField(controlRect, verticalMapping_prop, GUIContent.none);
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
				if (horizontalMapping_prop.enumValueIndex > 0)
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(uvLineOffset_prop, new GUIContent("UV Line Offset"), GUILayout.MinWidth(70f));
					if (EditorGUI.EndChangeCheck())
					{
						havePropertiesChanged = true;
					}
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(enableKerning_prop, new GUIContent("Enable Kerning?"));
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(enableExtraPadding_prop, new GUIContent("Extra Padding?"));
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
					checkPaddingRequired_prop.boolValue = true;
				}
				EditorGUILayout.EndHorizontal();
			}
			if (GUILayout.Button("<b>EXTRA SETTINGS</b>" + (m_foldout.extraSettings ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label))
			{
				m_foldout.extraSettings = !m_foldout.extraSettings;
			}
			if (m_foldout.extraSettings)
			{
				EditorGUI.indentLevel = 0;
				EditorGUI.BeginChangeCheck();
				DrawMaginProperty(margin_prop, "Margins");
				if (EditorGUI.EndChangeCheck())
				{
					m_textComponent.margin = margin_prop.vector4Value;
					havePropertiesChanged = true;
				}
				GUILayout.Space(10f);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Sorting Layer");
				EditorGUI.BeginChangeCheck();
				float labelWidth2 = EditorGUIUtility.labelWidth;
				float fieldWidth2 = EditorGUIUtility.fieldWidth;
				string[] sortingLayerNames = SortingLayerHelper.sortingLayerNames;
				string sortingLayerNameFromID = SortingLayerHelper.GetSortingLayerNameFromID(m_textComponent.sortingLayerID);
				int num11 = Array.IndexOf(sortingLayerNames, sortingLayerNameFromID);
				EditorGUIUtility.fieldWidth = 0f;
				int num12 = EditorGUILayout.Popup(string.Empty, num11, sortingLayerNames, GUILayout.MinWidth(80f));
				if (num12 != num11)
				{
					m_textComponent.sortingLayerID = SortingLayerHelper.GetSortingLayerIDForIndex(num12);
				}
				EditorGUIUtility.labelWidth = 40f;
				EditorGUIUtility.fieldWidth = 80f;
				int num13 = EditorGUILayout.IntField("Order", m_textComponent.sortingOrder);
				if (num13 != m_textComponent.sortingOrder)
				{
					m_textComponent.sortingOrder = num13;
				}
				EditorGUILayout.EndHorizontal();
				EditorGUIUtility.labelWidth = labelWidth2;
				EditorGUIUtility.fieldWidth = fieldWidth2;
				EditorGUILayout.PropertyField(geometrySortingOrder_prop, new GUIContent("Geometry Sorting"));
				EditorGUI.BeginChangeCheck();
				EditorGUIUtility.labelWidth = 150f;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(isOrthographic_prop, new GUIContent("Orthographic Mode?"));
				EditorGUILayout.PropertyField(isRichText_prop, new GUIContent("Enable Rich Text?"));
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(enableEscapeCharacterParsing_prop, new GUIContent("Parse Escape Characters"));
				EditorGUILayout.PropertyField(useMaxVisibleDescender_prop, new GUIContent("Use Visible Descender"));
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.PropertyField(spriteAsset_prop, new GUIContent("Sprite Asset", "The Sprite Asset used when NOT specifically referencing one using <sprite=\"Sprite Asset Name\"."), true);
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(isVolumetricText_prop, new GUIContent("Enabled Volumetric Setup"));
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
					m_textComponent.textInfo.ResetVertexLayout(isVolumetricText_prop.boolValue);
				}
				EditorGUIUtility.labelWidth = 135f;
				GUILayout.Space(10f);
			}
			if (havePropertiesChanged)
			{
				havePropertiesChanged_prop.boolValue = true;
				havePropertiesChanged = false;
			}
			EditorGUILayout.Space();
			base.serializedObject.ApplyModifiedProperties();
		}

		public void OnSceneGUI()
		{
			if (!IsMixSelectionTypes())
			{
				m_rectTransform.GetWorldCorners(m_rectCorners);
				Vector4 margin = m_textComponent.margin;
				Vector3 lossyScale = m_rectTransform.lossyScale;
				handlePoints[0] = m_rectCorners[0] + m_rectTransform.TransformDirection(new Vector3(margin.x * lossyScale.x, margin.w * lossyScale.y, 0f));
				handlePoints[1] = m_rectCorners[1] + m_rectTransform.TransformDirection(new Vector3(margin.x * lossyScale.x, (0f - margin.y) * lossyScale.y, 0f));
				handlePoints[2] = m_rectCorners[2] + m_rectTransform.TransformDirection(new Vector3((0f - margin.z) * lossyScale.x, (0f - margin.y) * lossyScale.y, 0f));
				handlePoints[3] = m_rectCorners[3] + m_rectTransform.TransformDirection(new Vector3((0f - margin.z) * lossyScale.x, margin.w * lossyScale.y, 0f));
				Handles.color = Color.yellow;
				Handles.DrawSolidRectangleWithOutline(handlePoints, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0), new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
				Vector3 vector = (handlePoints[0] + handlePoints[1]) * 0.5f;
				Vector3 rhs = Handles.FreeMoveHandle(vector, Quaternion.identity, HandleUtility.GetHandleSize(m_rectTransform.position) * 0.05f, Vector3.zero, Handles.DotHandleCap);
				bool flag = false;
				if (vector != rhs)
				{
					float num = vector.x - rhs.x;
					margin.x += (0f - num) / lossyScale.x;
					flag = true;
				}
				Vector3 vector2 = (handlePoints[1] + handlePoints[2]) * 0.5f;
				Vector3 rhs2 = Handles.FreeMoveHandle(vector2, Quaternion.identity, HandleUtility.GetHandleSize(m_rectTransform.position) * 0.05f, Vector3.zero, Handles.DotHandleCap);
				if (vector2 != rhs2)
				{
					float num2 = vector2.y - rhs2.y;
					margin.y += num2 / lossyScale.y;
					flag = true;
				}
				Vector3 vector3 = (handlePoints[2] + handlePoints[3]) * 0.5f;
				Vector3 rhs3 = Handles.FreeMoveHandle(vector3, Quaternion.identity, HandleUtility.GetHandleSize(m_rectTransform.position) * 0.05f, Vector3.zero, Handles.DotHandleCap);
				if (vector3 != rhs3)
				{
					float num3 = vector3.x - rhs3.x;
					margin.z += num3 / lossyScale.x;
					flag = true;
				}
				Vector3 vector4 = (handlePoints[3] + handlePoints[0]) * 0.5f;
				Vector3 rhs4 = Handles.FreeMoveHandle(vector4, Quaternion.identity, HandleUtility.GetHandleSize(m_rectTransform.position) * 0.05f, Vector3.zero, Handles.DotHandleCap);
				if (vector4 != rhs4)
				{
					float num4 = vector4.y - rhs4.y;
					margin.w += (0f - num4) / lossyScale.y;
					flag = true;
				}
				if (flag)
				{
					Undo.RecordObjects(new UnityEngine.Object[2]
					{
						m_rectTransform,
						(UnityEngine.Object)(object)m_textComponent
					}, "Margin Changes");
					m_textComponent.margin = margin;
					EditorUtility.SetDirty(base.target);
				}
			}
		}

		private bool IsMixSelectionTypes()
		{
			UnityEngine.Object[] gameObjects = Selection.gameObjects;
			if (gameObjects.Length > 1)
			{
				for (int i = 0; i < gameObjects.Length; i++)
				{
					if ((UnityEngine.Object)(object)((GameObject)gameObjects[i]).GetComponent<TextMeshPro>() == null)
					{
						return true;
					}
				}
			}
			return false;
		}

		private string[] GetMaterialPresets()
		{
			TMP_FontAsset tMP_FontAsset = fontAsset_prop.objectReferenceValue as TMP_FontAsset;
			if (tMP_FontAsset == null)
			{
				return null;
			}
			m_materialPresets = TMP_EditorUtility.FindMaterialReferences(tMP_FontAsset);
			m_materialPresetNames = new string[m_materialPresets.Length];
			for (int i = 0; i < m_materialPresetNames.Length; i++)
			{
				m_materialPresetNames[i] = m_materialPresets[i].name;
				if (m_targetMaterial.GetInstanceID() == m_materialPresets[i].GetInstanceID())
				{
					m_materialPresetSelectionIndex = i;
				}
			}
			m_isPresetListDirty = false;
			return m_materialPresetNames;
		}

		private void DrawMaginProperty(SerializedProperty property, string label)
		{
			float labelWidth = EditorGUIUtility.labelWidth;
			float fieldWidth = EditorGUIUtility.fieldWidth;
			Rect controlRect = EditorGUILayout.GetControlRect(false, 36f);
			Rect position = new Rect(controlRect.x, controlRect.y + 2f, controlRect.width, 18f);
			float num = controlRect.width + 3f;
			position.width = labelWidth;
			GUI.Label(position, label);
			Vector4 vector4Value = property.vector4Value;
			float num2 = num - labelWidth;
			float num3 = num2 / 4f;
			position.width = num3 - 5f;
			position.x = labelWidth + 15f;
			GUI.Label(position, "Left");
			position.x += num3;
			GUI.Label(position, "Top");
			position.x += num3;
			GUI.Label(position, "Right");
			position.x += num3;
			GUI.Label(position, "Bottom");
			position.y += 18f;
			position.x = labelWidth + 15f;
			vector4Value.x = EditorGUI.FloatField(position, GUIContent.none, vector4Value.x);
			position.x += num3;
			vector4Value.y = EditorGUI.FloatField(position, GUIContent.none, vector4Value.y);
			position.x += num3;
			vector4Value.z = EditorGUI.FloatField(position, GUIContent.none, vector4Value.z);
			position.x += num3;
			vector4Value.w = EditorGUI.FloatField(position, GUIContent.none, vector4Value.w);
			property.vector4Value = vector4Value;
			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUIUtility.fieldWidth = fieldWidth;
		}

		private void DrawPropertySlider(string label, SerializedProperty property)
		{
			float labelWidth = EditorGUIUtility.labelWidth;
			float fieldWidth = EditorGUIUtility.fieldWidth;
			Rect controlRect = EditorGUILayout.GetControlRect(false, 17f);
			GUIContent label2 = (label == "") ? GUIContent.none : new GUIContent(label);
			EditorGUI.Slider(new Rect(controlRect.x, controlRect.y, controlRect.width, controlRect.height), property, 0f, 1f, label2);
			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUIUtility.fieldWidth = fieldWidth;
		}

		private void DrawDoubleEnumPopup(SerializedProperty property1, SerializedProperty property2, string label)
		{
			float labelWidth = EditorGUIUtility.labelWidth;
			float fieldWidth = EditorGUIUtility.fieldWidth;
			Rect controlRect = EditorGUILayout.GetControlRect(false, 17f);
			Rect rect = new Rect(controlRect.x, controlRect.y, EditorGUIUtility.labelWidth, controlRect.height);
			EditorGUI.PrefixLabel(rect, new GUIContent(label));
			rect.x += rect.width;
			rect.width = (controlRect.width - rect.x) / 2f + 5f;
			EditorGUI.PropertyField(rect, property1, GUIContent.none);
			rect.x += rect.width + 5f;
			EditorGUI.PropertyField(rect, property2, GUIContent.none);
			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUIUtility.fieldWidth = fieldWidth;
		}

		private void DrawPropertyBlock(string[] labels, SerializedProperty[] properties)
		{
			float labelWidth = EditorGUIUtility.labelWidth;
			float fieldWidth = EditorGUIUtility.fieldWidth;
			Rect controlRect = EditorGUILayout.GetControlRect(false, 17f);
			GUI.Label(new Rect(controlRect.x, controlRect.y, labelWidth, controlRect.height), labels[0]);
			controlRect.x = labelWidth + 15f;
			controlRect.width = (controlRect.width + 20f - controlRect.x) / (float)labels.Length;
			for (int i = 0; i < labels.Length; i++)
			{
				if (i == 0)
				{
					EditorGUIUtility.labelWidth = 20f;
					GUI.enabled = ((properties[i] != fontSize_prop || !autoSizing_prop.boolValue) ? (GUI.enabled = true) : (GUI.enabled = false));
					EditorGUI.PropertyField(new Rect(controlRect.x - 20f, controlRect.y, 80f, controlRect.height), properties[i], new GUIContent("  "));
					controlRect.x += controlRect.width;
					GUI.enabled = true;
				}
				else
				{
					EditorGUIUtility.labelWidth = GUI.skin.textArea.CalcSize(new GUIContent(labels[i])).x;
					EditorGUI.PropertyField(new Rect(controlRect.x, controlRect.y, controlRect.width - 5f, controlRect.height), properties[i], new GUIContent(labels[i]));
					controlRect.x += controlRect.width;
				}
			}
			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUIUtility.fieldWidth = fieldWidth;
		}

		private void OnUndoRedo()
		{
			int currentGroup = Undo.GetCurrentGroup();
			int eventID = m_eventID;
			if (currentGroup != eventID)
			{
				for (int i = 0; i < base.targets.Length; i++)
				{
					TMPro_EventManager.ON_TEXTMESHPRO_PROPERTY_CHANGED(true, base.targets[i] as TextMeshPro);
					m_eventID = currentGroup;
				}
			}
		}
	}
}
