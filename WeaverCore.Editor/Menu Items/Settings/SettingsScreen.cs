using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Utilities;
using WeaverCore.Interfaces;

namespace WeaverCore.Editor
{
	public class SettingsScreen : ConfigurationScreen<Settings>
	{
		[MenuItem("WeaverCore/Settings")]
		public static void OpenSettings()
		{
			Open();
		}

		protected override void OnGUI()
		{
			EditorGUILayout.LabelField("Hollow Knight Directory");

			EditorGUILayout.BeginHorizontal();

			StoredSettings.HollowKnightDirectory = EditorGUILayout.TextField(StoredSettings.HollowKnightDirectory);
			if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
			{
				string exeLocation = EditorUtility.OpenFilePanel("Find where hollow_knight.exe is located", Environment.CurrentDirectory, "exe");
				if (exeLocation != null && exeLocation != "")
				{
					var fileInfo = new FileInfo(exeLocation);

					StoredSettings.HollowKnightDirectory = fileInfo.Directory.FullName;
				}
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Close"))
			{
				Close();
			}
			if (GUILayout.Button("Apply"))
			{
				Done();
			}

			EditorGUILayout.EndHorizontal();
		}
	}

	[Serializable]
	public class Settings : ConfigSettings
	{
		public string HollowKnightDirectory = null;

		public override void GetStoredSettings()
		{
			base.GetStoredSettings();
			if (HollowKnightDirectory == null)
			{
				HollowKnightDirectory = GameBuildSettings.GetSettings().HollowKnightLocation;
			}
			HollowKnightDirectory = new DirectoryInfo(HollowKnightDirectory).FullName;
		}

		public override void SetStoredSettings()
		{
			base.SetStoredSettings();
			var gameSettings = GameBuildSettings.GetSettings();
			gameSettings.HollowKnightLocation = HollowKnightDirectory;
			GameBuildSettings.SetSettings(gameSettings);
			GameBuildSettings.VerifySettings();
		}
	}
}
