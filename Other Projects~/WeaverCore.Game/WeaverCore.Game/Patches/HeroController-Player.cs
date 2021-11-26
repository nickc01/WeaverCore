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
		}

		private static Transform HeroController_LocateSpawnPoint(On.HeroController.orig_LocateSpawnPoint orig, HeroController self)
		{
			for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
			{
				var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
				var rootObjs = scene.GetRootGameObjects();
				for (int j = 0; j < rootObjs.GetLength(0); j++)
				{
					WeaverLog.Log("ROOT OBJ = " + rootObjs[j].name);
					WeaverLog.Log("TAG = " + rootObjs[j].tag);
					//if (rootObjs[j].CompareTag("RespawnPoint"))
					//{

					var bench = rootObjs[j].GetComponent<WeaverBench>();
					if (bench != null)
					{
						WeaverLog.Log("FOUND RESPAWN POINT");
						WeaverLog.Log("RETURNING BENCH");
						return bench.transform;
					}
					//}
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
				//TODO
				var bench = spawnPoint.GetComponent<WeaverBench>();
				if (bench == null)
				{
					Debug.LogError("HeroCtrl: Could not find WeaverBench on this spawn point, respawn type is set to Bench");
					yield break;
				}
				bench.RespawnResting = true;
				self.transform.position += bench.BenchSitOffset;
				yield return null;

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
						//handler.Method.Invoke(handler.Target, new object[] { source, eventArgs });
					}
				}

				self.proxyFSM.SendEvent("HeroCtrl-Respawned");
				ReflectionUtilities.GetMethod<HeroController>("FinishedEnteringScene").Invoke(self, new object[] { true,false});
				bench.Respawn();

				//Raise(self,"heroInPosition",)
				/*if (self.heroInPosition != null)
				{
					self.heroInPosition();
				}*/
			}
		}

		static void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
		{
			self.gameObject.AddComponent<Player>();
			orig(self);
		}
	}
}
