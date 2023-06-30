using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Assets.Components;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Patches
{
	static class HeroController_Player
	{
		[OnInit]
		static void Init()
		{
			On.HeroController.Start += HeroController_Start;
			On.HeroController.Respawn += HeroController_Respawn;
			On.HeroController.LocateSpawnPoint += HeroController_LocateSpawnPoint;
            On.GameManager.OnNextLevelReady += After_FixRespawnType;

            //On.HeroController.Pause += HeroController_Pause;
            //On.HeroController.UnPause += HeroController_UnPause;
		}

        private static void HeroController_UnPause(On.HeroController.orig_UnPause orig, HeroController self)
        {
			WeaverLog.Log(new System.Diagnostics.StackTrace());
			orig(self);
        }

        private static void HeroController_Pause(On.HeroController.orig_Pause orig, HeroController self)
        {
            WeaverLog.Log(new System.Diagnostics.StackTrace());
            orig(self);
        }

        static bool foundBenchWarp = false;

		[AfterModLoad("Benchwarp.Benchwarp")]
		static void BenchwarpLoaded()
		{
            if (!foundBenchWarp)
            {
				foundBenchWarp = true;
				On.GameManager.OnNextLevelReady += Before_FixRespawnType;
			}
		}

		static int originalRespawnType;

		private static void Before_FixRespawnType(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
			originalRespawnType = PlayerData.instance.GetInt("respawnType");
			orig(self);
		}

		//This is to account for the fact that benchwarp does an explicit check on the "spawnPoint" bench object to see if it has an FSM
		//which of course doesn't work with WeaverBenches
        private static void After_FixRespawnType(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
			if (foundBenchWarp && GameManager.instance.RespawningHero)
			{
				Transform spawnPoint = HeroController.instance.LocateSpawnPoint();
				if (spawnPoint != null && spawnPoint.gameObject != null
					&& spawnPoint.gameObject.GetComponent<WeaverBench>() != null)
				{
					PlayerData.instance.respawnType = originalRespawnType;
				}
			}
			orig(self);
		}

        private static Transform HeroController_LocateSpawnPoint(On.HeroController.orig_LocateSpawnPoint orig, HeroController self)
		{
			for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
			{
				var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
				var rootObjs = scene.GetRootGameObjects();
				for (int j = 0; j < rootObjs.GetLength(0); j++)
				{
					var bench = rootObjs[j].GetComponent<WeaverBench>();
					if (bench != null)
					{
						return bench.transform;
					}
				}
			}
			return orig(self);
		}



		internal static void Raise<TEventArgs>(this object source, string eventName, TEventArgs eventArgs) where TEventArgs : EventArgs
		{
			var eventDelegate = (MulticastDelegate)source.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(source);
			if (eventDelegate != null)
			{
				foreach (var handler in eventDelegate.GetInvocationList())
				{
					handler.Method.Invoke(handler.Target, new object[] { source, eventArgs });
				}
			}
		}

		private static IEnumerator HeroController_Respawn(On.HeroController.orig_Respawn orig, HeroController self)
		{
			var iter = orig(self);
			while (iter.MoveNext())
			{
				yield return iter.Current;
			}
			var spawnPoint = self.LocateSpawnPoint();

			if (PlayerData.instance.respawnType == 1 && spawnPoint != null)
			{
				var bench = spawnPoint.GetComponent<WeaverBench>();
				if (bench == null)
				{
					Debug.LogError("HeroCtrl: Could not find WeaverBench on this spawn point, respawn type is set to Bench");
					yield break;
				}
				self.transform.position += bench.BenchSitOffset;
				yield return new WaitForEndOfFrame();
				var eventDelegate = (MulticastDelegate)self.GetType().GetField("heroInPosition", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self);
				if (eventDelegate != null)
				{
					foreach (HeroController.HeroInPosition handler in eventDelegate.GetInvocationList())
					{
						try
						{
							handler(false);
						}
						catch (Exception e)
						{
							Debug.LogException(e);
						}
					}
				}
				self.proxyFSM.SendEvent("HeroCtrl-Respawned");
				ReflectionUtilities.GetMethod<HeroController>("FinishedEnteringScene").Invoke(self, new object[] { true,false});
				bench.RespawnSittingOnBench();
			}
		}

		static void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
		{
			self.gameObject.AddComponent<Player>();
			orig(self);
		}
	}
}
