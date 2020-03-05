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

				foreach (var bundle in AssetBundle.GetAllLoadedAssetBundles())
				{
					foreach (var material in bundle.LoadAllAssets<Material>())
					{
						if (material.name.Contains("Sprites-Default"))
						{
							DefaultSpriteMaterial = material;
							done = true;
							break;
						}
					}
					if (done)
					{
						break;
					}
				}


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
			foreach (var renderer in gameObject.GetComponents<Renderer>())
			{
				if (renderer.material != null && renderer.material != DefaultSpriteMaterial && renderer.material.name.Contains("Sprites-Default"))
				{
					renderer.material = DefaultSpriteMaterial;
				}
			}
			gameObject.transform.localScale = -gameObject.transform.localScale;
			gameObject.transform.localScale = -gameObject.transform.localScale;
		}
	}
}
