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

    public static class UnitySceneManager_Patches
	{
		[OnHarmonyPatch]
		static void Patch(HarmonyPatcher patcher)
		{
			var smType = typeof(UnityEngine.SceneManagement.SceneManager);
			var patchType = typeof(UnitySceneManager_Patches);
			var orig = smType.GetMethod("LoadScene", new Type[] { typeof(string), typeof(LoadSceneParameters) });
			var pre = patchType.GetMethod("LoadScene_Prefix");
			patcher.Patch(orig,pre,null);

			orig = smType.GetMethod("LoadSceneAsync", new Type[] { typeof(string), typeof(LoadSceneParameters) });
			pre = patchType.GetMethod("LoadSceneAsync_Prefix");
			patcher.Patch(orig, pre, null);

			UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
		}

		static Dictionary<string, Scene> unionizedScenes = new Dictionary<string, Scene>();

		private static void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
		{
			Debug.Log("LOADED SCENE = " + arg0.name);
			if (unionizedScenes.ContainsKey(arg0.path))
			{
				var destination = unionizedScenes[arg0.path];
				unionizedScenes.Remove(arg0.path);
				try
				{
                    Debug.Log($"MERGING {arg0.path} with {destination.path}");
					UnityEngine.SceneManagement.SceneManager.MergeScenes(arg0, destination);
				}
				catch (Exception e)
				{
					UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(arg0);
				}
			}
			else
			{
				foreach (var record in Registry.GetAllFeatures<SceneRecord>())
				{
					foreach (var replacement in record.SceneUnions)
					{
						if (replacement.SceneToUnionize == arg0.name || replacement.SceneToUnionize == arg0.path)
						{
							unionizedScenes.Add(replacement.SceneUnion, arg0);
                            Debug.Log($"LOADING SCENE UNION {replacement.SceneUnion} to combine with {arg0.path}");
                            UnityEngine.SceneManagement.SceneManager.LoadScene(replacement.SceneUnion, LoadSceneMode.Additive);
							//var loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(replacement.SceneUnion);
							//UnityEngine.SceneManagement.SceneManager.MergeScenes(loadedScene, arg0);
						}
					}
				}
			}
		}

		public static bool LoadScene_Prefix(ref string sceneName)
		{
			ReplaceScene(ref sceneName);
			return true;
		}

		public static bool LoadSceneAsync_Prefix(ref string sceneName)
		{
			ReplaceScene(ref sceneName);
			return true;
		}

		public static bool ReplaceScene(ref string sceneName)
		{
			foreach (var record in Registry.GetAllFeatures<SceneRecord>())
			{
				foreach (var replacement in record.SceneReplacements)
				{
					if (replacement.SceneToReplace == sceneName)
					{
						sceneName = replacement.Replacement;
						return true;
					}
				}
			}
			return false;
		}
	}
}
