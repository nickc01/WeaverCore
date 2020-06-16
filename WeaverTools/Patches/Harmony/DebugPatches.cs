using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using WeaverCore.Utilities;

namespace WeaverTools.Patches
{
	public static class DebugPatches
	{
		public static void ApplyPatches(HarmonyPatcher instance)
		{
			MethodInfo postFix = typeof(DebugPatches).GetMethod(nameof(Postfix), BindingFlags.Static | BindingFlags.NonPublic);

			foreach (var method in typeof(Debug).GetMethods(BindingFlags.Public | BindingFlags.Static))
			{
				if (!method.IsAbstract && method.Name.Contains("Log"))
				{
					var parameters = method.GetParameters();
					if (parameters != null && parameters.GetLength(0) >= 1 && parameters[0].Name == "message")
					{
						instance.Patch(method, null, postFix);
					}
				}
			}
		}

		static void Postfix(object message, MethodInfo __originalMethod)
		{
			if (ToolSettings.GetSettings().PrintUnityDebugLogs)
			{
				Modding.Logger.Log($"[{__originalMethod.DeclaringType.FullName + "." + __originalMethod.Name}] - {message}");
			}
		}
	}
}
