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
				SaveSpecificSettings.SaveAllSettings();
			}
		}

		[OnRuntimeInit]
		static void OnGameStart()
		{
			SaveSpecificSettings.LoadSaveSlot(1);
		}

		public override int CurrentSaveSlot => 1;

		public override void LoadSettings(SaveSpecificSettings settings)
		{

		}

		public override void SaveSettings(SaveSpecificSettings settings)
		{

		}
	}
}
