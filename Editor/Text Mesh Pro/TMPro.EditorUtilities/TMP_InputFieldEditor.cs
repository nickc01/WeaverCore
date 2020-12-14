using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.UI;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(TMP_InputField), true)]
	public class TMP_InputFieldEditor : SelectableEditor
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct m_foldout
		{
			public static bool textInput = true;

			public static bool fontSettings = true;

			public static bool extraSettings = true;
		}

		private static string[] uiStateLabel = new string[2]
		{
			"\t- <i>Click to expand</i> -",
			"\t- <i>Click to collapse</i> -"
		};

		private SerializedProperty m_TextViewport;

		private SerializedProperty m_TextComponent;

		private SerializedProperty m_Text;

		private SerializedProperty m_ContentType;

		private SerializedProperty m_LineType;

		private SerializedProperty m_InputType;

		private SerializedProperty m_CharacterValidation;

		private SerializedProperty m_InputValidator;

		private SerializedProperty m_RegexValue;

		private SerializedProperty m_KeyboardType;

		private SerializedProperty m_CharacterLimit;

		private SerializedProperty m_CaretBlinkRate;

		private SerializedProperty m_CaretWidth;

		private SerializedProperty m_CaretColor;

		private SerializedProperty m_CustomCaretColor;

		private SerializedProperty m_SelectionColor;

		private SerializedProperty m_HideMobileInput;

		private SerializedProperty m_Placeholder;

		private SerializedProperty m_VerticalScrollbar;

		private SerializedProperty m_ScrollbarScrollSensitivity;

		private SerializedProperty m_OnValueChanged;

		private SerializedProperty m_OnEndEdit;

		private SerializedProperty m_OnSelect;

		private SerializedProperty m_OnDeselect;

		private SerializedProperty m_ReadOnly;

		private SerializedProperty m_RichText;

		private SerializedProperty m_RichTextEditingAllowed;

		private SerializedProperty m_ResetOnDeActivation;

		private SerializedProperty m_RestoreOriginalTextOnEscape;

		private SerializedProperty m_OnFocusSelectAll;

		private SerializedProperty m_GlobalPointSize;

		private SerializedProperty m_GlobalFontAsset;

		private AnimBool m_CustomColor;

		private TMP_InputValidator m_ValidationScript;

		protected override void OnEnable()
		{
			//((SelectableEditor)this).OnEnable();
			base.OnEnable();
			m_TextViewport = base.serializedObject.FindProperty("m_TextViewport");
			m_TextComponent = base.serializedObject.FindProperty("m_TextComponent");
			m_Text = base.serializedObject.FindProperty("m_Text");
			m_ContentType = base.serializedObject.FindProperty("m_ContentType");
			m_LineType = base.serializedObject.FindProperty("m_LineType");
			m_InputType = base.serializedObject.FindProperty("m_InputType");
			m_CharacterValidation = base.serializedObject.FindProperty("m_CharacterValidation");
			m_InputValidator = base.serializedObject.FindProperty("m_InputValidator");
			m_RegexValue = base.serializedObject.FindProperty("m_RegexValue");
			m_KeyboardType = base.serializedObject.FindProperty("m_KeyboardType");
			m_CharacterLimit = base.serializedObject.FindProperty("m_CharacterLimit");
			m_CaretBlinkRate = base.serializedObject.FindProperty("m_CaretBlinkRate");
			m_CaretWidth = base.serializedObject.FindProperty("m_CaretWidth");
			m_CaretColor = base.serializedObject.FindProperty("m_CaretColor");
			m_CustomCaretColor = base.serializedObject.FindProperty("m_CustomCaretColor");
			m_SelectionColor = base.serializedObject.FindProperty("m_SelectionColor");
			m_HideMobileInput = base.serializedObject.FindProperty("m_HideMobileInput");
			m_Placeholder = base.serializedObject.FindProperty("m_Placeholder");
			m_VerticalScrollbar = base.serializedObject.FindProperty("m_VerticalScrollbar");
			m_ScrollbarScrollSensitivity = base.serializedObject.FindProperty("m_ScrollSensitivity");
			m_OnValueChanged = base.serializedObject.FindProperty("m_OnValueChanged");
			m_OnEndEdit = base.serializedObject.FindProperty("m_OnEndEdit");
			m_OnSelect = base.serializedObject.FindProperty("m_OnSelect");
			m_OnDeselect = base.serializedObject.FindProperty("m_OnDeselect");
			m_ReadOnly = base.serializedObject.FindProperty("m_ReadOnly");
			m_RichText = base.serializedObject.FindProperty("m_RichText");
			m_RichTextEditingAllowed = base.serializedObject.FindProperty("m_isRichTextEditingAllowed");
			m_ResetOnDeActivation = base.serializedObject.FindProperty("m_ResetOnDeActivation");
			m_RestoreOriginalTextOnEscape = base.serializedObject.FindProperty("m_RestoreOriginalTextOnEscape");
			m_OnFocusSelectAll = base.serializedObject.FindProperty("m_OnFocusSelectAll");
			m_GlobalPointSize = base.serializedObject.FindProperty("m_GlobalPointSize");
			m_GlobalFontAsset = base.serializedObject.FindProperty("m_GlobalFontAsset");
			m_CustomColor = new AnimBool(m_CustomCaretColor.boolValue);
			m_CustomColor.valueChanged.AddListener(((Editor)(object)this).Repaint);
			TMP_UIStyleManager.GetUIStyles();
		}

		protected override void OnDisable()
		{
			//((SelectableEditor)this).OnDisable();
			base.OnDisable();
			m_CustomColor.valueChanged.RemoveListener(((Editor)(object)this).Repaint);
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();
			//((SelectableEditor)this).OnInspectorGUI();
			base.OnInspectorGUI();
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(m_TextViewport);
			EditorGUILayout.PropertyField(m_TextComponent);
			TextMeshProUGUI textMeshProUGUI = null;
			if (m_TextComponent != null && m_TextComponent.objectReferenceValue != null)
			{
				textMeshProUGUI = (m_TextComponent.objectReferenceValue as TextMeshProUGUI);
			}
			EditorGUI.BeginDisabledGroup(m_TextComponent == null || m_TextComponent.objectReferenceValue == null);
			Rect controlRect = EditorGUILayout.GetControlRect(false, 25f);
			EditorGUIUtility.labelWidth = 130f;
			controlRect.y += 2f;
			GUI.Label(controlRect, "<b>TEXT INPUT BOX</b>" + (m_foldout.textInput ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label);
			if (GUI.Button(new Rect(controlRect.x, controlRect.y, controlRect.width - 150f, controlRect.height), GUIContent.none, GUI.skin.label))
			{
				m_foldout.textInput = !m_foldout.textInput;
			}
			if (m_foldout.textInput)
			{
				EditorGUI.BeginChangeCheck();
				m_Text.stringValue = EditorGUILayout.TextArea(m_Text.stringValue, TMP_UIStyleManager.TextAreaBoxEditor, GUILayout.Height(125f), GUILayout.ExpandWidth(true));
			}
			if (GUILayout.Button("<b>INPUT FIELD SETTINGS</b>" + (m_foldout.fontSettings ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label))
			{
				m_foldout.fontSettings = !m_foldout.fontSettings;
			}
			if (m_foldout.fontSettings)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(m_GlobalFontAsset, new GUIContent("Font Asset", "Set the Font Asset for both Placeholder and Input Field text object."));
				if (EditorGUI.EndChangeCheck())
				{
					TMP_InputField tMP_InputField = base.target as TMP_InputField;
					tMP_InputField.SetGlobalFontAsset(m_GlobalFontAsset.objectReferenceValue as TMP_FontAsset);
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(m_GlobalPointSize, new GUIContent("Point Size", "Set the point size of both Placeholder and Input Field text object."));
				if (EditorGUI.EndChangeCheck())
				{
					TMP_InputField tMP_InputField2 = base.target as TMP_InputField;
					tMP_InputField2.SetGlobalPointSize(m_GlobalPointSize.floatValue);
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(m_CharacterLimit);
				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(m_ContentType);
				if (!m_ContentType.hasMultipleDifferentValues)
				{
					EditorGUI.indentLevel++;
					if (m_ContentType.enumValueIndex == 0 || m_ContentType.enumValueIndex == 1 || m_ContentType.enumValueIndex == 9)
					{
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField(m_LineType);
						if (EditorGUI.EndChangeCheck() && (Object)(object)textMeshProUGUI != null)
						{
							if (m_LineType.enumValueIndex == 0)
							{
								textMeshProUGUI.enableWordWrapping = false;
							}
							else
							{
								textMeshProUGUI.enableWordWrapping = true;
							}
						}
					}
					if (m_ContentType.enumValueIndex == 9)
					{
						EditorGUILayout.PropertyField(m_InputType);
						EditorGUILayout.PropertyField(m_KeyboardType);
						EditorGUILayout.PropertyField(m_CharacterValidation);
						if (m_CharacterValidation.enumValueIndex == 6)
						{
							EditorGUILayout.PropertyField(m_RegexValue);
						}
						else if (m_CharacterValidation.enumValueIndex == 8)
						{
							EditorGUILayout.PropertyField(m_InputValidator);
						}
					}
					EditorGUI.indentLevel--;
				}
				EditorGUILayout.Space();
				EditorGUILayout.PropertyField(m_Placeholder);
				EditorGUILayout.PropertyField(m_VerticalScrollbar);
				if (m_VerticalScrollbar.objectReferenceValue != null)
				{
					EditorGUILayout.PropertyField(m_ScrollbarScrollSensitivity);
				}
				EditorGUILayout.PropertyField(m_CaretBlinkRate);
				EditorGUILayout.PropertyField(m_CaretWidth);
				EditorGUILayout.PropertyField(m_CustomCaretColor);
				m_CustomColor.target = m_CustomCaretColor.boolValue;
				if (EditorGUILayout.BeginFadeGroup(m_CustomColor.faded))
				{
					EditorGUILayout.PropertyField(m_CaretColor);
				}
				EditorGUILayout.EndFadeGroup();
				EditorGUILayout.PropertyField(m_SelectionColor);
			}
			if (GUILayout.Button("<b>CONTROL SETTINGS</b>" + (m_foldout.extraSettings ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label))
			{
				m_foldout.extraSettings = !m_foldout.extraSettings;
			}
			if (m_foldout.extraSettings)
			{
				EditorGUILayout.PropertyField(m_OnFocusSelectAll, new GUIContent("OnFocus - Select All", "Should all the text be selected when the Input Field is selected."));
				EditorGUILayout.PropertyField(m_ResetOnDeActivation, new GUIContent("Reset On DeActivation", "Should the Text and Caret position be reset when Input Field is DeActivated."));
				EditorGUILayout.PropertyField(m_RestoreOriginalTextOnEscape, new GUIContent("Restore On ESC Key", "Should the original text be restored when pressing ESC."));
				EditorGUILayout.PropertyField(m_HideMobileInput);
				EditorGUILayout.PropertyField(m_ReadOnly);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(m_RichText);
				EditorGUIUtility.labelWidth = 140f;
				EditorGUILayout.PropertyField(m_RichTextEditingAllowed, new GUIContent("Allow Rich Text Editing"));
				EditorGUIUtility.labelWidth = 130f;
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(m_OnValueChanged);
			EditorGUILayout.PropertyField(m_OnEndEdit);
			EditorGUILayout.PropertyField(m_OnSelect);
			EditorGUILayout.PropertyField(m_OnDeselect);
			EditorGUI.EndDisabledGroup();
			base.serializedObject.ApplyModifiedProperties();
		}

		public TMP_InputFieldEditor()
			
		{
		}
	}
}
