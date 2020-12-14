using UnityEditor;
using UnityEditor.UI;

namespace TMPro.EditorUtilities
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(TMP_Dropdown), true)]
	public class DropdownEditor : SelectableEditor
	{
		private SerializedProperty m_Template;

		private SerializedProperty m_CaptionText;

		private SerializedProperty m_CaptionImage;

		private SerializedProperty m_ItemText;

		private SerializedProperty m_ItemImage;

		private SerializedProperty m_OnSelectionChanged;

		private SerializedProperty m_Value;

		private SerializedProperty m_Options;

		protected override void OnEnable()
		{
			//((SelectableEditor)this).OnEnable();
			base.OnEnable();
			m_Template = base.serializedObject.FindProperty("m_Template");
			m_CaptionText = base.serializedObject.FindProperty("m_CaptionText");
			m_CaptionImage = base.serializedObject.FindProperty("m_CaptionImage");
			m_ItemText = base.serializedObject.FindProperty("m_ItemText");
			m_ItemImage = base.serializedObject.FindProperty("m_ItemImage");
			m_OnSelectionChanged = base.serializedObject.FindProperty("m_OnValueChanged");
			m_Value = base.serializedObject.FindProperty("m_Value");
			m_Options = base.serializedObject.FindProperty("m_Options");
		}

		public override void OnInspectorGUI()
		{
			//((SelectableEditor)this).OnInspectorGUI();
			base.OnInspectorGUI();
			EditorGUILayout.Space();
			base.serializedObject.Update();
			EditorGUILayout.PropertyField(m_Template);
			EditorGUILayout.PropertyField(m_CaptionText);
			EditorGUILayout.PropertyField(m_CaptionImage);
			EditorGUILayout.PropertyField(m_ItemText);
			EditorGUILayout.PropertyField(m_ItemImage);
			EditorGUILayout.PropertyField(m_Value);
			EditorGUILayout.PropertyField(m_Options);
			EditorGUILayout.PropertyField(m_OnSelectionChanged);
			base.serializedObject.ApplyModifiedProperties();
		}

		public DropdownEditor()
			
		{
		}
	}
}
