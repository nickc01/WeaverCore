using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Editor.Compilation;
using WeaverCore.Implementations;
using WeaverCore.Settings;

namespace WeaverCore.Editor.Implementations
{
	public class E_SaveSpecificSettings_I : SaveSpecificSettings_I
	{
		[OnInit]
		static void Init()
		{
			EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
		}

		private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
		{
			if (obj == PlayModeStateChange.ExitingPlayMode)
			{
				//Debug.Log("ON GAME CLOSE");
				SaveSpecificSettings.SaveAllSettings();
			}
		}

		[OnRuntimeInit]
		static void OnGameStart()
		{
			//Debug.Log("ON GAME START");
			SaveSpecificSettings.LoadSaveSlot(1);
		}

		public override int CurrentSaveSlot => 1;

		public override void LoadSettings(SaveSpecificSettings settings)
		{
			/*var settingType = settings.GetType();
			if (PersistentData.TryGetData<string>(settingType.FullName + "_SAVEDATA", out var json))
			{
				JsonUtility.FromJsonOverwrite(json, settings);
			}*/
		}

		public override void SaveSettings(SaveSpecificSettings settings)
		{
			/*var settingType = settings.GetType();
			PersistentData.StoreData(JsonUtility.ToJson(settings,true), settingType.FullName + "_SAVEDATA");
			PersistentData.SaveData();*/
		}
	}
}
