using GlobalEnums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Compilation;

namespace WeaverCore.Editor.Settings
{
	public class GeneralSettingsScreen : EditorWindow
	{
		public const int WindowWidth = 600;
		public const int WindowHeight = 300;

		public static string HollowKnightLocation => GameBuildSettings.Settings.HollowKnightLocation;

		string hkDirTemp = "";

		public static void OpenSettingsScreen()
		{
			var window = GetWindow<GeneralSettingsScreen>();
			window.hkDirTemp = HollowKnightLocation;
			window.titleContent = new GUIContent("General Settings");
			window.Show();

			var resolution = Screen.currentResolution;

			var x = (resolution.width / 2) - (WindowWidth / 2);
			var y = (resolution.height / 2) - (WindowHeight / 2);

			window.position = new Rect(x, y, WindowWidth, WindowHeight);
		}

		private void OnGUI()
		{
			EditorGUILayout.LabelField("Hollow Knight Directory");

			EditorGUILayout.BeginHorizontal();

			hkDirTemp = EditorGUILayout.TextField(hkDirTemp);
			if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
			{
				string exeLocation = EditorUtility.OpenFilePanel("Find where hollow_knight.exe is located", Environment.CurrentDirectory, "exe");
				if (exeLocation != null && exeLocation != "")
				{
					var fileInfo = new FileInfo(exeLocation);

					hkDirTemp = fileInfo.Directory.FullName;
				}
			}

			EditorGUILayout.EndHorizontal();

			GeneralSettings.Instance.CurrentGameLanguage = (SupportedLanguages)EditorGUILayout.EnumPopup(new GUIContent("Current Game Language", "The current language the game will be using in the editor. Use this to test out Language Tables"), GeneralSettings.Instance.CurrentGameLanguage);

			GeneralSettings.Instance.DisableGameFreezing = EditorGUILayout.Toggle(new GUIContent("Disable Game Freezing", "Disables the Freezing effect that occurs when taking damage or parrying an attack in the editor"), GeneralSettings.Instance.DisableGameFreezing);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Close"))
			{
				Close();
			}
			if (GUILayout.Button("Apply"))
			{
				Close();
				GameBuildSettings.Settings.HollowKnightLocation = hkDirTemp;
				GameBuildSettings.Verify(GameBuildSettings.Settings);
				GameBuildSettings.SaveSettings();
				GeneralSettings.Save();
			}

			EditorGUILayout.EndHorizontal();
		}
	}
}
