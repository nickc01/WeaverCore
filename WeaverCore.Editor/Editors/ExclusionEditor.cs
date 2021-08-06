using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Used mainly for excluding certain properties from being displayed
/// </summary>
public abstract class ExclusionEditor : Editor
{
	public abstract IEnumerable<string> PropertiesToExclude();

	public override void OnInspectorGUI()
	{
		bool result;
		using (new LocalizationGroup(target))
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.UpdateIfRequiredOrScript();
			SerializedProperty iterator = serializedObject.GetIterator();
			bool enterChildren = true;
			while (iterator.NextVisible(enterChildren))
			{
				using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
				{
					if (!PropertiesToExclude().Any(p => p == iterator.name))
					{
						EditorGUILayout.PropertyField(iterator, true, new GUILayoutOption[0]);
					}
				}
				enterChildren = false;
			}
			serializedObject.ApplyModifiedProperties();
			result = EditorGUI.EndChangeCheck();
		}
	}
}
