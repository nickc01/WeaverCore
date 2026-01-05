using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Assets;
using WeaverCore.Utilities;
using WeaverCore;

public static class DebugToolsInput_Patches
{
	[OnHarmonyPatch]
	static void Patch(HarmonyPatcher patcher)
	{
		var assembly = ReflectionUtilities.FindLoadedAssembly("Assembly-CSharp");
		if (assembly == null)
		{
			WeaverLog.LogWarning("DebugToolsInput_Patches: Assembly-CSharp not found.");
			return;
		}

		var inputHandlerType = assembly.GetType("InputHandler");
		if (inputHandlerType != null)
		{
			var setCursorEnabled = inputHandlerType.GetMethod("SetCursorEnabled", BindingFlags.NonPublic | BindingFlags.Static);
			var cursorPrefix = typeof(DebugToolsInput_Patches).GetMethod(nameof(SetCursorEnabledPrefix), BindingFlags.NonPublic | BindingFlags.Static);
			patcher.Patch(setCursorEnabled, cursorPrefix, null);
		}
		else
		{
			WeaverLog.LogWarning("DebugToolsInput_Patches: InputHandler type not found.");
		}

		var inputModuleType = assembly.GetType("InControl.HollowKnightInputModule");
		if (inputModuleType != null)
		{
			var processMethod = inputModuleType.GetMethod("Process", BindingFlags.Public | BindingFlags.Instance);
			var processPrefix = typeof(DebugToolsInput_Patches).GetMethod(nameof(HollowKnightInputModuleProcessPrefix), BindingFlags.NonPublic | BindingFlags.Static);
			patcher.Patch(processMethod, processPrefix, null);
		}
		else
		{
			WeaverLog.LogWarning("DebugToolsInput_Patches: HollowKnightInputModule type not found.");
		}
	}

	static void SetCursorEnabledPrefix(ref bool isEnabled)
	{
		if (WeaverCoreDebugTools.IsOpen)
		{
			isEnabled = true;
		}
	}

	static void HollowKnightInputModuleProcessPrefix(object __instance)
	{
		if (!WeaverCoreDebugTools.IsOpen)
		{
			return;
		}

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		if (__instance != null)
		{
			__instance.ReflectSetField("allowMouseInput", true);
		}
	}
}
