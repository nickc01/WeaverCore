using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;

namespace WeaverCore.Editor.Utilities
{
	/// <summary>
	/// Contains several utility functions related to build platforms
	/// </summary>
	public static class PlatformUtilities
	{
		/// <summary>
		/// Gets the file extension for a target build platform
		/// </summary>
		public static string GetBuildTargetExtension(BuildTarget target)
		{
			switch (target)
			{
				case BuildTarget.StandaloneWindows:
					return ".bundle.win";
				case BuildTarget.StandaloneOSX:
					return ".bundle.mac";
				case BuildTarget.StandaloneLinux64:
					return ".bundle.unix";
				default:
					return null;
			}
		}

		/// <summary>
		/// Tests if a build target is available
		/// </summary>
		/// <param name="buildTarget">The build target to test</param>
		/// <returns>Returns whether this build target is supported or not</returns>
		public static bool IsPlatformSupportLoaded(BuildTarget buildTarget)
		{
			var UnityEditor = System.Reflection.Assembly.Load("UnityEditor");
			var ModuleManagerT = UnityEditor.GetType("UnityEditor.Modules.ModuleManager");

			var buildString = (string)ModuleManagerT.GetMethod("GetTargetStringFromBuildTarget", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { buildTarget });
			return (bool)ModuleManagerT.GetMethod("IsPlatformSupportLoaded", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { buildString });
		}

		/// <summary>
		/// Returns all the supported build targets that this Unity Editor supports
		/// </summary>
		public static IEnumerable<BuildTarget> GetPCBuildTargets()
		{
			if (IsPlatformSupportLoaded(BuildTarget.StandaloneWindows))
			{
				yield return BuildTarget.StandaloneWindows;
			}
			if (IsPlatformSupportLoaded(BuildTarget.StandaloneOSX))
			{
				yield return BuildTarget.StandaloneOSX;
			}
			if (IsPlatformSupportLoaded(BuildTarget.StandaloneLinux64))
			{
				yield return BuildTarget.StandaloneLinux64;
			}
		}
	}
}
