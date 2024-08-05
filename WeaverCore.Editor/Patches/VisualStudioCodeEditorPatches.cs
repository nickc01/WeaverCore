using System;
using System.IO;
using System.Reflection;
using System.Text;
using WeaverCore.Attributes;

namespace WeaverCore.Editor.Patches
{
    static class VisualStudioCodeEditorPatches
	{
		[OnHarmonyPatch]
		static void OnHarmonyPatch(HarmonyPatcher patcher)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assembly.GetName().Name == "Unity.VisualStudio.Editor")
				{
					var installType = assembly.GetType("Microsoft.Unity.VisualStudio.Editor.VisualStudioCodeInstallation");
					
					if (installType != null)
					{
						/*{
							//TryDiscoverInstallation
							var orig = installType.GetMethod("TryDiscoverInstallation", BindingFlags.Public | BindingFlags.Static);
							var postfix = typeof(TestPatches).GetMethod(nameof(TryDiscoverInstallationPostfix), BindingFlags.NonPublic | BindingFlags.Static);
							patcher.Patch(orig, null, postfix);
						}*/

						{
							//ProcessStartInfoFor
							var orig = installType.GetMethod("ProcessStartInfoFor", BindingFlags.NonPublic | BindingFlags.Static);
							var prefix = typeof(VisualStudioCodeEditorPatches).GetMethod(nameof(ProcessStartInfoForPrefix), BindingFlags.NonPublic | BindingFlags.Static);
							patcher.Patch(orig, prefix, null);
						}
					}

					break;
				}
			}
		}

		static bool IsWaylandSessionRunning()
		{
			return Environment.GetEnvironmentVariable("WAYLAND_DISPLAY").Contains("wayland");
		}

		static bool ProcessStartInfoForPrefix(object __instance, ref string application, ref string arguments)
		{
			if (IsWaylandSessionRunning() && !arguments.StartsWith("--ozone-platform-hint"))
			{
				arguments = "--ozone-platform-hint=wayland --enable-wayland-ime --use-gl=egl " + arguments;
			}
			return true;
		}

		static void TryDiscoverInstallationPostfix(object __instance, ref object installation)
		{
			StringBuilder output = new StringBuilder("Installation = ");

			foreach (var field in installation.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
			{
				output.AppendLine($" ---- {field.Name} = {field.GetValue(installation)}");
			}

			foreach (var field in installation.GetType().BaseType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
			{
				output.AppendLine($" ---- {field.Name} = {field.GetValue(installation)}");
			}

			WeaverLog.Log(output.ToString());
			//WeaverLog.Log("INSTALLATION = " + installation.ToString());
		}

	}
}
