﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Patches
{
	static class TMPROAddComponentPatches
	{
		static Dictionary<Type, Type> TypeReplacements = new Dictionary<Type, Type>();



		[OnInit]
		static void Patch()
		{
			TypeReplacements.Add(typeof(TMPro.TextMeshProUGUI),typeof(WeaverCore.Assets.TMPro.TextMeshProUGUI));
			TypeReplacements.Add(typeof(TMPro.TextMeshPro),typeof(WeaverCore.Assets.TMPro.TextMeshPro));


			var original = typeof(UnityEngine.GameObject).GetMethod("AddComponent", new Type[] { typeof(Type) });
			var prefix = typeof(TMPROAddComponentPatches).GetMethod("AddComponentPrefix", BindingFlags.Public | BindingFlags.Static);

			var patcher = HarmonyPatcher.Create("com.TMProAddComponent.patch");

			patcher.Patch(original, prefix, null);
		}


		public static bool AddComponentPrefix(GameObject __instance, Type componentType, ref object __result)
		{
			if (TypeReplacements.ContainsKey(componentType))
			{
				__result = __instance.AddComponent(TypeReplacements[componentType]);
				return false;
			}
			return true;
		}

	}
}
