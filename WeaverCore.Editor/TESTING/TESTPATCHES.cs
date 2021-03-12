using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
	static class TESTPATCHES
	{
		/*[OnHarmonyPatch]
		static void Init(HarmonyPatcher patcher)
		{
			WeaverLog.Log("Patch Done!");
			var method = typeof(Transform).GetProperty("position").GetSetMethod();

			WeaverLog.Log("Method = " + method.Name);
			foreach (var param in method.GetParameters())
			{
				WeaverLog.Log("Param = " + param.ParameterType.FullName + ":" + param.Name);
			}
			patcher.Patch(method, typeof(TESTPATCHES).GetMethod("Prefix"), null);

		}


		public static bool Prefix(Transform __instance, Vector3 value)
		{
			if (Vector3.Distance(value,Vector3.zero) < 0.1f)
			{
				WeaverLog.Log("Setting Position From = " + __instance.position);
				WeaverLog.Log("Setting Position To = " + value);
			}

			return true;
		}*/
	}
}
