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

		public string BuildLocation = GameBuildSettings.GetSettings().HollowKnightLocation + "\\hollow_knight_Data\\Managed\\Mods";

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
}
