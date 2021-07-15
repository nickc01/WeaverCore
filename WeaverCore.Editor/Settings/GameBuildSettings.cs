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
	public sealed class GameBuildSettings
	{
		public static string GameBuildSettingsLocation = BuildTools.WeaverCoreFolder.AddSlash() + "Other Projects~\\WeaverCore.Game\\Settings.txt";

		public string HollowKnightLocation;
		public string UnityEditorLocation;

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
				//path += "Contents/Resources/Data/Managed/Mods/";
			}
			else if (SystemInfo.operatingSystem.Contains("Linux"))
			{
				yield return Environment.GetEnvironmentVariable("HOME") + "/.steam/steam/steamapps/common/Hollow Knight";
				//path += "hollow_knight_Data/Managed/Mods";
			}
		}

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
			

			/*if (!hkDirectory.Exists || !Directory.Exists(hkDirectory.FullName + "\\hollow_knight_Data"))
			{
				Debug.LogError("The Hollow Knight directory is not configured correctly. Be sure to configure this in WeaverCore/Settings/General Settings");
			}*/

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
			//File.Open(,FileMode.)
			//return new StreamWriter(File.OpenWrite(GameBuildSettingsLocation));
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
