using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{

	[Serializable]
	public class BuildSettings : ConfigSettings
	{
		public string ModName = PlayerSettings.productName;
		public bool WindowsSupport = true;
		public bool MacSupport = false;
		public bool LinuxSupport = false;
		public bool StartGame = true;


		public IEnumerable<BuildTarget> GetBuildModes()
		{
			if (WindowsSupport)
			{
				if (!PlatformUtilities.IsPlatformSupportLoaded(BuildTarget.StandaloneWindows))
				{
					throw new Exception("Attempting to build for the windows platform, but Unity currently has no support for building windows builds");
				}
				yield return BuildTarget.StandaloneWindows;
			}
			if (MacSupport)
			{
				if (!PlatformUtilities.IsPlatformSupportLoaded(BuildTarget.StandaloneOSX))
				{
					throw new Exception("Attempting to build for the mac platform, but Unity currently has no support for building mac builds");
				}
				yield return BuildTarget.StandaloneOSX;
			}
			if (LinuxSupport)
			{
				if (!PlatformUtilities.IsPlatformSupportLoaded(BuildTarget.StandaloneLinuxUniversal))
				{
					throw new Exception("Attempting to build for the linux platform, but Unity currently has no support for building linux builds");
				}
				yield return BuildTarget.StandaloneLinuxUniversal;
			}
		}
	}


	/*[Serializable]
	public class BuildSettingsOLD
	{
		public string ModName = "";
		public bool WindowsSupport = true;
		public bool MacSupport = false;
		public bool LinuxSupport = false;

		public string HollowKnightDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight";

		public bool StartGame = true;


		public static BuildSettingsOLD GetStoredSettings()
		{
			BuildSettingsOLD settings;

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
				//settings = Json.Deserialize<BuildSettings>(json);
				settings = JsonUtility.FromJson<BuildSettingsOLD>(json);
			}
			else
			{
				settings = new BuildSettingsOLD();
			}

			settings.Check();

			if (settings.ModName == "")
			{
				settings.ModName = PlayerSettings.productName;
			}

			return settings;
		}

		public static void SetStoredSettings(BuildSettingsOLD settings)
		{
			using (var file = File.Create("WeaverBuildSettings.dat"))
			{
				using (var writer = new StreamWriter(file))
				{
					//writer.Write(Json.Serialize(settings));
					writer.Write(JsonUtility.ToJson(settings));
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
	}*/
}
