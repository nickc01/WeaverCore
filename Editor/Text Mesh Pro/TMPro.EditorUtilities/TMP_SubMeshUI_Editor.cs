using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(TMP_SubMeshUI))]
	public class TMP_SubMeshUI_Editor : Editor
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct m_foldout
		{
			public static bool fontSettings = true;
		}

		private static string[] uiStateLabel = new string[2]
		{
			"\t- <i>Click to expand</i> -",
			"\t- <i>Click to collapse</i> -"
		};

		private SerializedProperty fontAsset_prop;

		private SerializedProperty spriteAsset_prop;

		private TMP_SubMeshUI m_SubMeshComponent;

		private CanvasRenderer m_canvasRenderer;

		private Editor m_materialEditor;

		private Material m_targetMaterial;

		public void OnEnable()
		{
			TMP_UIStyleManager.GetUIStyles();
			fontAsset_prop = base.serializedObject.FindProperty("m_fontAsset");
			spriteAsset_prop = base.serializedObject.FindProperty("m_spriteAsset");
			m_SubMeshComponent = (base.target as TMP_SubMeshUI);
			m_canvasRenderer = m_SubMeshComponent.canvasRenderer;
			if (m_canvasRenderer != null && m_canvasRenderer.GetMaterial() != null)
			{
				m_materialEditor = Editor.CreateEditor(m_canvasRenderer.GetMaterial());
				m_targetMaterial = m_canvasRenderer.GetMaterial();
			}
		}

		public void OnDisable()
		{
			if (m_materialEditor != null)
			{
				Object.DestroyImmediate(m_materialEditor);
			}
		}

		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("<b>SUB OBJECT SETTINGS</b>" + (m_foldout.fontSettings ? uiStateLabel[1] : uiStateLabel[0]), TMP_UIStyleManager.Section_Label))
			{
				m_foldout.fontSettings = !m_foldout.fontSettings;
			}
			if (m_foldout.fontSettings)
			{
				GUI.enabled = false;
				EditorGUILayout.PropertyField(fontAsset_prop);
				EditorGUILayout.PropertyField(spriteAsset_prop);
				GUI.enabled = true;
			}
			EditorGUILayout.Space();
			if (m_canvasRenderer != null && m_canvasRenderer.GetMaterial() != null)
			{
				Material material = m_canvasRenderer.GetMaterial();
				if (material != m_targetMaterial)
				{
					m_targetMaterial = material;
					Object.DestroyImmediate(m_materialEditor);
				}
				if (m_materialEditor == null)
				{
					m_materialEditor = Editor.CreateEditor(material);
				}
				m_materialEditor.DrawHeader();
				m_materialEditor.OnInspectorGUI();
			}
		}
	}
}
