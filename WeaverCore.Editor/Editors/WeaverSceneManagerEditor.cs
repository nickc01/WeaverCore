using UnityEditor;
using WeaverCore.Components;

[CustomEditor(typeof(WeaverSceneManager))]
public class WeaverSceneManagerEditor : Editor
{
	//Set environment type on scene entry. 0 = Dust, 1 = Grass, 2 = Bone, 3 = Spa, 4 = Metal, 5 = No Effect, 6 = Wet
	enum EnvironmentType
	{
		Dust = 0,
		Grass = 1,
		Bone = 2,
		Spa = 3,
		Metal = 4,
		NoEffect = 5,
		Wet = 6,
	}


	public override void OnInspectorGUI()
	{
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
				iterator.intValue = (int)(EnvironmentType)EditorGUILayout.EnumPopup("Environment Type", (EnvironmentType)iterator.intValue);
				continue;
			}
			EditorGUILayout.PropertyField(iterator, true);
		}

		serializedObject.ApplyModifiedProperties();
	}
}

