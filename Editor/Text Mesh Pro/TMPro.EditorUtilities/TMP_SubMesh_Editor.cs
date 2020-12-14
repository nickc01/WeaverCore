using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	[CustomEditor(typeof(TMP_SubMesh))]
	[CanEditMultipleObjects]
	public class TMP_SubMesh_Editor : Editor
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

		private TMP_SubMesh m_SubMeshComponent;

		private Renderer m_Renderer;

		public void OnEnable()
		{
			TMP_UIStyleManager.GetUIStyles();
			fontAsset_prop = base.serializedObject.FindProperty("m_fontAsset");
			spriteAsset_prop = base.serializedObject.FindProperty("m_spriteAsset");
			m_SubMeshComponent = (base.target as TMP_SubMesh);
			m_Renderer = m_SubMeshComponent.renderer;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.indentLevel = 0;
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
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Sorting Layer");
			EditorGUI.BeginChangeCheck();
			string[] sortingLayerNames = SortingLayerHelper.sortingLayerNames;
			string sortingLayerNameFromID = SortingLayerHelper.GetSortingLayerNameFromID(m_Renderer.sortingLayerID);
			int num = Array.IndexOf(sortingLayerNames, sortingLayerNameFromID);
			EditorGUIUtility.fieldWidth = 0f;
			int num2 = EditorGUILayout.Popup(string.Empty, num, sortingLayerNames, GUILayout.MinWidth(80f));
			if (num2 != num)
			{
				m_Renderer.sortingLayerID = SortingLayerHelper.GetSortingLayerIDForIndex(num2);
			}
			EditorGUIUtility.labelWidth = 40f;
			EditorGUIUtility.fieldWidth = 80f;
			int num3 = EditorGUILayout.IntField("Order", m_Renderer.sortingOrder);
			if (num3 != m_Renderer.sortingOrder)
			{
				m_Renderer.sortingOrder = num3;
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}
