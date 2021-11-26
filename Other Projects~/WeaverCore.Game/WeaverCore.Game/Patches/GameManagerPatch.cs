using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine.SceneManagement;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Patches
{
	static class GameManagerPatch
	{
		[OnInit]
		static void Init()
		{
			On.GameManager.SetState += SetStatePatch;
			On.GameManager.BeginSceneTransitionRoutine += GameManager_BeginSceneTransitionRoutine;
			On.GameManager.TransitionScene += GameManager_TransitionScene;
			On.GameManager.ChangeToScene += GameManager_ChangeToScene;
			On.GameManager.WarpToDreamGate += GameManager_WarpToDreamGate;
			On.GameManager.RefreshTilemapInfo += GameManager_RefreshTilemapInfo;
			On.GameManager.SetupSceneRefs += GameManager_SetupSceneRefs;

			On.GameMap.GetTilemapDimensions += GameMap_GetTilemapDimensions;
		}

		private static void GameManager_SetupSceneRefs(On.GameManager.orig_SetupSceneRefs orig, GameManager self, bool refreshTilemapInfo)
		{
			WeaverSceneManager.CurrentSceneManager = null;
			var gm_sm = typeof(GameManager).GetProperty("sm");
			bool found = false;
			for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
			{
				var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
				var rootObjs = scene.GetRootGameObjects();
				for (int j = 0; j < rootObjs.GetLength(0); j++)
				{
					var sm = rootObjs[j].GetComponent<WeaverSceneManager>();
					if (sm != null)
					{
						WeaverSceneManager.CurrentSceneManager = sm;
						self.sceneWidth = sm.SceneDimensions.width;
						self.sceneHeight = sm.SceneDimensions.height;
						gm_sm.SetValue(self, sm);
						found = true;
						break;
					}
					/*if (rootObjs[j].CompareTag("SceneManager"))
					{
						
					}*/
				}
				if (found)
				{
					break;
				}
			}
			orig(self, refreshTilemapInfo);

			var foundSceneManager = gm_sm.GetValue(self) as SceneManager;
			if (foundSceneManager != null && WeaverSceneManager.CurrentSceneManager == null)
			{
				WeaverSceneManager.CurrentSceneManager = foundSceneManager;
			}
		}

		static T GetField<T>(GameMap instance, string name)
		{
			return (T)typeof(GameMap).GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
		}

		static void SetField<T>(GameMap instance, string name, T value)
		{
			typeof(GameMap).GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, value);
		}

		private static void GameMap_GetTilemapDimensions(On.GameMap.orig_GetTilemapDimensions orig, GameMap self)
		{
			//self.originOffsetX = 0f;
			//self.originOffsetY = 0f;
			SetField(self, "originOffsetX", 0f);
			SetField(self, "originOffsetY", 0f);
			SetField(self, "sceneWidth", GameManager.instance.sceneWidth);
			SetField(self, "sceneHeight", GameManager.instance.sceneHeight);
			//self.sceneWidth = GameManager.instance.sceneWidth;
			//self.sceneHeight = GameManager.instance.sceneHeight;
		}

		private static void GameManager_RefreshTilemapInfo(On.GameManager.orig_RefreshTilemapInfo orig, GameManager self, string targetScene)
		{
			for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
			{
				var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
				if (string.IsNullOrEmpty(targetScene) || !(scene.name != targetScene))
				{
					var rootObjs = scene.GetRootGameObjects();
					for (int j = 0; j < rootObjs.GetLength(0); j++)
					{
						var sm = rootObjs[i].GetComponent<WeaverSceneManager>();
						if (sm != null)
						{
							WeaverSceneManager.CurrentSceneManager = sm;
							self.sceneWidth = sm.SceneDimensions.width;
							self.sceneHeight = sm.SceneDimensions.height;
							return;
						}
						/*if (rootObjs[j].CompareTag("SceneManager"))
						{
							
						}*/
					}
				}
			}
			orig(self,targetScene);
		}

		[OnHarmonyPatch]
		static void Patch(HarmonyPatcher patcher)
		{
			var orig = typeof(UnityEngine.SceneManagement.SceneManager).GetMethod("GetSceneByName");
			var pre = typeof(GameManagerPatch).GetMethod("GetSceneByName_Prefix");
			patcher.Patch(orig, pre, null);
		}

		public static bool GetSceneByName_Prefix(string name, ref Scene __result)
		{
			//WeaverLog.Log("Name = " + name);
			if (name.ToLower().Contains(".unity"))
			{
				//WeaverLog.Log("REPLACING");
				__result = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(name);
				return false;
			}
			else
			{
				return true;
			}
		}

		private static void GameManager_WarpToDreamGate(On.GameManager.orig_WarpToDreamGate orig, GameManager self)
		{
			var scene = self.playerData.dreamGateScene;
			UnitySceneManagerPatch.ReplaceScene(ref scene);
			orig(self);
			typeof(GameManager).GetField("targetScene", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(self, scene);
		}

		private static void GameManager_ChangeToScene(On.GameManager.orig_ChangeToScene orig, GameManager self, string targetScene, string entryGateName, float pauseBeforeEnter)
		{
			UnitySceneManagerPatch.ReplaceScene(ref targetScene);
			orig(self, targetScene, entryGateName, pauseBeforeEnter);
		}

		private static System.Collections.IEnumerator GameManager_TransitionScene(On.GameManager.orig_TransitionScene orig, GameManager self, TransitionPoint gate)
		{
			var origSceneName = gate.targetScene;
			UnitySceneManagerPatch.ReplaceScene(ref gate.targetScene);
			var e = orig(self,gate);
			while (e.MoveNext())
			{
				yield return e.Current;
			}
			gate.targetScene = origSceneName;
			//return e;
		}

		private static System.Collections.IEnumerator GameManager_BeginSceneTransitionRoutine(On.GameManager.orig_BeginSceneTransitionRoutine orig, GameManager self, GameManager.SceneLoadInfo info)
		{
			var origSceneName = info.SceneName;
			UnitySceneManagerPatch.ReplaceScene(ref info.SceneName);
			var e = orig(self, info);
			while (e.MoveNext())
			{
				yield return e.Current;
			}
			info.SceneName = origSceneName;
			//return e;
		}

		private static void SetStatePatch(On.GameManager.orig_SetState orig, GameManager self, GlobalEnums.GameState newState)
		{
			orig(self,newState);
			WeaverGameManager.TriggerGameStateChange();
		}
	}
}
