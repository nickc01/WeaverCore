using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	[CustomEditor(typeof(TMP_Settings))]
	public class TMP_SettingsEditor : Editor
	{
		private SerializedProperty prop_FontAsset;

		private SerializedProperty prop_DefaultFontAssetPath;

		private SerializedProperty prop_DefaultFontSize;

		private SerializedProperty prop_DefaultAutoSizeMinRatio;

		private SerializedProperty prop_DefaultAutoSizeMaxRatio;

		private SerializedProperty prop_DefaultTextMeshProTextContainerSize;

		private SerializedProperty prop_DefaultTextMeshProUITextContainerSize;

		private SerializedProperty prop_AutoSizeTextContainer;

		private SerializedProperty prop_SpriteAsset;

		private SerializedProperty prop_SpriteAssetPath;

		private SerializedProperty prop_EnableEmojiSupport;

		private SerializedProperty prop_StyleSheet;

		private ReorderableList m_list;

		private SerializedProperty prop_matchMaterialPreset;

		private SerializedProperty prop_WordWrapping;

		private SerializedProperty prop_Kerning;

		private SerializedProperty prop_ExtraPadding;

		private SerializedProperty prop_TintAllSprites;

		private SerializedProperty prop_ParseEscapeCharacters;

		private SerializedProperty prop_MissingGlyphCharacter;

		private SerializedProperty prop_WarningsDisabled;

		private SerializedProperty prop_LeadingCharacters;

		private SerializedProperty prop_FollowingCharacters;

		public void OnEnable()
		{
			prop_FontAsset = base.serializedObject.FindProperty("m_defaultFontAsset");
			prop_DefaultFontAssetPath = base.serializedObject.FindProperty("m_defaultFontAssetPath");
			prop_DefaultFontSize = base.serializedObject.FindProperty("m_defaultFontSize");
			prop_DefaultAutoSizeMinRatio = base.serializedObject.FindProperty("m_defaultAutoSizeMinRatio");
			prop_DefaultAutoSizeMaxRatio = base.serializedObject.FindProperty("m_defaultAutoSizeMaxRatio");
			prop_DefaultTextMeshProTextContainerSize = base.serializedObject.FindProperty("m_defaultTextMeshProTextContainerSize");
			prop_DefaultTextMeshProUITextContainerSize = base.serializedObject.FindProperty("m_defaultTextMeshProUITextContainerSize");
			prop_AutoSizeTextContainer = base.serializedObject.FindProperty("m_autoSizeTextContainer");
			prop_SpriteAsset = base.serializedObject.FindProperty("m_defaultSpriteAsset");
			prop_SpriteAssetPath = base.serializedObject.FindProperty("m_defaultSpriteAssetPath");
			prop_EnableEmojiSupport = base.serializedObject.FindProperty("m_enableEmojiSupport");
			prop_StyleSheet = base.serializedObject.FindProperty("m_defaultStyleSheet");
			m_list = new ReorderableList(base.serializedObject, base.serializedObject.FindProperty("m_fallbackFontAssets"), true, true, true, true);
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
			prop_matchMaterialPreset = base.serializedObject.FindProperty("m_matchMaterialPreset");
			prop_WordWrapping = base.serializedObject.FindProperty("m_enableWordWrapping");
			prop_Kerning = base.serializedObject.FindProperty("m_enableKerning");
			prop_ExtraPadding = base.serializedObject.FindProperty("m_enableExtraPadding");
			prop_TintAllSprites = base.serializedObject.FindProperty("m_enableTintAllSprites");
			prop_ParseEscapeCharacters = base.serializedObject.FindProperty("m_enableParseEscapeCharacters");
			prop_MissingGlyphCharacter = base.serializedObject.FindProperty("m_missingGlyphCharacter");
			prop_WarningsDisabled = base.serializedObject.FindProperty("m_warningsDisabled");
			prop_LeadingCharacters = base.serializedObject.FindProperty("m_leadingCharacters");
			prop_FollowingCharacters = base.serializedObject.FindProperty("m_followingCharacters");
			TMP_UIStyleManager.GetUIStyles();
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();
			GUILayout.Label("<b>TEXTMESH PRO - SETTINGS</b>", TMP_UIStyleManager.Section_Label);
			EditorGUI.indentLevel = 0;
			EditorGUIUtility.labelWidth = 135f;
			EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
			GUILayout.Label("<b>Default Font Asset</b>", TMP_UIStyleManager.Label);
			GUILayout.Label("Select the Font Asset that will be assigned by default to newly created text objects when no Font Asset is specified.", TMP_UIStyleManager.Label);
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(prop_FontAsset);
			GUILayout.Space(10f);
			GUILayout.Label("The relative path to a Resources folder where the Font Assets and Material Presets are located.\nExample \"Fonts & Materials/\"", TMP_UIStyleManager.Label);
			EditorGUILayout.PropertyField(prop_DefaultFontAssetPath, new GUIContent("Path:        Resources/"));
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
			GUILayout.Label("<b>Fallback Font Assets</b>", TMP_UIStyleManager.Label);
			GUILayout.Label("Select the Font Assets that will be searched to locate and replace missing characters from a given Font Asset.", TMP_UIStyleManager.Label);
			GUILayout.Space(5f);
			m_list.DoLayoutList();
			GUILayout.Label("<b>Fallback Material Settings</b>", TMP_UIStyleManager.Label);
			EditorGUILayout.PropertyField(prop_matchMaterialPreset, new GUIContent("Match Material Presets"));
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
			GUILayout.Label("<b>New Text Object Default Settings</b>", TMP_UIStyleManager.Label);
			GUILayout.Label("Default settings used by all new text objects.", TMP_UIStyleManager.Label);
			GUILayout.Space(10f);
			EditorGUI.BeginChangeCheck();
			GUILayout.Label("<b>Text Container Default Settings</b>", TMP_UIStyleManager.Label);
			EditorGUIUtility.labelWidth = 150f;
			EditorGUILayout.PropertyField(prop_DefaultTextMeshProTextContainerSize, new GUIContent("TextMeshPro"));
			EditorGUILayout.PropertyField(prop_DefaultTextMeshProUITextContainerSize, new GUIContent("TextMeshPro UI"));
			EditorGUILayout.PropertyField(prop_AutoSizeTextContainer, new GUIContent("Auto Size Text Container", "Set the size of the text container to match the text."));
			GUILayout.Space(10f);
			GUILayout.Label("<b>Text Component Default Settings</b>", TMP_UIStyleManager.Label);
			EditorGUIUtility.labelWidth = 150f;
			EditorGUILayout.PropertyField(prop_DefaultFontSize, new GUIContent("Default Font Size"), GUILayout.MinWidth(200f), GUILayout.MaxWidth(200f));
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(new GUIContent("Text Auto Size Ratios"));
			EditorGUIUtility.labelWidth = 35f;
			EditorGUILayout.PropertyField(prop_DefaultAutoSizeMinRatio, new GUIContent("Min:"));
			EditorGUILayout.PropertyField(prop_DefaultAutoSizeMaxRatio, new GUIContent("Max:"));
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = 150f;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(prop_WordWrapping);
			EditorGUILayout.PropertyField(prop_Kerning);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(prop_ExtraPadding);
			EditorGUILayout.PropertyField(prop_TintAllSprites);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(prop_ParseEscapeCharacters, new GUIContent("Parse Escape Sequence"));
			EditorGUIUtility.fieldWidth = 10f;
			EditorGUILayout.PropertyField(prop_MissingGlyphCharacter, new GUIContent("Missing Glyph Repl."));
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = 135f;
			GUILayout.Space(10f);
			GUILayout.Label("<b>Disable warnings for missing glyphs on text objects.</b>", TMP_UIStyleManager.Label);
			EditorGUILayout.PropertyField(prop_WarningsDisabled, new GUIContent("Disable warnings"));
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
			GUILayout.Label("<b>Default Sprite Asset</b>", TMP_UIStyleManager.Label);
			GUILayout.Label("Select the Sprite Asset that will be assigned by default when using the <sprite> tag when no Sprite Asset is specified.", TMP_UIStyleManager.Label);
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(prop_SpriteAsset);
			GUILayout.Space(10f);
			EditorGUILayout.PropertyField(prop_EnableEmojiSupport, new GUIContent("Enable Emoji Support", "Enables Emoji support for Touch Screen Keyboards on target devices."));
			GUILayout.Space(10f);
			GUILayout.Label("The relative path to a Resources folder where the Sprite Assets are located.\nExample \"Sprite Assets/\"", TMP_UIStyleManager.Label);
			EditorGUILayout.PropertyField(prop_SpriteAssetPath, new GUIContent("Path:        Resources/"));
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
			GUILayout.Label("<b>Default Style Sheet</b>", TMP_UIStyleManager.Label);
			GUILayout.Label("Select the Style Sheet that will be used for all text objects in this project.", TMP_UIStyleManager.Label);
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(prop_StyleSheet);
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
			GUILayout.Label("<b>Line Breaking Resources for Asian languages</b>", TMP_UIStyleManager.Label);
			GUILayout.Label("Select the text assets that contain the Leading and Following characters which define the rules for line breaking with Asian languages.", TMP_UIStyleManager.Label);
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(prop_LeadingCharacters);
			EditorGUILayout.PropertyField(prop_FollowingCharacters);
			EditorGUILayout.EndVertical();
			if (base.serializedObject.ApplyModifiedProperties())
			{
				EditorUtility.SetDirty(base.target);
				TMPro_EventManager.ON_TMP_SETTINGS_CHANGED();
			}
		}
	}
}
