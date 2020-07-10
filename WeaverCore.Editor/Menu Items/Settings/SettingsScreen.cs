using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Editor
{
	public class SettingsScreen : EditorWindow
	{
		[MenuItem("WeaverCore/Settings")]
		public static void OpenSettings()
		{
			var settings = GetWindow<SettingsScreen>("Settings");

			settings.Show();
		}

		Settings CurrentSettings;

		void Awake()
		{
			CurrentSettings = Settings.GetSettings();
		}

		void OnGUI()
		{
			EditorGUILayout.LabelField("Hollow Knight Directory");

			EditorGUILayout.BeginHorizontal();

			CurrentSettings.HollowKnightDirectory = EditorGUILayout.TextField(CurrentSettings.HollowKnightDirectory);
			if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
			{
				string exeLocation = EditorUtility.OpenFilePanel("Find where hollow_knight.exe is located", Environment.CurrentDirectory, "exe");
				if (exeLocation != null && exeLocation != "")
				{
					var fileInfo = new FileInfo(exeLocation);

					CurrentSettings.HollowKnightDirectory = fileInfo.Directory.FullName;
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
				//RetrievedBuildSettings = CurrentSettings;
				//Finish();
				Settings.SetSettings(CurrentSettings);
				Close();
			}

			EditorGUILayout.EndHorizontal();
		}

		void OnDestroy()
		{

		}
	}

	[Serializable]
	public struct Settings
	{
		static string _settingsFileLocation = null;
		public static string SettingsFileLocation
		{
			get
			{
				if (_settingsFileLocation == null)
				{
					var file = new FileInfo("Assets\\WeaverCore\\Hidden~\\Settings.txt");
					if (!file.Directory.Exists)
					{
						file.Directory.Create();
					}
					_settingsFileLocation = file.FullName;
				}
				return _settingsFileLocation;
			}
			set
			{
				_settingsFileLocation = value;
			}
		}


		public string HollowKnightDirectory;

		static StreamReader GetReader()
		{
			return new StreamReader(File.OpenRead(SettingsFileLocation));
		}

		static StreamWriter GetWriter()
		{
			if (!File.Exists(SettingsFileLocation))
			{
				return new StreamWriter(File.Create(SettingsFileLocation));
			}
			else
			{
				return new StreamWriter(File.OpenWrite(SettingsFileLocation));
			}
		}

		public static Settings GetSettings()
		{
			var settingsFile = new FileInfo(SettingsFileLocation);
			if (settingsFile.Exists)
			{
				using (var reader = GetReader())
				{
					return JsonUtility.FromJson<Settings>(reader.ReadToEnd());
				}
			}
			else
			{
				return new Settings()
				{
					HollowKnightDirectory = GameBuildSettings.GetSettings().HollowKnightLocation
				};
			}
		}

		public static void SetSettings(Settings settings)
		{
			var settingsFile = new FileInfo(SettingsFileLocation);
			using (var writer = GetWriter())
			{
				writer.Write(JsonUtility.ToJson(settings));
				var gameSettings = GameBuildSettings.GetSettings();
				gameSettings.HollowKnightLocation = settings.HollowKnightDirectory;
				GameBuildSettings.SetSettings(gameSettings);
				GameBuildSettings.VerifySettings();
			}
		}

		/*static void VerifyHKDirectory()
		{
			using (var reader = ReadSettingsFile())
			{
				reader.ReadLine();
				var hollowKnightDir = reader.ReadLine();
				var directory = new DirectoryInfo(hollowKnightDir);
				//Debug.Log("Sub Directory = " + directory.FullName + "\\hollow_knight_data");
				if (!directory.Exists || !Directory.Exists(directory.FullName + "\\hollow_knight_data"))
				{
					WeaverLog.LogError("The Hollow Knight directory is currently not configured. Please specify it in the WeaverCore/Settings Menu");
				}
				while (reader.ReadLine() == "")
				{

				}
				var unityEditorLocation = reader.ReadLine();
				//Debug.Log("Unity Editor Location = " + unityEditorLocation);
				//Debug.Log("Hollow Knight Directory = " + hollowKnightDir);

				reader.Close();

				var unityEditorTrueLocation = new FileInfo(typeof(EditorWindow).Assembly.Location).Directory.FullName;
				//Debug.Log("True Location = " + unityEditorTrueLocation);

				if (unityEditorLocation != unityEditorTrueLocation)
				{
					using (var fileStream = File.CreateText(new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\Settings.txt").FullName))
					{
						fileStream.WriteLine("[Hollow Knight Directory]");
						fileStream.WriteLine(hollowKnightDir);
						fileStream.WriteLine("");
						fileStream.WriteLine("[Unity Editor Location]");
						fileStream.WriteLine(unityEditorTrueLocation);
						fileStream.Close();
					}
				}
			}
		}*/

		/*static StreamReader ReadSettingsFile()
		{
			var file = new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\Settings.txt");
			//using (var fileStream = File.OpenRead(file.FullName))
			//{
				return new StreamReader(File.OpenRead(file.FullName));
			//}
		}

		public static Settings GetSettings()
		{
			//var file = new FileInfo("Assets\\WeaverCore\\OtherProjects~\\WeaverCore.Game\\Settings.txt");
			//using (var fileStream = ReadSettingsFile())
			//{
			using (var reader = ReadSettingsFile())
			{

				reader.ReadLine();
				var hollowKnightDir = reader.ReadLine();
				var settings = new Settings();
				settings.HollowKnightDirectory = hollowKnightDir;



				//WeaverLog.Log("Hollow Knight Directory = " + hollowKnightDir);
				return settings;
			}
			//}
		}

		public static void SetSettings(Settings settings)
		{
			//TODO TODO TODO
		}*/
	}

}
