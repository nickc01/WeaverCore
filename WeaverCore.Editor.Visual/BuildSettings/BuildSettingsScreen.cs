using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using WeaverCore.Interfaces;
//using WeaverCore.Editor.Helpers;

namespace WeaverCore.Editor.Visual
{
	internal class BuildSettingsScreen : EditorWindow
	{
		public static BuildSettings RetrievedBuildSettings { get; private set; }


		static bool Open = false;

		BuildSettings CurrentSettings;

		static bool Done = false;

		static bool Awaiter()
		{
			if (Done == false)
			{
				return false;
			}
			else
			{
				Done = false;
				return true;
			}
		}

		/// <summary>
		/// Retrieves the build settings the player specified. The results will be stored in <see cref="RetrieveBuildSettings"/>
		/// </summary>
		public static IUAwaiter RetrieveBuildSettings()
		{
			if (Open)
			{
				throw new Exception("The Build Settings Window is already open");
			}
			BuildSettingsScreen window = (BuildSettingsScreen)GetWindow(typeof(BuildSettingsScreen));
			Done = false;
			Open = true;
			window.Setup();
			window.Show();
			return new Awaiters.WaitTillTrue(Awaiter);
		}

		void Setup()
		{
			RetrievedBuildSettings = null;

			var resolution = Screen.currentResolution;
			var width = position.width;
			var height = position.height;

			var x = (resolution.width / 2) - (width / 2);
			var y = (resolution.height / 2) - (height / 2);

			position = new Rect(x, y, width, height);
			titleContent.text = "Compile";

			CurrentSettings = BuildSettings.GetStoredSettings();
		}

		void OnGUI()
		{
			EditorGUILayout.LabelField("Mod Name");
			CurrentSettings.ModName = EditorGUILayout.TextField(CurrentSettings.ModName);
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Windows Support");
			CurrentSettings.WindowsSupport = EditorGUILayout.Toggle(CurrentSettings.WindowsSupport);

			EditorGUILayout.LabelField("Mac Support");
			CurrentSettings.MacSupport = EditorGUILayout.Toggle(CurrentSettings.MacSupport);

			EditorGUILayout.LabelField("Linux Support");
			CurrentSettings.LinuxSupport = EditorGUILayout.Toggle(CurrentSettings.LinuxSupport);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Hollow Knight Directory");

			EditorGUILayout.BeginHorizontal();

			CurrentSettings.HollowKnightDirectory = EditorGUILayout.TextField(CurrentSettings.HollowKnightDirectory);
			if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
			{
				string exeLocation = EditorUtility.OpenFilePanel("Find where hollow_knight.exe is located", Path.GetTempPath(), "exe");
				if (exeLocation != null && exeLocation != "")
				{
					var fileInfo = new FileInfo(exeLocation);

					CurrentSettings.HollowKnightDirectory = fileInfo.Directory.FullName;
				}
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Auto-Start Game");
			CurrentSettings.StartGame = EditorGUILayout.Toggle(CurrentSettings.StartGame);

			CurrentSettings.Check();


			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Close"))
			{
				Finish();
			}
			if (GUILayout.Button("Compile") && Open)
			{
				RetrievedBuildSettings = CurrentSettings;
				Finish();
			}

			EditorGUILayout.EndHorizontal();
		}

		void OnDestroy()
		{
			Finish(false);
		}

		void Finish(bool close = true)
		{
			if (Open)
			{
				Done = true;
				if (close)
				{
					Close();
				}
				Open = false;
				BuildSettings.SetStoredSettings(CurrentSettings);
			}
		}
	}
}
