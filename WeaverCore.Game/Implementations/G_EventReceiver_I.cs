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
		/*protected class GMProperties
		{
			public int GameObjectID = 0;
		}

		protected class FSMProperties
		{
			public int FsmID = 0;
		}*/

		protected class EventInfo
		{
			public string eventName;
			public GameObject destination;
		}

		class Patch : IPatch
		{
			void IPatch.Patch(HarmonyPatcher patcher)
			{
				Debugger.Log("DOING EVENT PATCH");
				//EventPrefixMethod = typeof(G_EventReceiver_I).GetMethod(nameof(EventPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				//var IDCheckerMethod = typeof(G_EventReceiver_I).GetMethod(nameof(IDChecker), BindingFlags.Static | BindingFlags.NonPublic);
				//var IDCheckerGMMethod = typeof(G_EventReceiver_I).GetMethod(nameof(IDCheckerGM), BindingFlags.Static | BindingFlags.NonPublic);

				//var eventMethod = typeof(Fsm).GetMethod("Event", new Type[] { typeof(FsmEventTarget), typeof(FsmEvent) });


				//var fsmT = typeof(Fsm);
				//var fsmGMT = typeof(FsmGameObject);

				//patcher.Patch(eventMethod, EventPrefixMethod, null);

				On.HutongGames.PlayMaker.Fsm.Event_FsmEventTarget_FsmEvent += Fsm_Event_FsmEventTarget_FsmEvent;

				//patcher.Patch(fsmT.GetConstructor(new Type[] { }), null, IDCheckerMethod);
				//patcher.Patch(fsmT.GetConstructor(new Type[] { typeof(Fsm), typeof(FsmVariables) }), null, IDCheckerMethod);
				//patcher.Patch(fsmT.GetMethod("Clear"), null, IDCheckerMethod);
				//patcher.Patch(fsmT.GetMethod("Init"), null, IDCheckerMethod);
				//patcher.Patch(fsmT.GetMethod("Init", new Type[] { typeof(MonoBehaviour) }), null, IDCheckerMethod);
				//patcher.Patch(fsmT.GetMethod("OnDestroy"), null, IDCheckerMethod);
				//patcher.Patch(fsmT.GetMethod("Reset"), null, IDCheckerMethod);
				//patcher.Patch(fsmT.GetProperty("Owner").GetSetMethod(), null, IDCheckerMethod);

				/*Debugger.Log("PATCH_A");
				Debugger.Log("IDCHECKERGMMETHOD = " + IDCheckerGMMethod);
				Debugger.Log("Property = " + fsmGMT.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance));
				Debugger.Log("Set Method = " + fsmGMT.GetProperty("Value",BindingFlags.Public | BindingFlags.Instance).GetSetMethod());
				patcher.Patch(fsmGMT.GetProperty("Value").GetSetMethod(), null, IDCheckerGMMethod);
				Debugger.Log("PATCH_B");
				patcher.Patch(fsmGMT.GetProperty("RawValue").GetSetMethod(), null, IDCheckerGMMethod);
				Debugger.Log("PATCH_C");
				patcher.Patch(fsmGMT.GetMethod("SafeAssign"), null, IDCheckerGMMethod);
				Debugger.Log("PATCH_D");
				patcher.Patch(fsmGMT.GetConstructor(new Type[] { typeof(FsmGameObject) }), null, IDCheckerGMMethod);
				Debugger.Log("PATCH_E");*/

			}
		}


		//protected static PropertyTable<GameObject, GMProperties> GameObjectIDs = new PropertyTable<GameObject, GMProperties>();
		//protected static PropertyTable<PlayMakerFSM, GMProperties> FSMIDs = new PropertyTable<PlayMakerFSM, GMProperties>();

		//protected static Dictionary<int, List<GameObject>> GameObjectHooks = new Dictionary<int, List<GameObject>>();

		protected static List<EventInfo> EventHooks = new List<EventInfo>();

		//static PropertyInfo ValueProp = null;

		static MethodInfo EventPrefixMethod;

		public override void Initialize()
		{

		}

		/*static void IDCheckerGM(FsmGameObject __instance, GameObject ___value)
		{
			try
			{
				var gmValues = GameObjectIDs.GetOrCreate(___value);

				gmValues.GameObjectID = ___value.GetInstanceID();

				Debugger.Log("ID for " + ___value + " = " + ___value.GetInstanceID());
			}
			catch (Exception)
			{
				//If we reach here, then ___owner or gameObject is truly null
			}
		}

		static void IDChecker(Fsm __instance, MonoBehaviour ___owner)
		{
			try
			{
				var gmValues = GameObjectIDs.GetOrCreate(___owner.gameObject);
				var fsmValues = FSMIDs.GetOrCreate((PlayMakerFSM)___owner);

				gmValues.GameObjectID = ___owner.gameObject.GetInstanceID();
				fsmValues.GameObjectID = ___owner.gameObject.GetInstanceID();
			}
			catch (Exception)
			{
				//If we reach here, then ___owner or gameObject is truly null
			}
		}*/

		private static void Fsm_Event_FsmEventTarget_FsmEvent(On.HutongGames.PlayMaker.Fsm.orig_Event_FsmEventTarget_FsmEvent orig, Fsm self, FsmEventTarget eventTarget, FsmEvent fsmEvent)
		{
			Debugger.Log("INSIDE PREFIX");

			try
			{
				if (fsmEvent != null)
				{
					foreach (var eventHook in EventHooks)
					{
						if (fsmEvent.Name == eventHook.eventName)
						{
							var receiver = eventHook.destination.GetComponent<EventReceiver>();
							if (receiver != null)
							{
								receiver.ReceiveEvent(fsmEvent.Name, self.GameObject);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Debugger.Log("EVENT PREFIX ERROR = " + e);
			}
			finally
			{
				orig(self, eventTarget, fsmEvent);
			}
		}

		static bool EventPrefix(Fsm __instance, FsmEventTarget eventTarget, FsmEvent fsmEvent)
		{
			try
			{
				//Debugger.Log("RUNNING PREFIX");
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
							receiver.ReceiveEvent(fsmEvent.Name,__instance.GameObject);
						}
					}
				}

				/*int objectID = int.MaxValue;

				if (eventTarget == null)
				{
					return true;
				}

				Debugger.Log("A");

				Debugger.Log("Event Target = " + eventTarget);

				Debugger.Log("Target = " + eventTarget.target);

				if (eventTarget.target == FsmEventTarget.EventTarget.GameObject)
				{
					Debugger.Log("B");
					Debugger.Log("G1 = " + eventTarget.gameObject);
					Debugger.Log("G2 = " + eventTarget.gameObject?.GameObject);
					Debugger.Log("G3 = " + eventTarget.gameObject?.GameObject?.Value);
					try
					{
						objectID = GameObjectIDs.GetOrCreate(eventTarget.gameObject.GameObject.Value).GameObjectID;
						Debugger.Log("FOUND OBJECT ID = " + objectID);
					}
					catch (Exception)
					{
						
					}
					
				}
				else if (eventTarget.target == FsmEventTarget.EventTarget.GameObjectFSM)
				{
					Debugger.Log("C");
					Debugger.Log("G1 = " + eventTarget.gameObject);
					Debugger.Log("G2 = " + eventTarget.gameObject?.GameObject);
					Debugger.Log("G3 = " + eventTarget.gameObject?.GameObject?.Value);
					try
					{
						objectID = GameObjectIDs.GetOrCreate(eventTarget.gameObject.GameObject.Value).GameObjectID;
						Debugger.Log("FOUND OBJECT ID = " + objectID);
					}
					catch (Exception)
					{
						
					}
				}
				else if (eventTarget.target == FsmEventTarget.EventTarget.FSMComponent)
				{
					Debugger.Log("D");
					try
					{
						objectID = FSMIDs.GetOrCreate(eventTarget.fsmComponent).GameObjectID;
					}
					catch (Exception)
					{
						
					}
				}
				else
				{
					Debugger.Log("E");
					return true;
				}

				//var objectID = GameObjectIDs.GetOrCreate(__instance).GameObjectID;

				Debugger.Log("Current Event = " + fsmEvent.Name);
				Debugger.Log("Event Instance ID = " + objectID);

				foreach (var allHooks in GameObjectHooks)
				{
					Debugger.Log("All Hooks Key Instance ID = " + allHooks.Key);
					foreach (var destination in allHooks.Value)
					{
						Debugger.Log("Hook Destination = " + destination.name);
					}
				}

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
				}*/
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

		/*public override void ReceiveEventsFromObject(int instanceID, GameObject destination)
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
		}*/

		public override void ReceiveAllEventsOfName(string name, GameObject destination)
		{
			if (destination == null)
			{
				throw new Exception("The gameObject cannot be null");
			}

			Debugger.Log("Adding object for " + name);

			EventHooks.Add(new EventInfo() { eventName = name, destination = destination });
		}

		public override void RemoveReceiver(GameObject destination)
		{
			EventHooks.RemoveAll(hook => hook.destination == destination);
			/*foreach (var entry in GameObjectHooks)
			{
				if (entry.Value != null)
				{
					entry.Value.Remove(destination);
					//TODO - OPTIMIZE BY REMOVING KEY WHEN LIST IS EMPTY
				}
			}*/
		}

		/*public override void OnReceiveEvent(EventReceiver receiver, string eventName)
		{
			
		}*/
	}
}