using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverCore.Helpers;

namespace WeaverTools.Patches
{
	internal static class ObjectPrinterPatch
	{
		internal static void HealthManager_Start(On.HealthManager.orig_Start orig, HealthManager self)
		{
			var settings = ToolSettings.GetSettings();
			if (settings.DebugMode)
			{
				ObjectDebugger.DebugObject(self.gameObject);
			}
			if (!ObjectDebugger.ObjectBeingDebugged(self.gameObject))
			{
				orig(self);
			}
		}

		internal static void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
		{
			var settings = ToolSettings.GetSettings();
			if (settings.DebugMode)
			{
				//WeaverCore.Debugger.Log("Printing Player");
				ObjectDebugger.DebugObject(self.gameObject);
			}
			if (!ObjectDebugger.ObjectBeingDebugged(self.gameObject))
			{
				orig(self);
			}
		}

		internal static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadMode)
		{
			var settings = ToolSettings.GetSettings();
			if (settings.DebugMode)
			{
				//var sceneDirectory = ObjectDebugger.DebugDirectory.CreateSubdirectory("Scenes").CreateSubdirectory(scene.name);
				//var componentsDir = sceneDirectory.CreateSubdirectory("Components");
				//var objectsDir = sceneDirectory.CreateSubdirectory("Objects");

				//objectsDir.Create();

				Dictionary<Type, Component> SceneComponents = new Dictionary<Type, Component>();
				foreach (var gameObject in scene.GetRootGameObjects())
				{
					/*foreach (var component in gameObject.GetComponents<Component>())
					{
						var type = component.GetType();
						if (!SceneComponents.ContainsKey(type))
						{
							SceneComponents.Add(type, component);
						}
					}*/
					ObjectDebugger.DebugObject(gameObject);
				}
				/*componentsDir.Create();

				foreach (var component in SceneComponents)
				{
					ObjectDebugger.DebugComponent(component.Value, componentsDir);
				}*/
			}
		}
	}
}
