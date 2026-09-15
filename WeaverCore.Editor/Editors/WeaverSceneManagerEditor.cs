using UnityEditor;
using WeaverCore.Components;
using WeaverCore.Utilities;

[CustomEditor(typeof(WeaverSceneManager))]
public class WeaverSceneManagerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		serializedObject.UpdateIfRequiredOrScript();

		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true);
		}
		var iterator = serializedObject.GetIterator();
		bool enterChildren = true;

		while (iterator.NextVisible(enterChildren))
		{
			enterChildren = false;
			if (iterator.name == "m_Script")
			{
				continue;
			}
			if (iterator.name == "sceneDimensions" && serializedObject.FindProperty("autoSetDimensions").boolValue)
			{
				continue;
			}

			if (iterator.name == "infectedMusic" && serializedObject.FindProperty("canBeInfected").boolValue == false)
			{
				continue;
			}

			if (iterator.name == "musicSnapshot" && serializedObject.FindProperty("customMusicSnapshot").boolValue == false)
			{
				continue;
			}

			if (iterator.name == "environmentType")
			{
				EditorGUI.BeginChangeCheck();
				var newValue = EnvironmentTypeEditorUtilities.DrawEnvironmentTypePopup("Environment Type", iterator.intValue);
				if (EditorGUI.EndChangeCheck())
				{
					iterator.intValue = newValue;
				}
				continue;
			}
			EditorGUILayout.PropertyField(iterator, true);
		}

		serializedObject.ApplyModifiedProperties();
	}
}

internal static class EnvironmentTypeEditorUtilities
{
	public static int DrawEnvironmentTypePopup(string label, int currentValue)
	{
		EnvironmentTypeUtilities.GetAllEnvironmentTypes(out var environmentIndexes, out var environmentNames);

		bool foundCurrentValue = false;
		for (int i = 0; i < environmentIndexes.Length; i++)
		{
			if (environmentIndexes[i] == currentValue)
			{
				foundCurrentValue = true;
				break;
			}
		}

		if (!foundCurrentValue)
		{
			var newIndexes = new int[environmentIndexes.Length + 1];
			var newNames = new string[environmentNames.Length + 1];

			for (int i = 0; i < environmentIndexes.Length; i++)
			{
				newIndexes[i] = environmentIndexes[i];
				newNames[i] = environmentNames[i];
			}

			newIndexes[newIndexes.Length - 1] = currentValue;
			newNames[newNames.Length - 1] = $"Environment {currentValue}";

			environmentIndexes = newIndexes;
			environmentNames = newNames;
		}

		return EditorGUILayout.IntPopup(label, currentValue, environmentNames, environmentIndexes);
	}
}
