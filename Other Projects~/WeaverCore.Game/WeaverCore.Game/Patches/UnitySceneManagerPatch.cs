using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Patches
{
	public static class UnitySceneManagerPatch
	{
		[OnHarmonyPatch]
		static void Patch(HarmonyPatcher patcher)
		{
			var smType = typeof(UnityEngine.SceneManagement.SceneManager);
			var patchType = typeof(UnitySceneManagerPatch);
			var orig = smType.GetMethod("LoadScene", new Type[] { typeof(string), typeof(LoadSceneParameters) });
			var pre = patchType.GetMethod("LoadScene_Prefix");
			patcher.Patch(orig,pre,null);

			orig = smType.GetMethod("LoadSceneAsync", new Type[] { typeof(string), typeof(LoadSceneParameters) });
			pre = patchType.GetMethod("LoadSceneAsync_Prefix");
			patcher.Patch(orig, pre, null);

			UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;

			On.GameManager.EnterHero += GameManager_EnterHero;
		}

		private static void GameManager_EnterHero(On.GameManager.orig_EnterHero orig, GameManager self, bool additiveGateSearch)
		{
			var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(self.nextSceneName);
			//WeaverLog.Log("EnterHero Scene = " + JsonUtility.ToJson(scene,true));
			foreach (var prop in scene.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				WeaverLog.Log($"{prop.Name} = {prop.GetValue(scene)}");
			}
			WeaverLog.Log("Is Valid = " + scene.IsValid());
			orig(self,additiveGateSearch);
		}

		private static void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
		{
			foreach (var record in Registry.GetAllFeatures<SceneRecord>())
			{
				foreach (var replacement in record.SceneUnions)
				{
					if (replacement.SceneToUnionize == arg0.name || replacement.SceneToUnionize == arg0.path)
					{
						UnityEngine.SceneManagement.SceneManager.LoadScene(replacement.SceneUnion, LoadSceneMode.Additive);
					}
				}
			}
		}

		public static bool LoadScene_Prefix(ref string sceneName)
		{
			ReplaceScene(ref sceneName);
			return true;
			/*WeaverLog.Log("Loading Scene = " + sceneName);
			foreach (var record in Registry.GetAllFeatures<SceneRecord>())
			{
				foreach (var replacement in record.SceneReplacements)
				{
					WeaverLog.Log("Potential Replacement = " + replacement.SceneToReplace + " -> " + replacement.Replacement);
					if (replacement.SceneToReplace == sceneName)
					{
						WeaverLog.Log("Replacing with Scene = " + replacement.Replacement);
						sceneName = replacement.Replacement;
						return true;
					}
				}
			}
			return true;*/
		}

		public static bool LoadSceneAsync_Prefix(ref string sceneName)
		{
			ReplaceScene(ref sceneName);
			return true;
			/*WeaverLog.Log("Loading Scene Async = " + sceneName);
			foreach (var record in Registry.GetAllFeatures<SceneRecord>())
			{
				foreach (var replacement in record.SceneReplacements)
				{
					WeaverLog.Log("Potential Replacement = " + replacement.SceneToReplace + " -> " + replacement.Replacement);
					if (replacement.SceneToReplace == sceneName)
					{
						WeaverLog.Log("Replacing with Scene = " + replacement.Replacement);
						sceneName = replacement.Replacement;
						return true;
					}
				}
			}
			return true;*/
		}

		public static bool ReplaceScene(ref string sceneName)
		{
			WeaverLog.Log("Loading Scene Async = " + sceneName);
			foreach (var record in Registry.GetAllFeatures<SceneRecord>())
			{
				WeaverLog.Log("Record = " + record.name);
				foreach (var replacement in record.SceneReplacements)
				{
					WeaverLog.Log("Potential Replacement = " + replacement.SceneToReplace + " -> " + replacement.Replacement);
					if (replacement.SceneToReplace == sceneName)
					{
						WeaverLog.Log("Replacing with Scene = " + replacement.Replacement);
						sceneName = replacement.Replacement;
						return true;
					}
				}
			}
			return false;
		}
	}
}
