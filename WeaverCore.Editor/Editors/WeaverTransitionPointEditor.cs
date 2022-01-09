using UnityEditor;
using UnityEngine;
using WeaverCore.Components;

[CustomEditor(typeof(WeaverTransitionPoint))]
public class WeaverTransitionPointEditor : Editor
{
	public override void OnInspectorGUI()
	{
		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true);
		}
		EditorGUILayout.PropertyField(serializedObject.FindProperty("gateType"), true);

		var iterator = serializedObject.GetIterator();
		bool enterChildren = true;

		while (iterator.NextVisible(enterChildren))
		{
			enterChildren = false;
			if (iterator.name == "gateType")
			{
				continue;
			}
			if (iterator.name == "m_Script")
			{
				continue;
			}
			if (iterator.name == "hardLandOnExit" && serializedObject.FindProperty("gateType").enumValueIndex != 0)
			{
				continue;
			}

			if (iterator.name == "respawnMarker")
			{
				if (serializedObject.FindProperty("nonHazardGate").boolValue == false)
				{
					EditorGUILayout.PropertyField(iterator, true);
					var value = iterator.objectReferenceValue;
					if (value == null)
					{
						GUIStyle s = new GUIStyle(EditorStyles.textField);
						s.normal.textColor = Color.red;
						EditorGUILayout.LabelField("A respawn marker must be configured", s);
					}
				}
				continue;
			}

			if (iterator.name == "AtmosSnapshot" && serializedObject.FindProperty("enableAtmosSnapshot").boolValue == false)
			{
				continue;
			}

			if (iterator.name == "EnviroSnapshot" && serializedObject.FindProperty("enableEnviroSnapshot").boolValue == false)
			{
				continue;
			}

			if (iterator.name == "ActorSnapshot" && serializedObject.FindProperty("enableActorSnapshot").boolValue == false)
			{
				continue;
			}

			if (iterator.name == "MusicSnapshot" && serializedObject.FindProperty("enableMusicSnapshot").boolValue == false)
			{
				continue;
			}

			EditorGUILayout.PropertyField(iterator, true);
		}

		serializedObject.ApplyModifiedProperties();
	}
}
