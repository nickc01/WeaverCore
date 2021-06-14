using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;

namespace WeaverCore.Editor.Settings
{
	public sealed class GameBuildSettings
	{
		public const string GameBuildSettingsLocation = "Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\Settings.txt";

		public string HollowKnightLocation;
		public string UnityEditorLocation;


		static GameBuildSettings _settings;
		public static GameBuildSettings Settings
		{
			get
			{
				if (_settings == null)
				{
					_settings = LoadSettings();
					Verify(_settings);
				}
				return _settings;
			}
		}

		static void Verify(GameBuildSettings settings)
		{
			var hkDirectory = new DirectoryInfo(settings.HollowKnightLocation);
			var ueDirectory = new DirectoryInfo(settings.UnityEditorLocation);

			if (!hkDirectory.Exists || !Directory.Exists(hkDirectory.FullName + "\\hollow_knight_Data"))
			{
				Debug.LogError("The Hollow Knight directory is not configured correctly. Be sure to configure this in WeaverCore/Settings/General Settings");
			}

			var currentUEDirectory = new FileInfo(typeof(EditorWindow).Assembly.Location).Directory;

			if (ueDirectory.FullName != currentUEDirectory.FullName)
			{
				settings.UnityEditorLocation = currentUEDirectory.FullName;
				SaveSettings(settings);
			}
		}

		static StreamReader GetReader()
		{
			return new StreamReader(File.OpenRead(GameBuildSettingsLocation));
		}

		static StreamWriter GetWriter()
		{
			return new StreamWriter(File.OpenWrite(GameBuildSettingsLocation));
		}

		static GameBuildSettings LoadSettings()
		{
			using (var reader = GetReader())
			{
				var settings = new GameBuildSettings();
				reader.ReadLine();
				settings.HollowKnightLocation = reader.ReadLine();
				reader.ReadLine();
				reader.ReadLine();
				settings.UnityEditorLocation = reader.ReadLine();
				return settings;
			}
		}

		public static void SaveSettings()
		{
			SaveSettings(_settings);
		}

		static void SaveSettings(GameBuildSettings settings)
		{
			using (var writer = GetWriter())
			{
				writer.WriteLine("[Hollow Knight Location]");
				writer.WriteLine(settings.HollowKnightLocation);
				writer.WriteLine("");
				writer.WriteLine("[Unity Editor Location]");
				writer.WriteLine(settings.UnityEditorLocation);
			}
		}
	}

	/*internal class GameBuildSettingsOLD
	{

		//static string fileLocation = null;

		public static string BuildSettingsFileLocation
		{
			get
			{
				return new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\Settings.txt").FullName;
			}
		}


		public string HollowKnightLocation;
		public string UnityEditorLocation;

		static StreamReader GetReader()
		{
			return new StreamReader(File.OpenRead(BuildSettingsFileLocation));
		}

		static StreamWriter GetWriter()
		{
			return new StreamWriter(File.OpenWrite(BuildSettingsFileLocation));
		}

		public static GameBuildSettings GetSettings()
		{
			using (var reader = GetReader())
			{
				var settings = new GameBuildSettings();
				reader.ReadLine();
				settings.HollowKnightLocation = reader.ReadLine();
				reader.ReadLine();
				reader.ReadLine();
				settings.UnityEditorLocation = reader.ReadLine();
				return settings;
			}
		}

		public static void SetSettings(GameBuildSettings settings)
		{
			using (var writer = GetWriter())
			{
				writer.WriteLine("[Hollow Knight Location]");
				writer.WriteLine(settings.HollowKnightLocation);
				writer.WriteLine("");
				writer.WriteLine("[Unity Editor Location]");
				writer.WriteLine(settings.UnityEditorLocation);
			}
		}

		[OnInit]
		public static void VerifySettings()
		{
			var settings = GetSettings();
			var hkDirectory = new DirectoryInfo(settings.HollowKnightLocation);
			var ueDirectory = new DirectoryInfo(settings.UnityEditorLocation);

			if (!hkDirectory.Exists || !Directory.Exists(hkDirectory.FullName + "\\hollow_knight_Data"))
			{
				Debug.LogError("The Hollow Knight directory is not configured correctly. Be sure to configure this in WeaverCore/Settings");
			}

			var currentUEDirectory = new FileInfo(typeof(EditorWindow).Assembly.Location).Directory;

			if (ueDirectory.FullName != currentUEDirectory.FullName)
			{
				settings.UnityEditorLocation = currentUEDirectory.FullName;
				SetSettings(settings);
			}
		}
	}*/
}
