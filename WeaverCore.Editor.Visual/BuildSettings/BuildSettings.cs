using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using WeaverCore.Helpers;

namespace WeaverCore.Editor.Visual
{
	public class BuildSettings
	{
		public string ModName = "";
		public bool WindowsSupport = true;
		public bool MacSupport = false;
		public bool LinuxSupport = false;

		public string HollowKnightDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight";

		public bool StartGame = true;


		public static BuildSettings GetStoredSettings()
		{
			BuildSettings settings;

			if (File.Exists("WeaverBuildSettings.dat"))
			{
				string json = "";
				using (var file = File.Open("WeaverBuildSettings.dat", FileMode.Open))
				{
					using (var reader = new StreamReader(file))
					{
						json = reader.ReadToEnd();
					}
				}
				settings = Json.Deserialize<BuildSettings>(json);
			}
			else
			{
				settings = new BuildSettings();
			}

			settings.Check();

			if (settings.ModName == "")
			{
				settings.ModName = PlayerSettings.productName;
			}

			return settings;
		}

		public static void SetStoredSettings(BuildSettings settings)
		{
			using (var file = File.Create("WeaverBuildSettings.dat"))
			{
				using (var writer = new StreamWriter(file))
				{
					writer.Write(Json.Serialize(settings));
				}
			}
		}

		public void Check()
		{
			if (!HollowKnightDirectory.EndsWith("\\") && !HollowKnightDirectory.EndsWith("/"))
			{
				HollowKnightDirectory += "\\";
			}

			ModName = ModName.Replace(" ", "");
		}

		public bool SupportedBuildMode(BuildMode mode)
		{
			if (mode.Target == BuildTarget.StandaloneWindows)
			{
				return WindowsSupport;
			}
			else if (mode.Target == BuildTarget.StandaloneOSX)
			{
				return MacSupport;
			}
			else if (mode.Target == BuildTarget.StandaloneLinuxUniversal)
			{
				return LinuxSupport;
			}
			return false;
		}
	}
}
