using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using System.Reflection;
using WeaverCore.Utilities;
using WeaverCore.Interfaces;
using WeaverCore.Attributes;

namespace WeaverCore.Internal.Harmony
{
	public static class GameObjectPatches
	{
		/*class Patch : IPatch
		{
			void IPatch.Patch(HarmonyPatcher patcher)
			{
				
			}
		}*/

		/*[OnHarmonyPatch]
		static void ApplyPatches(HarmonyPatcher patcher)
		{
			MethodInfo postfix = typeof(GameObjectPatches).GetMethod("AllPostFix", BindingFlags.NonPublic | BindingFlags.Static);

			foreach (var m in typeof(UnityEngine.Object).GetMethods(BindingFlags.Public | BindingFlags.Static))
			{
				var method = m;
				if (method.Name == "Instantiate")
				{
					if (method.ContainsGenericParameters)
					{
						method = method.MakeGenericMethod(typeof(UnityEngine.Object));
					}
					//patcher.Patch(method, null, postfix);
				}
			}
		}


		static bool ranOnce = false;

		static Material DefaultSpriteMaterial;


		static void Initialize()
		{
			if (!ranOnce)
			{
				ranOnce = true;
				bool done = false;
				DefaultSpriteMaterial = WeaverAssets.LoadWeaverAsset<Material>("Sprites-Default");
			}
		}


		static void AllPostFix(UnityEngine.Object __result)
		{
			Initialize();

			if (__result == null)
			{
				return;
			}

			GameObject gameObject = null;
			if (__result is GameObject)
			{
				gameObject = __result as GameObject;
			}
			else if (__result is Component)
			{
				gameObject = (__result as Component).gameObject;
			}
			else
			{
				return;
			}

			List<Renderer> Renderers = new List<Renderer>();

			foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
			{
				if (renderer.sharedMaterial != null && renderer.sharedMaterial.shader != null && (renderer.sharedMaterial.shader.name.Contains("Error") || renderer.sharedMaterial.shader.name.Contains("Missing")))
				{
					renderer.sharedMaterial.shader = DefaultSpriteMaterial.shader;
				}
			}

			gameObject.transform.localScale = -gameObject.transform.localScale;
			gameObject.transform.localScale = -gameObject.transform.localScale;
		}*/
	}
}
