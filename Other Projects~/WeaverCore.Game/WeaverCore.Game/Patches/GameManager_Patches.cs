using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Patches
{
	static class GameManager_Patches
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

            On.GameManager.Awake += GameManager_Awake;
		}

        private static void GameManager_Awake(On.GameManager.orig_Awake orig, GameManager self)
        {
			self.GetType().GetField("verboseMode", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(self, true);

			orig(self);
        }

        /*private static void GameManager_SetupSceneRefs(On.GameManager.orig_SetupSceneRefs orig, GameManager self, bool refreshTilemapInfo)
		{
			WeaverLog.Log("PATCH_A");
			WeaverSceneManager.CurrentSceneManager = null;
			WeaverLog.Log("PATCH_B");
			var gm_sm = typeof(GameManager).GetProperty("sm");
			WeaverLog.Log("PATCH_C = " + gm_sm);
			bool found = false;
			WeaverLog.Log("PATCH_D");
			for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
			{
				WeaverLog.Log("PATCH_E = " + i);
				var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
				WeaverLog.Log("PATCH_F = " + scene);
				var rootObjs = scene.GetRootGameObjects();
				WeaverLog.Log("PATCH_G = " + rootObjs);
				for (int j = 0; j < rootObjs.GetLength(0); j++)
				{
					WeaverLog.Log("PATCH_H = " + rootObjs[j]);
					var sm = rootObjs[j].GetComponent<WeaverSceneManager>();
					WeaverLog.Log("PATCH_I = " + sm);
					if (sm != null)
					{
						WeaverLog.Log("PATCH_J");
						WeaverSceneManager.CurrentSceneManager = sm;
						WeaverLog.Log("PATCH_K");
						WeaverLog.Log("PATCH_L = " + sm.SceneDimensions);
						self.sceneWidth = sm.SceneDimensions.width;
						WeaverLog.Log("PATCH_M");
						self.sceneHeight = sm.SceneDimensions.height;
						WeaverLog.Log("PATCH_N");
						gm_sm.SetValue(self, sm);
						WeaverLog.Log("PATCH_O");
						found = true;
						WeaverLog.Log("PATCH_P");
						break;
					}
				}
				if (found)
				{
					break;
				}
			}
			WeaverLog.Log("PATCH_Q");
			orig(self, refreshTilemapInfo);
			WeaverLog.Log("PATCH_R");

			var foundSceneManager = gm_sm.GetValue(self) as SceneManager;
			WeaverLog.Log("PATCH_S = " + foundSceneManager);
			if (foundSceneManager != null && WeaverSceneManager.CurrentSceneManager == null)
			{
				WeaverLog.Log("PATCH_T");
				WeaverSceneManager.CurrentSceneManager = foundSceneManager;
			}
			WeaverLog.Log("PATCH_U");
		}*/

        private static void GameManager_SetupSceneRefs(On.GameManager.orig_SetupSceneRefs orig, GameManager self, bool refreshTilemapInfo)
		{
			Debug.Log("SETTING UP SCENE REFS");
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
				}
				if (found)
				{
					break;
				}
			}
			try
            {
				orig(self, refreshTilemapInfo);
			}
			catch (Exception e)
            {
				Debug.Log("SETUP SCENE REFS EXCEPTION");
				Debug.LogException(e);
                if (GameManager.instance.IsGameplayScene() && refreshTilemapInfo)
                {
					GameManager.instance.RefreshTilemapInfo(GameManager.instance.sceneName);
                }
            }

			var foundSceneManager = gm_sm.GetValue(self) as SceneManager;
			if (foundSceneManager != null && WeaverSceneManager.CurrentSceneManager == null)
			{
				WeaverSceneManager.CurrentSceneManager = foundSceneManager;
			}

			Debug.Log("Current Scene = " + JsonUtility.ToJson(foundSceneManager,true));
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
			SetField(self, "originOffsetX", 0f);
			SetField(self, "originOffsetY", 0f);
			SetField(self, "sceneWidth", GameManager.instance.sceneWidth);
			SetField(self, "sceneHeight", GameManager.instance.sceneHeight);
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
					}
				}
			}
			orig(self,targetScene);
		}

		[OnHarmonyPatch]
		static void Patch(HarmonyPatcher patcher)
		{
			var orig = typeof(UnityEngine.SceneManagement.SceneManager).GetMethod("GetSceneByName");
			var pre = typeof(GameManager_Patches).GetMethod("GetSceneByName_Prefix");
			patcher.Patch(orig, pre, null);
		}

		public static bool GetSceneByName_Prefix(string name, ref Scene __result)
		{
			if (name.ToLower().Contains(".unity"))
			{
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
			UnitySceneManager_Patches.ReplaceScene(ref scene);
			orig(self);
			typeof(GameManager).GetField("targetScene", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(self, scene);
		}

		private static void GameManager_ChangeToScene(On.GameManager.orig_ChangeToScene orig, GameManager self, string targetScene, string entryGateName, float pauseBeforeEnter)
		{
			UnitySceneManager_Patches.ReplaceScene(ref targetScene);
			orig(self, targetScene, entryGateName, pauseBeforeEnter);
		}

		private static System.Collections.IEnumerator GameManager_TransitionScene(On.GameManager.orig_TransitionScene orig, GameManager self, TransitionPoint gate)
		{
			var origSceneName = gate.targetScene;
			UnitySceneManager_Patches.ReplaceScene(ref gate.targetScene);
			var e = orig(self,gate);
			while (e.MoveNext())
			{
				yield return e.Current;
			}
			gate.targetScene = origSceneName;
		}

		private static System.Collections.IEnumerator GameManager_BeginSceneTransitionRoutine(On.GameManager.orig_BeginSceneTransitionRoutine orig, GameManager self, GameManager.SceneLoadInfo info)
		{
			var origSceneName = info.SceneName;
			UnitySceneManager_Patches.ReplaceScene(ref info.SceneName);
			var e = orig(self, info);
			while (e.MoveNext())
			{
				yield return e.Current;
			}
			info.SceneName = origSceneName;
		}

		private static void SetStatePatch(On.GameManager.orig_SetState orig, GameManager self, GlobalEnums.GameState newState)
		{
			orig(self,newState);
			WeaverGameManager.TriggerGameStateChange();
		}
	}
}
