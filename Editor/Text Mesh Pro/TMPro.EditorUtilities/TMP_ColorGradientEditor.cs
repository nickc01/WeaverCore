using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	[CustomEditor(typeof(TMP_ColorGradient))]
	public class TMP_ColorGradientEditor : Editor
	{
		private SerializedProperty topLeftColor;

		private SerializedProperty topRightColor;

		private SerializedProperty bottomLeftColor;

		private SerializedProperty bottomRightColor;

		private void OnEnable()
		{
			TMP_UIStyleManager.GetUIStyles();
			topLeftColor = base.serializedObject.FindProperty("topLeft");
			topRightColor = base.serializedObject.FindProperty("topRight");
			bottomLeftColor = base.serializedObject.FindProperty("bottomLeft");
			bottomRightColor = base.serializedObject.FindProperty("bottomRight");
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();
			GUILayout.Label("<b>TextMeshPro - Color Gradient Preset</b>", TMP_UIStyleManager.Section_Label);
			EditorGUILayout.BeginVertical(TMP_UIStyleManager.SquareAreaBox85G);
			EditorGUILayout.PropertyField(topLeftColor, new GUIContent("Top Left"));
			EditorGUILayout.PropertyField(topRightColor, new GUIContent("Top Right"));
			EditorGUILayout.PropertyField(bottomLeftColor, new GUIContent("Bottom Left"));
			EditorGUILayout.PropertyField(bottomRightColor, new GUIContent("Bottom Right"));
			EditorGUILayout.EndVertical();
			if (base.serializedObject.ApplyModifiedProperties())
			{
				TMPro_EventManager.ON_COLOR_GRAIDENT_PROPERTY_CHANGED(base.target as TMP_ColorGradient);
			}
		}
	}
}
