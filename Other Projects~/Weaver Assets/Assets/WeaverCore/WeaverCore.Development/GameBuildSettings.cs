using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Editor
{
	internal class GameBuildSettings
	{
		class Verifier : IInit
		{
			public void OnInit()
			{
				VerifySettings();
			}
		}

		//static string fileLocation = null;

		public static string BuildSettingsFileLocation
		{
			get
			{

				if (WeaverAssetsInfo.InWeaverAssetsProject)
				{
					return new FileInfo("..\\WeaverCore.Game\\Settings.txt").FullName;
				}
				else
				{
					return new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\Settings.txt").FullName;
				}
				/*if (fileLocation == null)
				{
					fileLocation = new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\Settings.txt").FullName;
				}
				return fileLocation;*/
			}
			/*set
			{
				fileLocation = value;
			}*/
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
	}
}
