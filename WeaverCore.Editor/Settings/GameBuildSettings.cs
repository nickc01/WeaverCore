using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Editor.Compilation;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Settings
{
	/// <summary>
	/// Stores general build settings that are shared between WeaverCore and WeaverCore.Game
	/// </summary>
	public sealed class GameBuildSettings
	{
		/// <summary>
		/// The location where the build settings are stored
		/// </summary>
		public static string GameBuildSettingsLocation = BuildTools.WeaverCoreFolder.AddSlash() + $"Other Projects~{Path.DirectorySeparatorChar}WeaverCore.Game{Path.DirectorySeparatorChar}Settings.txt";

		/// <summary>
		/// The currently configured folder location of Hollow Knight.exe
		/// </summary>
		public string HollowKnightLocation;

		/// <summary>
		/// The location of where the Unity Editor assemblies are located
		/// </summary>
		public string UnityEditorLocation;

		/// <summary>
		/// Gets the location of the "Mods" folder
		/// </summary>
		public string ModsLocation
		{
			get
			{
				if (!Directory.Exists(HollowKnightLocation))
				{
					return null;
				}
				var path = PathUtilities.AddSlash(HollowKnightLocation);
				if (SystemInfo.operatingSystem.Contains("Windows"))
				{
					path += "hollow_knight_Data\\Managed\\Mods";
				}
				else if (SystemInfo.operatingSystem.Contains("Mac"))
				{
					path += "Contents/Resources/Data/Managed/Mods/";
				}
				else if (SystemInfo.operatingSystem.Contains("Linux"))
				{
					path += "hollow_knight_Data/Managed/Mods";
				}
				return path;
			}
		}

		[OnInit]
		static void VerifyOnStartup()
		{
			if (_settings == null)
			{
				_settings = LoadSettings();
				Verify(_settings);
			}
		}

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

		static IEnumerable<string> GetDefaultInstallPaths()
		{
			if (SystemInfo.operatingSystem.Contains("Windows"))
			{
				yield return "C:/Program Files (x86)/Steam/steamapps/Common/Hollow Knight";
				yield return "C:/Program Files/Steam/steamapps/Common/Hollow Knight";
				yield return "C:/Steam/steamapps/common/Hollow Knight";
				yield return "C:/Program Files (x86)/GOG Galaxy/Games/Hollow Knight";
				yield return "C:/Program Files/GOG Galaxy/Games/Hollow Knight";
				yield return "C:/GOG Galaxy/Games/Hollow Knight";
			}
			else if (SystemInfo.operatingSystem.Contains("Mac"))
			{
				yield return Environment.GetEnvironmentVariable("HOME") + "/Library/Application Support/Steam/steamapps/common/Hollow Knight/hollow_knight.app";
			}
			else if (SystemInfo.operatingSystem.Contains("Linux"))
			{
				yield return Environment.GetEnvironmentVariable("HOME") + "/.steam/steam/steamapps/common/Hollow Knight";
			}
		}

		/// <summary>
		/// Verifies if the Hollow Knight location is configured correctly
		/// </summary>
		public static void Verify(GameBuildSettings settings)
		{
			var hkDirectory = new DirectoryInfo(settings.HollowKnightLocation);
			var ueDirectory = new DirectoryInfo(settings.UnityEditorLocation);

			bool settingsChanged = false;

			if (!CheckPath(hkDirectory))
			{
				bool validPathFound = false;
				foreach (var defaultPath in GetDefaultInstallPaths())
				{
					if (CheckPath(new DirectoryInfo(defaultPath)))
					{
						hkDirectory = new DirectoryInfo(defaultPath);
						settingsChanged = true;
						validPathFound = true;
						break;
					}
				}
				if (!validPathFound)
				{
					PrintError();
				}
			}

			var currentUEDirectory = new FileInfo(typeof(EditorWindow).Assembly.Location).Directory;

			if (ueDirectory.FullName != currentUEDirectory.FullName)
			{
				settingsChanged = true;
				settings.UnityEditorLocation = currentUEDirectory.FullName;
			}

			if (settingsChanged)
			{
				SaveSettings(settings);
			}

			static bool CheckPath(DirectoryInfo hkDirectory)
			{
				if (!hkDirectory.Exists)
				{
					return false;
				}

				if (SystemInfo.operatingSystem.Contains("Windows") && hkDirectory.Name != "Hollow Knight")
				{
					return false;
				}
				else if (SystemInfo.operatingSystem.Contains("Linux") && hkDirectory.Name != "Hollow Knight")
				{
					return false;
				}
				else if (SystemInfo.operatingSystem.Contains("Mac") && hkDirectory.Name != "hollow_knight.app")
				{
					return false;
				}
				return true;
			}
		}

		private static void PrintError()
		{
			Debug.LogError("The Hollow Knight directory is not configured correctly. Be sure to configure this in WeaverCore/Settings/General Settings");
		}

		static StreamReader GetReader()
		{
			return new StreamReader(File.OpenRead(GameBuildSettingsLocation));
		}

		static StreamWriter GetWriter()
		{
			return new StreamWriter(File.Open(GameBuildSettingsLocation,FileMode.Create,FileAccess.Write));
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
}
