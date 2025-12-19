using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{
	static class TK2DCamera_Patches
	{
		[OnHarmonyPatch]
		static void Init(HarmonyPatcher patcher)
		{
			if (tk2dCameraType == null)
			{
				Debug.LogError("TK2D camera type not found; skipping TK2DCamera_Patches");
				return;
			}

			var awake = tk2dCameraType.GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
			var prefix = typeof(TK2DCamera_Patches).GetMethod(nameof(Awake_Prefix), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			patcher.Patch(awake, prefix, null);

			var cam = GameObject.FindObjectOfType(tk2dCameraType);
			if (cam != null)
			{
				((MonoBehaviour)cam).gameObject.AddComponent<WeaverCamera>();
			}
		}

		static Type tk2dCameraType = FindType("tk2dCamera");

		static Type FindType(string typeName)
		{
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					var t = asm.GetType(typeName, throwOnError: false, ignoreCase: false);
					if (t != null)
					{
						return t;
					}
				}
				catch { }
			}
			return null;
		}

		static void Awake_Prefix(MonoBehaviour __instance)
		{
			if (__instance != null && __instance.gameObject.GetComponent<WeaverCamera>() == null)
			{
				__instance.gameObject.AddComponent<WeaverCamera>();
			}
		}
	}
}
