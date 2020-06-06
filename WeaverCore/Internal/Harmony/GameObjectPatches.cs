using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static WeaverCore.Helpers.Harmony;
using System.Reflection;
using WeaverCore.Helpers;

namespace WeaverCore.Internal.Harmony
{
	public static class GameObjectPatches
	{
		static bool ranOnce = false;

		static Material DefaultSpriteMaterial;


		static void Initialize()
		{
			if (!ranOnce)
			{
				ranOnce = true;
				bool done = false;
				DefaultSpriteMaterial = WeaverAssetLoader.LoadWeaverAsset<Material>("Sprites-Default");
			}
		}

		public static void Patch(HarmonyInstance harmony)
		{
			MethodInfo postfix = typeof(GameObjectPatches).GetMethod(nameof(AllPostFix), BindingFlags.NonPublic | BindingFlags.Static);

			foreach (var m in typeof(UnityEngine.Object).GetMethods(BindingFlags.Public | BindingFlags.Static))
			{
				var method = m;
				if (method.Name == "Instantiate")
				{
					if (method.ContainsGenericParameters)
					{
						method = method.MakeGenericMethod(typeof(UnityEngine.Object));
					}
					harmony.Patch(method, null, postfix);
				}
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
			if (__result is GameObject gm)
			{
				gameObject = gm;
			}
			else if (__result is Component component)
			{
				gameObject = component.gameObject;
			}
			else
			{
				return;
			}

			List<Renderer> Renderers = new List<Renderer>();

			foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
			{
				if (renderer.sharedMaterial?.shader != null && (renderer.sharedMaterial.shader.name.Contains("Error") || renderer.sharedMaterial.shader.name.Contains("Missing")))
				{
					renderer.sharedMaterial.shader = DefaultSpriteMaterial.shader;
				}
			}

			gameObject.transform.localScale = -gameObject.transform.localScale;
			gameObject.transform.localScale = -gameObject.transform.localScale;
		}
	}
}
