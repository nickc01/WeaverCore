using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using Harmony;

namespace WeaverCore.Game.Implementations
{
	public class G_EventReceiver_I : EventReceiver_I
	{
		protected class FsmProperties
		{
			public int GameObjectID = 0;
		}

		protected class EventInfo
		{
			public string eventName;
			public GameObject destination;
		}

		class Patch : IPatch
		{
			void IPatch.Patch(HarmonyPatcher patcher)
			{
				EventPrefixMethod = typeof(G_EventReceiver_I).GetMethod(nameof(EventPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				var IDCheckerMethod = typeof(G_EventReceiver_I).GetMethod(nameof(IDChecker), BindingFlags.Static | BindingFlags.NonPublic);

				var eventMethod = typeof(Fsm).GetMethod("Event", new Type[] { typeof(FsmEventTarget), typeof(FsmEvent) });


				var fsmT = typeof(Fsm);

				patcher.Patch(eventMethod, EventPrefixMethod, null);

				patcher.Patch(fsmT.GetConstructor(new Type[] { }), null, IDCheckerMethod);
				patcher.Patch(fsmT.GetConstructor(new Type[] { typeof(Fsm), typeof(FsmVariables) }), null, IDCheckerMethod);
				patcher.Patch(fsmT.GetMethod("Clear"), null, IDCheckerMethod);
				patcher.Patch(fsmT.GetMethod("Init"), null, IDCheckerMethod);
				patcher.Patch(fsmT.GetMethod("Init", new Type[] { typeof(MonoBehaviour) }), null, IDCheckerMethod);
				patcher.Patch(fsmT.GetMethod("OnDestroy"), null, IDCheckerMethod);
				patcher.Patch(fsmT.GetMethod("Reset"), null, IDCheckerMethod);
				patcher.Patch(fsmT.GetProperty("Owner").GetSetMethod(), null, IDCheckerMethod);
			}
		}


		protected static PropertyTable<Fsm, FsmProperties> GameObjectIDs = new PropertyTable<Fsm, FsmProperties>();

		protected static Dictionary<int, List<GameObject>> GameObjectHooks = new Dictionary<int, List<GameObject>>();

		protected static List<EventInfo> EventHooks = new List<EventInfo>();

		static PropertyInfo ValueProp = null;

		static MethodInfo EventPrefixMethod;

		public override void Initialize()
		{

		}

		static void IDChecker(Fsm __instance, MonoBehaviour ___owner)
		{
			var values = GameObjectIDs.GetOrCreate(__instance);
			try
			{
				values.GameObjectID = ___owner.gameObject.GetInstanceID();
			}
			catch (NullReferenceException)
			{
				values.GameObjectID = 0;
			}
		}

		static bool EventPrefix(Fsm __instance, FsmEventTarget eventTarget, FsmEvent fsmEvent)
		{
			try
			{
				if (fsmEvent == null)
				{
					return true;
				}
				foreach (var eventHook in EventHooks)
				{
					if (fsmEvent.Name == eventHook.eventName)
					{
						var receiver = eventHook.destination.GetComponent<EventReceiver>();
						if (receiver != null)
						{
							receiver.ReceiveEvent(fsmEvent.Name);
						}
					}
				}

				var objectID = GameObjectIDs.GetOrCreate(__instance).GameObjectID;

				if (GameObjectHooks.TryGetValue(objectID,out var list) && list != null)
				{
					foreach (var destination in list)
					{
						var receiver = destination.GetComponent<EventReceiver>();
						if (receiver != null)
						{
							receiver.ReceiveEvent(fsmEvent.Name);
						}
					}
				}
				return true;
			}
			catch (Exception e)
			{
				Debugger.Log("EVENT PREFIX ERROR = " + e);
			}
			return true;
		}

		/*public override void SendEvent(GameObject destination, string eventName)
		{
			var fsmObject = destination.GetComponent<PlayMakerFSM>();

			if (fsmObject != null && fsmObject.Fsm != null)
			{
				SendEvent(fsmObject.Fsm, eventName,destination);
			}
		}*/

		/*void SendEvent(Fsm fsm, string eventName,GameObject destination)
		{
			var target = new FsmEventTarget()
			{
				target = FsmEventTarget.EventTarget.GameObject,
				gameObject = new FsmOwnerDefault()
				{
					GameObject = destination,
					OwnerOption = OwnerDefaultOption.UseOwner
				},
				sendToChildren = false,
				excludeSelf = false
			};

			var fsmEvent = new FsmEvent(eventName);

			fsm.Event(target, eventName);
		}*/

		/*public override void BroadcastEvent(string eventName)
		{
			foreach (var fsmObject in GameObject.FindObjectsOfType<PlayMakerFSM>())
			{
				if (fsmObject.Fsm != null)
				{
					SendEvent(fsmObject.Fsm,eventName,fsmObject.gameObject);
				}
			}
		}*/

		public override void ReceiveEventsFromObject(int instanceID, GameObject destination)
		{
			List<GameObject> HookList;

			if (!GameObjectHooks.ContainsKey(instanceID))
			{
				GameObjectHooks.Add(instanceID, HookList = new List<GameObject>());
			}
			else
			{
				HookList = GameObjectHooks[instanceID];
			}
			if (!HookList.Contains(destination))
			{
				HookList.Add(destination);
			}

			if (destination.GetComponent<EventReceiver>() == null)
			{
				destination.AddComponent<EventReceiver>();
			}
		}

		public override void ReceiveAllEventsOfName(string name, GameObject destination)
		{
			if (destination == null)
			{
				throw new Exception("The gameObject cannot be null");
			}

			EventHooks.Add(new EventInfo() { eventName = name, destination = destination });
		}

		public override void RemoveReceiver(GameObject destination)
		{
			EventHooks.RemoveAll(hook => hook.destination == destination);
			foreach (var entry in GameObjectHooks)
			{
				if (entry.Value != null)
				{
					entry.Value.Remove(destination);
					//TODO - OPTIMIZE BY REMOVING KEY WHEN LIST IS EMPTY
				}
			}
		}

		public override void OnReceiveEvent(EventReceiver receiver, string eventName)
		{
			
		}
	}
}