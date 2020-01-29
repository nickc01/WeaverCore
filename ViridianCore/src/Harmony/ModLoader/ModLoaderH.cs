using Harmony;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ViridianCore.Helpers;

namespace ViridianCore.HarmonyInternal
{

	/*[HarmonyPatch(typeof(HealthManager))]
	[HarmonyPatch("Hit")]
	[HarmonyPatch(new Type[] { typeof(HitInstance) })]*/
	[HarmonyPatch]
	internal class ModLoaderH
	{
		static Type ModLoader;

		public static bool MethodActive = false;

		static MethodInfo TargetMethod()
		{
			ModLoader = typeof(IMod).Assembly.GetType("Modding.ModLoader");
			return ModLoader.GetMethod("UpdateModText",BindingFlags.Static | BindingFlags.NonPublic);
		}

		static bool Prefix()
		{
			MethodActive = true;
			return true;
		}

		static void Postfix()
		{
			MethodActive = false;
		}
	}

	[HarmonyPatch]
	internal class DictionaryKeysH
	{
		static MethodInfo TargetMethod()
		{
			var dictionaryT = typeof(Dictionary<object, object>);
			return dictionaryT.GetProperty("Keys").GetGetMethod();
		}

		static bool Prefix(object __instance)
		{
			if (ModLoaderH.MethodActive && __instance is Dictionary<string, List<IMod>> d)
			{
				if (d.TryGetValue("ViridianLink.Game.Implementations", out var list))
				{
					for (int i = list.Count - 1; i >= 0; i--)
					{
						var item = list[i];
						list.RemoveAt(i);
						var type = item.GetType();
						var addition = "ViridianLink.Game.Implementations." + type.Name;
						if (type.IsGenericType)
						{
							addition += "<";
							foreach (var param in type.GetGenericArguments())
							{
								addition += ";" + param.FullName;
							}
							addition += ">";
						}
						d.Add(addition, new List<IMod>() { item });
					}
					d.Remove("ViridianLink.Game.Implementations");
				}
			}
			return true;
		}

		static void Postfix()
		{

		}
	}
}
